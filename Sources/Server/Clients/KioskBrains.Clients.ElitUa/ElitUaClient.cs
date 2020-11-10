using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Helpers.Text;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.Extensions.Options;
using MimeKit;
using OfficeOpenXml;

namespace KioskBrains.Clients.ElitUa
{
    public class ElitUaClient
    {
        private readonly ElitUaClientSettings _settings;

        public ElitUaClient(
            IOptions<ElitUaClientSettings> settings)
        {
            _settings = settings.Value;
            Assure.ArgumentNotNull(_settings, nameof(_settings));
        }

        public async Task<ElitPriceList> GetUnappliedPriceListAsync(
            string lastAppliedPriceListId,
            CancellationToken cancellationToken)
        {
            var lastEmailData = await GetLastPriceListEmailAsync(lastAppliedPriceListId, cancellationToken);

            if (lastEmailData == null)
            {
                return ElitPriceList.GetForError("No email.");
            }

            if (lastAppliedPriceListId == lastEmailData.Uid)
            {
                return new ElitPriceList()
                    {
                        IsSuccess = true,
                        PriceListId = lastEmailData.Uid,
                        StatusMessage = $"Price list with Id '{lastEmailData.Uid}' is already applied.",
                    };
            }

            if (lastEmailData.AttachmentBytes == null
                || lastEmailData.AttachmentBytes.Length == 0)
            {
                return ElitPriceList.GetForError("Last email has no attachment.");
            }

            // new price list is available
            var priceList = new ElitPriceList()
                {
                    IsSuccess = true,
                    PriceListId = lastEmailData.Uid,
                    Records = new List<ElitPriceListRecord>(),
                };
            var statusMessageBuilder = new StringBuilder($"New price list with Id '{lastEmailData.Uid}' is available ({lastEmailData.AttachmentBytes.Length:N} bytes).");
            using (var excelPackage = new ExcelPackage())
            {
                using (var xlsxStream = new MemoryStream(lastEmailData.AttachmentBytes))
                {
                    excelPackage.Load(xlsxStream);
                }

                var worksheet = excelPackage.Workbook.Worksheets.First();

                var validRecordsCount = 0;
                var invalidRecordsCount = 0;

                // skip 2 header rows
                for (var rowNumber = 3; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                {
                    var rowCells = worksheet.Cells[rowNumber, 1, rowNumber, worksheet.Dimension.End.Column];
                    var record = new ElitPriceListRecord()
                        {
                            ActiveItemNo = rowCells[rowNumber, 1].Text,
                            PartNumber = rowCells[rowNumber, 2].Text,
                            Brand = rowCells[rowNumber, 3].Text,
                            ItemDescription = rowCells[rowNumber, 7].Text,
                            EcatDescription = rowCells[rowNumber, 8].Text,
                            CustomerPrice = rowCells[rowNumber, 9].Text.ParseDecimalOrNull() ?? 0m,
                        };
                    if (record.IsValid())
                    {
                        priceList.Records.Add(record);
                        validRecordsCount++;
                    }
                    else
                    {
                        invalidRecordsCount++;
                    }
                }

                statusMessageBuilder.Append($" Valid: {validRecordsCount}, invalid: {invalidRecordsCount}.");
            }

            priceList.StatusMessage = statusMessageBuilder.ToString();

            return priceList;
        }

        private async Task<EmailData> GetLastPriceListEmailAsync(
            string uidToIgnore,
            CancellationToken cancellationToken)
        {
            using (var imapClient = new ImapClient())
            {
                // we don't care if server has valid ssl certificate
                imapClient.ServerCertificateValidationCallback = (s, c, h, e) => true;

                // hardcoded to Gmail
                await imapClient.ConnectAsync("imap.gmail.com", 993, true, cancellationToken);

                // remove any OAuth functionality as we won't be using it
                // ReSharper disable StringLiteralTypo
                imapClient.AuthenticationMechanisms.Remove("XOAUTH2");
                // ReSharper restore StringLiteralTypo

                await imapClient.AuthenticateAsync(_settings.EmailAccount, _settings.EmailPassword, cancellationToken);

                await imapClient.Inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

                var lastMessages = await imapClient.Inbox.FetchAsync(
                    imapClient.Inbox.Count - 1,
                    -1,
                    MessageSummaryItems.UniqueId,
                    cancellationToken);
                var lastMessage = lastMessages.FirstOrDefault();
                if (lastMessage == null)
                {
                    return null;
                }

                var lastMessageUid = lastMessage.UniqueId.ToString();
                if (lastMessageUid == uidToIgnore)
                {
                    return new EmailData()
                        {
                            Uid = lastMessageUid,
                        };
                }

                var message = await imapClient.Inbox.GetMessageAsync(lastMessage.UniqueId, cancellationToken);
                var attachments = message.Attachments?.ToArray() ?? new MimeEntity[0];
                if (attachments.Length == 0)
                {
                    return new EmailData()
                        {
                            Uid = lastMessageUid,
                        };
                }

                byte[] attachmentBytes;
                var attachment = attachments[0];
                using (var stream = new MemoryStream())
                {
                    if (attachment is MessagePart messagePart)
                    {
                        await messagePart.Message.WriteToAsync(stream, cancellationToken);
                    }
                    else
                    {
                        var mimePart = (MimePart)attachment;
                        await mimePart.Content.DecodeToAsync(stream, cancellationToken);
                    }

                    attachmentBytes = stream.ToArray();
                }

                await imapClient.DisconnectAsync(true, cancellationToken);

                return new EmailData()
                    {
                        Uid = lastMessageUid,
                        AttachmentBytes = attachmentBytes,
                    };
            }
        }
    }
}
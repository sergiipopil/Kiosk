using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KioskBrains.Common.Contracts;
using KioskBrains.Kiosk.Core.Storage;
using KioskBrains.Kiosk.Helpers.Threads;

namespace KioskBrains.Kiosk.Core.Devices.Documents
{
    public class DocumentManager
    {
        #region Singleton

        public static DocumentManager Current { get; } = new DocumentManager();

        private DocumentManager()
        {
        }

        #endregion

        private const string DocumentsRecordName = "Documents";

        private readonly StatePersistenceManager<DocumentsStateModel> _statePersistenceManager
            = new StatePersistenceManager<DocumentsStateModel>(
                DocumentsRecordName,
                state =>
                    {
                        if (state == null
                            || !state.IsValid())
                        {
                            // create new if state doesn't exist or is invalid
                            state = new DocumentsStateModel()
                                {
                                    CurrentNumbers = new Dictionary<string, int>(),
                                };
                        }

                        return state;
                    });

        public Task<int> GetNextDocumentNumberAsync(string documentName)
        {
            // todo: sync/block parallel invocations

            Assure.ArgumentNotNull(documentName, nameof(documentName));

            return ThreadHelper.RunInBackgroundThreadAsync(async () =>
                {
                    // read current state
                    var state = await _statePersistenceManager.LoadAsync(context: null);

                    // increment document number
                    if (!state.CurrentNumbers.ContainsKey(documentName))
                    {
                        state.CurrentNumbers[documentName] = 0;
                    }

                    var documentNumber = state.CurrentNumbers[documentName];
                    documentNumber++;
                    state.CurrentNumbers[documentName] = documentNumber;

                    // save new document number
                    await _statePersistenceManager.SaveAsync(state, context: null);

                    return documentNumber;
                });
        }

        private const string SecretKeySymbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public string GenerateReceiptAuthCode(int length = 6)
        {
            return GenerateSecretKey(length);
        }

        public string GenerateSecretKey(int length)
        {
            var random = new Random();
            var secretKeyBuilder = new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                secretKeyBuilder.Append(SecretKeySymbols[random.Next(SecretKeySymbols.Length)]);
            }

            return secretKeyBuilder.ToString();
        }
    }
}
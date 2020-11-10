using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using KioskBrains.Common.Contracts;

namespace KioskBrains.Kiosk.CommonNet.Helpers
{
    public static class FileHelper
    {
        public static async Task SaveFileAsync(string filePath, string fileContent)
        {
            Assure.ArgumentNotNull(filePath, nameof(filePath));
            Assure.ArgumentNotNull(fileContent, nameof(fileContent));

            // .temp in order to prevent reading before write completion
            var tempFilePath = filePath + ".temp";
            var messageBytes = Encoding.UTF8.GetBytes(fileContent);
            using (var fileStream = new FileStream(tempFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await fileStream.WriteAsync(messageBytes, 0, messageBytes.Length);
            }

            // rename ready file
            File.Move(tempFilePath, filePath);
        }

        public static async Task<string> ReadFileAsync(string filePath)
        {
            var tryNumber = 1;
            while (true)
            {
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: 4096, useAsync: true))
                    {
                        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                        {
                            var message = await streamReader.ReadToEndAsync();
                            return message;
                        }
                    }
                }
                catch (IOException)
                {
                    // sometimes the file is still not released - try 1 more time with small delay
                    if (tryNumber == 1)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                    }
                    else
                    {
                        throw;
                    }
                }
                tryNumber++;
            }
        }

        public static Task DeleteFileAsync(string filePath)
        {
            Assure.ArgumentNotNull(filePath, nameof(filePath));

            return Task.Run(() => File.Delete(filePath));
        }
    }
}
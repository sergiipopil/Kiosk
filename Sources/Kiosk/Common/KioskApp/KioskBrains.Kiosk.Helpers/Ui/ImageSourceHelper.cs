using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using KioskBrains.Common.Contracts;
using KioskBrains.Kiosk.Helpers.Threads;

namespace KioskBrains.Kiosk.Helpers.Ui
{
    public static class ImageSourceHelper
    {
        public static async Task<BitmapImage> FromBytes(byte[] imageBytes)
        {
            Assure.ArgumentNotNull(imageBytes, nameof(imageBytes));

            using (var inMemoryRandomAccessStream = new InMemoryRandomAccessStream())
            {
                await inMemoryRandomAccessStream.WriteAsync(imageBytes.AsBuffer());
                inMemoryRandomAccessStream.Seek(0);
                BitmapImage bitmapImage = null;
                await ThreadHelper.RunInUiThreadAsync(async () =>
                    {
                        bitmapImage = new BitmapImage();
                        // ReSharper disable AccessToDisposedClosure
                        await bitmapImage.SetSourceAsync(inMemoryRandomAccessStream);
                        // ReSharper restore AccessToDisposedClosure
                    });
                return bitmapImage;
            }
        }
    }
}
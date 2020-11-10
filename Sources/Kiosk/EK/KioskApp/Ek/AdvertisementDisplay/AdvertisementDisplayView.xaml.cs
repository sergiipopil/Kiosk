using System;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Core.Storage;

namespace KioskApp.Ek.AdvertisementDisplay
{
    public sealed partial class AdvertisementDisplayView : UserControl
    {
        public AdvertisementDisplayView()
        {
            InitializeComponent();
        }

        private void AdvertisementDisplayView_OnLoaded(object sender, RoutedEventArgs e)
        {
#pragma warning disable 4014
            InitializeVideoPlayerAsync();
#pragma warning restore 4014
        }

        private async Task InitializeVideoPlayerAsync()
        {
            try
            {
                AdvertisementVideoPlayer.MediaPlayer.IsLoopingEnabled = true;

                var kioskRoot = await StorageHelper.GetKioskRootFolderAsync();
                var videoFolder = await kioskRoot.GetFolderAsync("Video");
                var videoFile = await videoFolder.GetFileAsync("AdVideo.mp4");

                AdvertisementVideoPlayer.MediaPlayer.Source = MediaSource.CreateFromStorageFile(videoFile);
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.Application, "Initialization of advertisement player failed.", ex);
            }
        }
    }
}
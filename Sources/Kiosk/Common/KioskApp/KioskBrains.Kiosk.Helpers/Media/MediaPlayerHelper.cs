namespace KioskBrains.Kiosk.Helpers.Media
{
    /// <summary>
    /// Allows for disposal of the underlying MediaSources attached to a MediaPlayer, regardless
    /// of if a MediaSource or MediaPlaybackItem was passed to the MediaPlayer.
    ///
    /// It is left to the app to implement a clean-up of the other possible IMediaPlaybackSource
    /// type, which is a MediaPlaybackList.
    ///
    /// </summary>
    public static class MediaPlayerHelper
    {
        public static void CleanUpMediaPlayerSource(Windows.Media.Playback.MediaPlayer mediaPlayer)
        {
            if (mediaPlayer?.Source != null)
            {
                var source = mediaPlayer.Source as Windows.Media.Core.MediaSource;
                source?.Dispose();

                var item = mediaPlayer.Source as Windows.Media.Playback.MediaPlaybackItem;
                item?.Source?.Dispose();

                mediaPlayer.Source = null;
            }
        }
    }
}
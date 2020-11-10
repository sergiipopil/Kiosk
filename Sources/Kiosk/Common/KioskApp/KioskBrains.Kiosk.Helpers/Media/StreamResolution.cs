using System;
using Windows.Media.MediaProperties;

namespace KioskBrains.Kiosk.Helpers.Media
{
    /// <summary>
    /// Wrapper class around IMediaEncodingProperties to help how devices report supported resolutions
    /// </summary>
    public class StreamResolution
    {
        public StreamResolution(IMediaEncodingProperties properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            // Only handle ImageEncodingProperties and VideoEncodingProperties, which are the two types that GetAvailableMediaStreamProperties can return
            if (!(properties is ImageEncodingProperties) && !(properties is VideoEncodingProperties))
            {
                throw new ArgumentException("Argument is of the wrong type. Required: " + typeof(ImageEncodingProperties).Name
                                                                                        + " or " + typeof(VideoEncodingProperties).Name + ".", nameof(properties));
            }

            // Store the actual instance of the IMediaEncodingProperties for setting them later
            EncodingProperties = properties;
        }

        public uint Width
        {
            get
            {
                if (EncodingProperties is ImageEncodingProperties imageEncodingProperties)
                {
                    return imageEncodingProperties.Width;
                }

                if (EncodingProperties is VideoEncodingProperties videoEncodingProperties)
                {
                    return videoEncodingProperties.Width;
                }

                return 0;
            }
        }

        public uint Height
        {
            get
            {
                if (EncodingProperties is ImageEncodingProperties imageEncodingProperties)
                {
                    return imageEncodingProperties.Height;
                }

                if (EncodingProperties is VideoEncodingProperties videoEncodingProperties)
                {
                    return videoEncodingProperties.Height;
                }

                return 0;
            }
        }

        public uint FrameRate
        {
            get
            {
                if (EncodingProperties is VideoEncodingProperties videoEncodingProperties)
                {
                    if (videoEncodingProperties.FrameRate.Denominator != 0)
                    {
                        return videoEncodingProperties.FrameRate.Numerator/videoEncodingProperties.FrameRate.Denominator;
                    }
                }

                return 0;
            }
        }

        public uint BitRate
        {
            get
            {
                if (EncodingProperties is VideoEncodingProperties videoEncodingProperties)
                {
                    return videoEncodingProperties.Bitrate;
                }

                return 0;
            }
        }

        public double AspectRatio => Math.Round((Height != 0) ? (Width/(double)Height) : double.NaN, 2);

        public IMediaEncodingProperties EncodingProperties { get; }

        /// <summary>
        /// Output properties to a readable format for UI purposes
        /// eg. 1920x1080 [1.78] 30fps MPEG
        /// </summary>
        /// <returns>Readable string</returns>
        public string GetFriendlyName(bool showFrameRate = true)
        {
            if (EncodingProperties is ImageEncodingProperties ||
                !showFrameRate)
            {
                return Width + "x" + Height + " [" + AspectRatio + "] " + EncodingProperties.Subtype;
            }

            if (EncodingProperties is VideoEncodingProperties)
            {
                return Width + "x" + Height + " [" + AspectRatio + "] " + FrameRate + "FPS " + EncodingProperties.Subtype;
            }

            return string.Empty;
        }
    }
}
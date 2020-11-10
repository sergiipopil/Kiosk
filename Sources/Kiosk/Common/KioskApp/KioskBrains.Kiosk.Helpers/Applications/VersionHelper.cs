using System.Collections.Generic;
using Windows.ApplicationModel;

namespace KioskBrains.Kiosk.Helpers.Applications
{
    public static class VersionHelper
    {
        static VersionHelper()
        {
            var version = Package.Current.Id.Version;

            var versionParts = new List<int>
                {
                    version.Major,
                    version.Minor
                };

            if (version.Build > 0)
            {
                versionParts.Add(version.Build);
            }

            if (version.Revision > 0)
            {
                versionParts.Add(version.Revision);
            }

            AppVersionString = string.Join(".", versionParts);
        }

        public static string AppVersionString { get; }
    }
}
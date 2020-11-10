using System.Collections.Generic;
using System.Reflection;

namespace KioskBrains.Kiosk.CommonNet.Helpers
{
    public class VersionHelper
    {
        public static string GetEntryAssemblyVersion()
        {
            var version = Assembly.GetEntryAssembly().GetName().Version;

            var versionParts = new List<int>
                {
                    version.Major,
                    version.Minor
                };
            if (version.Revision > 0)
            {
                versionParts.Add(version.Revision);
            }
            if (version.Build > 0)
            {
                versionParts.Add(version.Build);
            }

            return string.Join(".", versionParts);
        }
    }
}
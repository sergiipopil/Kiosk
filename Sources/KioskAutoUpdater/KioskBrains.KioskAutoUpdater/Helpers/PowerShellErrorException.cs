using System;

namespace KioskBrains.KioskAutoUpdater.Helpers
{
    public class PowerShellErrorException : Exception
    {
        public PowerShellErrorException(PowerShellResponse response)
            : base($"PowerShell script failed (errors: '{response.ErrorOutput}').")
        {
        }
    }
}
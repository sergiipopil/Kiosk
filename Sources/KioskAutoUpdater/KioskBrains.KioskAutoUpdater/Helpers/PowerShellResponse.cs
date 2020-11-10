using System.Linq;
using System.Management.Automation;
using KioskBrains.Common.Contracts;

namespace KioskBrains.KioskAutoUpdater.Helpers
{
    public class PowerShellResponse
    {
        public bool HadErrors { get; set; }

        public string ErrorOutput { get; set; }

        public PowerShellResponse(PowerShell powerShellInstance)
        {
            Assure.ArgumentNotNull(powerShellInstance, nameof(powerShellInstance));

            HadErrors = powerShellInstance.HadErrors;
            if (HadErrors)
            {
                ErrorOutput = string.Join("\n", powerShellInstance.Streams.Error.Select(x => $"{x.ToString()}\n{x.InvocationInfo?.PositionMessage}"));
            }
        }
    }
}
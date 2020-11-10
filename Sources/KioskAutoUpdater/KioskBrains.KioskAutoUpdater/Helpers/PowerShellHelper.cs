using System;
using System.Management.Automation;
using System.Threading.Tasks;

namespace KioskBrains.KioskAutoUpdater.Helpers
{
    public static class PowerShellHelper
    {
        /// <summary>
        /// Important! Host should be built in x64.
        /// </summary>
        public static async Task<PowerShellResponse> RunScriptsAsync(params string[] scripts)
        {
            using (var powerShellInstance = PowerShell.Create(RunspaceMode.NewRunspace))
            {
                // no prompts
                var runspaceInvoke = new RunspaceInvoke(powerShellInstance.Runspace);
                runspaceInvoke.Invoke("Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy ByPass -Force");

                foreach (var script in scripts)
                {
                    powerShellInstance.AddScript(script);
                }

                // todo: add timeout
                var invocationTask = Task.Factory.FromAsync(powerShellInstance.BeginInvoke(), (Func<IAsyncResult, PSDataCollection<PSObject>>)powerShellInstance.EndInvoke);
                await invocationTask;

                return new PowerShellResponse(powerShellInstance);
            }
        }
    }
}
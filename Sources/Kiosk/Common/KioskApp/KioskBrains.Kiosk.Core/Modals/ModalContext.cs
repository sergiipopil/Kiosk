using System;
using System.Threading.Tasks;
using KioskBrains.Kiosk.Helpers.Threads;

namespace KioskBrains.Kiosk.Core.Modals
{
    public class ModalContext
    {
        /// <summary>
        /// Invoked in UI thread.
        /// </summary>
        internal Action OnCloseModalRequest { get; set; }

        internal bool IsEarlyClosingRequested { get; set; }

        public Task CloseModalAsync()
        {
            return ThreadHelper.RunInUiThreadAsync(() =>
                {
                    if (OnCloseModalRequest == null)
                    {
                        IsEarlyClosingRequested = true;
                    }
                    else
                    {
                        OnCloseModalRequest();
                    }
                });
        }
    }
}
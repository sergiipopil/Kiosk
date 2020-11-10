using System;
using Windows.UI.Xaml.Controls;
using KioskBrains.Common.Contracts;

namespace KioskBrains.Kiosk.Core.Modals
{
    public class ModalArgs
    {
        public string ModalTitle { get; }

        public Func<ModalContext, UserControl> ModalContentProvider { get; }

        public bool CenterTitleHorizontally { get; set; }

        public bool ShowCancelButton { get; }

        /// <param name="modalTitle">Modal window title.</param>
        /// <param name="modalContentProvider">Invoked in UI thread.</param>
        /// <param name="centerTitleHorizontally">Center-align Title Horizontally?</param>
        /// <param name="showCancelButton">Show Cancel button?</param>
        public ModalArgs(string modalTitle, Func<ModalContext, UserControl> modalContentProvider, bool centerTitleHorizontally = false, bool showCancelButton = true)
        {
            Assure.ArgumentNotNull(modalTitle, nameof(modalTitle));
            Assure.ArgumentNotNull(modalContentProvider, nameof(modalContentProvider));

            ModalTitle = modalTitle;
            ModalContentProvider = modalContentProvider;
            CenterTitleHorizontally = centerTitleHorizontally;
            ShowCancelButton = showCancelButton;
        }
    }
}
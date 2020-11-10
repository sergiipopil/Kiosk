using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace KioskBrains.Kiosk.Helpers.Ui.Extensions
{
    /// <summary>
    /// Handles the ClipToBounds attached behavior defined by the attached property
    /// of the <see cref="FrameworkElementExtensions"/> class.
    /// </summary>
    public class ClipToBoundsHandler
    {
        private FrameworkElement _fe;

        /// <summary>
        /// Attaches to the specified framework element.
        /// </summary>
        /// <param name="fe">The fe.</param>
        public void Attach(FrameworkElement fe)
        {
            _fe = fe;
            UpdateClipGeometry();
            fe.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            if (_fe == null)
                return;

            UpdateClipGeometry();
        }

        private void UpdateClipGeometry()
        {
            _fe.Clip =
                new RectangleGeometry
                    {
                        Rect = new Rect(0, 0, _fe.ActualWidth, _fe.ActualHeight)
                    };
        }

        /// <summary>
        /// Detaches this instance.
        /// </summary>
        public void Detach()
        {
            if (_fe == null)
                return;

            _fe.SizeChanged -= OnSizeChanged;
            _fe = null;
        }
    }
}
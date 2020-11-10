using Windows.UI.Xaml.Controls;
using KioskBrains.Common.Contracts;

namespace KioskApp.CoreExtension.Application
{
    public sealed partial class KioskApplicationView : UserControl
    {
        public KioskApplicationView(KioskApplication model)
        {
            Assure.ArgumentNotNull(model, nameof(model));

            Model = model;

            InitializeComponent();

            PointerReleased += (sender, args) =>
                {
                    // prevents automatic loss of focus
                    args.Handled = true;
                };
        }

        public KioskApplication Model { get; }

        public Panel GetModalLayer()
        {
            return ModalLayer;
        }
    }
}
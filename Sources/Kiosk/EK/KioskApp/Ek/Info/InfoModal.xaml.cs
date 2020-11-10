using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskBrains.Common.Contracts;
using KioskBrains.Kiosk.Core.Modals;

namespace KioskApp.Ek.Info
{
    public sealed partial class InfoModal : UserControl
    {
        private readonly ModalContext _modalContext;

        public InfoModal(ModalContext modalContext)
        {
            Assure.ArgumentNotNull(modalContext, nameof(modalContext));

            _modalContext = modalContext;

            InitializeComponent();
        }

        #region TitlePart1

        public static readonly DependencyProperty TitlePart1Property = DependencyProperty.Register(
            nameof(TitlePart1), typeof(string), typeof(InfoModal), new PropertyMetadata(default(string)));

        public string TitlePart1
        {
            get => (string)GetValue(TitlePart1Property);
            set => SetValue(TitlePart1Property, value);
        }

        #endregion

        #region TitlePart2

        public static readonly DependencyProperty TitlePart2Property = DependencyProperty.Register(
            nameof(TitlePart2), typeof(string), typeof(InfoModal), new PropertyMetadata(default(string)));

        public string TitlePart2
        {
            get => (string)GetValue(TitlePart2Property);
            set => SetValue(TitlePart2Property, value);
        }

        #endregion

        #region InfoBlocks

        public static readonly DependencyProperty InfoBlocksProperty = DependencyProperty.Register(
            nameof(InfoBlocks), typeof(InfoBlock[]), typeof(InfoModal), new PropertyMetadata(default(InfoBlock[])));

        public InfoBlock[] InfoBlocks
        {
            get => (InfoBlock[])GetValue(InfoBlocksProperty);
            set => SetValue(InfoBlocksProperty, value);
        }

        #endregion

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _modalContext.CloseModalAsync();
        }
    }
}
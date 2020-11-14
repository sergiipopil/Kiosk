using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using KioskApp.Ek.Info;
using KioskApp.Search;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Core.Modals;
using System.Threading;

namespace KioskApp.Ek.Products
{
    public sealed partial class ProductDescriptionPresenter : UserControl
    {
        public ProductDescriptionPresenter()
        {
            FontSize = 14;

            InitializeComponent();

            TextElement.RegisterPropertyChangedCallback(
                TextBlock.IsTextTrimmedProperty,
                (sender, dp) => UpdateFullDescriptionLinkVisibility());
        }

        #region MaxLines

        public static readonly DependencyProperty MaxLinesProperty = DependencyProperty.Register(
            nameof(MaxLines), typeof(int), typeof(ProductDescriptionPresenter), new PropertyMetadata(default(int)));

        public int MaxLines
        {
            get => (int)GetValue(MaxLinesProperty);
            set => SetValue(MaxLinesProperty, value);
        }

        #endregion

        #region Product

        public static readonly DependencyProperty ProductProperty = DependencyProperty.Register(
            nameof(Product), typeof(Product), typeof(ProductDescriptionPresenter), new PropertyMetadata(default(Product)));

        public Product Product
        {
            get => (Product)GetValue(ProductProperty);
            set => SetValue(ProductProperty, value);
        }

        #endregion

        #region IsExtended

        public static readonly DependencyProperty IsExtendedProperty = DependencyProperty.Register(
            nameof(IsExtended), typeof(bool), typeof(ProductDescriptionPresenter), new PropertyMetadata(default(bool), IsExtendedPropertyChangedCallback));

        private static void IsExtendedPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProductDescriptionPresenter)d).UpdateFullDescriptionLinkVisibility();
        }

        public bool IsExtended
        {
            get => (bool)GetValue(IsExtendedProperty);
            set => SetValue(IsExtendedProperty, value);
        }

        #endregion

        #region ShowFullDescriptionLink

        public static readonly DependencyProperty ShowFullDescriptionLinkProperty = DependencyProperty.Register(
            nameof(ShowFullDescriptionLink), typeof(bool), typeof(ProductDescriptionPresenter), new PropertyMetadata(default(bool)));

        public bool ShowFullDescriptionLink
        {
            get => (bool)GetValue(ShowFullDescriptionLinkProperty);
            set => SetValue(ShowFullDescriptionLinkProperty, value);
        }

        #endregion

        private void UpdateFullDescriptionLinkVisibility()
        {
            ShowFullDescriptionLink = IsExtended && TextElement.IsTextTrimmed;
        }

        private void ShowFullDescriptionLink_OnTapped(object sender, TappedRoutedEventArgs e)
        { 
            e.Handled = true;
            Product.RequestDescriptionTranslate();
            Thread.Sleep(100);
            InfoBlock[] parsedDescription;
            try
            {
                parsedDescription = InfoModalHelper.ParseInfoText(Product.Parameters + "\n" + Product.Description);
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.Application, $"Product description failed (product '{Product.Key}').", ex);
                return;
            }

#pragma warning disable 4014
            ModalManager.Current.ShowModalAsync(new ModalArgs(
#pragma warning restore 4014
                "ProductDescription",
                context => new InfoModal(context)
                    {
                        TitlePart1 = Product.Name,
                        InfoBlocks = parsedDescription,
                    },
                showCancelButton: false));
        }
    }
}
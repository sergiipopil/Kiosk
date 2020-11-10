using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KioskApp.Ek.Products
{
    public sealed partial class ResultCountPresenter : UserControl
    {
        public ResultCountPresenter()
        {
            InitializeComponent();
        }

        #region Count

        public static readonly DependencyProperty CountProperty = DependencyProperty.Register(
            nameof(Count), typeof(long?), typeof(ResultCountPresenter), new PropertyMetadata(default(long?), CountPropertyChangedCallback));

        private static void CountPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ResultCountPresenter)d).OnCountChanged();
        }

        public long? Count
        {
            get => (long?)GetValue(CountProperty);
            set => SetValue(CountProperty, value);
        }

        #endregion

        #region CountString

        public static readonly DependencyProperty CountStringProperty = DependencyProperty.Register(
            nameof(CountString), typeof(string), typeof(ResultCountPresenter), new PropertyMetadata(default(string)));

        public string CountString
        {
            get => (string)GetValue(CountStringProperty);
            set => SetValue(CountStringProperty, value);
        }

        #endregion

        private void OnCountChanged()
        {
            if (Count == null)
            {
                CountString = null;
            }
            else if (Count.Value > 1000)
            {
                CountString = "1000+";
            }
            else
            {
                CountString = Count.ToString();
            }
        }
    }
}
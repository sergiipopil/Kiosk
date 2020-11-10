using Windows.UI.Xaml;
using KioskBrains.Common.Helpers.Text;

namespace KioskBrains.Kiosk.Core.Ui.Controls
{
    public class IntegerInput : TextInput
    {
        protected override int DefaultMaxLength => 9;

        #region Value

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(int?), typeof(IntegerInput), new PropertyMetadata(default(int?), ValueChangedCallback));

        private static void ValueChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((IntegerInput)dependencyObject).OnValueChanged(
                dependencyPropertyChangedEventArgs.OldValue as int?,
                dependencyPropertyChangedEventArgs.NewValue as int?);
        }

        public int? Value
        {
            get => (int?)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        #endregion

        // no need to lock, changed/checked only in UI thread
        private bool _textOrValueChangeHandling;

        protected override void OnTextChanged(string previousValue, string currentValue)
        {
            if (!_textOrValueChangeHandling)
            {
                if (!_textOrValueChangeHandling)
                {
                    _textOrValueChangeHandling = true;
                    try
                    {
                        Value = Text.ParseIntOrNull();
                    }
                    finally
                    {
                        _textOrValueChangeHandling = false;
                    }
                }
            }
        }

        protected virtual void OnValueChanged(int? previousValue, int? currentValue)
        {
            if (!_textOrValueChangeHandling)
            {
                _textOrValueChangeHandling = true;
                try
                {
                    Text = Value?.ToString();
                }
                finally
                {
                    _textOrValueChangeHandling = false;
                }
            }
        }

        public override void AddText(string textAddition)
        {
            if (Text == "0")
            {
                Text = null;
            }

            base.AddText(textAddition);
        }
    }
}
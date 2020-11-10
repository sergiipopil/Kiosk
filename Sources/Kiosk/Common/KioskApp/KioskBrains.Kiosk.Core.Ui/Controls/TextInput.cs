using Windows.UI.Xaml;

namespace KioskBrains.Kiosk.Core.Ui.Controls
{
    public class TextInput : VirtualKeyboardTargetBase
    {
        public TextInput()
        {
            DefaultStyleKey = typeof(TextInput);
        }

        #region MaxLength

        public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register(
            nameof(MaxLength), typeof(int), typeof(TextInput), new PropertyMetadata(default(int)));

        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        #endregion

        protected virtual int DefaultMaxLength => 0;

        #region TextMargin

        public static readonly DependencyProperty TextMarginProperty = DependencyProperty.Register(
            nameof(TextMargin), typeof(Thickness), typeof(TextInput), new PropertyMetadata(default(Thickness)));

        /// <summary>
        /// Margin to align text with a blinking cursor (if it's required by design).
        /// </summary>
        public Thickness TextMargin
        {
            get => (Thickness)GetValue(TextMarginProperty);
            set => SetValue(TextMarginProperty, value);
        }

        #endregion

        #region BlinkingCursorStyle

        public static readonly DependencyProperty BlinkingCursorStyleProperty = DependencyProperty.Register(
            nameof(BlinkingCursorStyle), typeof(Style), typeof(TextInput), new PropertyMetadata(default(Style)));

        public Style BlinkingCursorStyle
        {
            get => (Style)GetValue(BlinkingCursorStyleProperty);
            set => SetValue(BlinkingCursorStyleProperty, value);
        }

        #endregion

        public override void AddText(string textAddition)
        {
            var maxLength = MaxLength;
            if (maxLength <= 0)
            {
                maxLength = DefaultMaxLength;
            }

            var newText = Text + textAddition;
            if (maxLength > 0
                && newText.Length > maxLength)
            {
                newText = newText.Substring(0, maxLength);
            }

            Text = newText;
        }
    }
}
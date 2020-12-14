using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskBrains.Kiosk.Core.Ui.Controls
{
    public abstract class VirtualKeyboardTargetBase : Control, IVirtualKeyboardControlTarget
    {
        #region Text

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(VirtualKeyboardTargetBase), new PropertyMetadata(default(string), TextChangedCallback));

        private static void TextChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((VirtualKeyboardTargetBase)dependencyObject).OnTextChanged(
                new PropertyChangedEventArgs<string>(
                    dependencyPropertyChangedEventArgs.OldValue as string,
                    dependencyPropertyChangedEventArgs.NewValue as string));
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        #endregion

        public event EventHandler<PropertyChangedEventArgs<string>> TextChanged;

        protected virtual void OnTextChanged(PropertyChangedEventArgs<string> e)
        {
            OnTextChanged(e.OldValue, e.NewValue);
            TextChanged?.Invoke(this, e);
        }

        protected virtual void OnTextChanged(string previousValue, string currentValue)
        {
        }

        public virtual void AddText(string textAddition)
        {
            Text = Text + textAddition;
        }

        public virtual void ProcessBackspace()
        {
            var currentText = Text;
            if (currentText?.Length > 0)
            {
                Text = currentText.Substring(0, currentText.Length - 1);
            }
        }
        public virtual void ProcessLeftArrow()
        {
            var options = new FindNextElementOptions()
            {
                //SearchRoot = TicTacToeGrid,
                XYFocusNavigationStrategyOverride = XYFocusNavigationStrategyOverride.Projection
            };
            DependencyObject candidate = null;
            candidate = FocusManager.FindNextElement(
                        FocusNavigationDirection.Left, options);
            if (candidate != null && candidate is Control)
            {
                (candidate as Control).Focus(FocusState.Keyboard);
            }
            var currentText = Text;
            
        }
        
        public virtual void ProcessRightArrow()
        {
            var currentText = Text;
            if (currentText?.Length > 0)
            {
                Text = currentText.Substring(0, currentText.Length - 1);
            }
        }

        #region RelatedKeyboard

        public static readonly DependencyProperty RelatedKeyboardProperty = DependencyProperty.Register(
            nameof(RelatedKeyboard), typeof(VirtualKeyboardControl), typeof(VirtualKeyboardTargetBase), new PropertyMetadata(default(VirtualKeyboardControl)));

        public VirtualKeyboardControl RelatedKeyboard
        {
            get => (VirtualKeyboardControl)GetValue(RelatedKeyboardProperty);
            set => SetValue(RelatedKeyboardProperty, value);
        }

        #endregion

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            if (RelatedKeyboard != null)
            {
                RelatedKeyboard.Target = this;
            }
        }
    }
}
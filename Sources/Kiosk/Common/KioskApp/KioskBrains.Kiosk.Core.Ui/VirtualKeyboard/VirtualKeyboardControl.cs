using System;
using System.Windows.Input;
using Windows.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Core.Languages;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Keys;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts.Numbers;
using KioskBrains.Kiosk.Helpers.Threads;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskBrains.Kiosk.Core.Ui.VirtualKeyboard
{
    public sealed class VirtualKeyboardControl : ContentControl
    {
        public VirtualKeyboardControl()
        {
            DefaultStyleKey = typeof(VirtualKeyboardControl);

            KeyPressedCommand = new RelayCommand(
                nameof(KeyPressedCommand),
                parameter => OnKeyPressed(parameter as VirtualKey));

            Loaded += VirtualKeyboardControl_Loaded;
            Unloaded += VirtualKeyboardControl_Unloaded;
        }

        private void VirtualKeyboardControl_Loaded(object sender, RoutedEventArgs e)
        {
            LanguageManager.Current.LanguageChanged += OnLanguageChanged;
        }

        private void VirtualKeyboardControl_Unloaded(object sender, RoutedEventArgs e)
        {
            LanguageManager.Current.LanguageChanged -= OnLanguageChanged;
        }

        #region App Language

        private void OnLanguageChanged(object sender, Language language)
        {
            try
            {
                _currentKeyboardLayoutProvider?.OnAppLanguageChanged(language);
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.Ui, $"Keyboard '{nameof(OnLanguageChanged)}' failed.", ex);
            }
        }

        #endregion

        #region KeyButtonStyleSelector

        public static readonly DependencyProperty KeyButtonStyleSelectorProperty = DependencyProperty.Register(
            nameof(KeyButtonStyleSelector), typeof(IVirtualKeyButtonStyleSelector), typeof(VirtualKeyboardControl), new PropertyMetadata(default(IVirtualKeyButtonStyleSelector)));

        public IVirtualKeyButtonStyleSelector KeyButtonStyleSelector
        {
            get => (IVirtualKeyButtonStyleSelector)GetValue(KeyButtonStyleSelectorProperty);
            set => SetValue(KeyButtonStyleSelectorProperty, value);
        }

        #endregion

        #region KeyButtonContentTemplateSelector

        public static readonly DependencyProperty KeyButtonContentTemplateSelectorProperty = DependencyProperty.Register(
            nameof(KeyButtonContentTemplateSelector), typeof(DataTemplateSelector), typeof(VirtualKeyboardControl), new PropertyMetadata(default(DataTemplateSelector)));

        public DataTemplateSelector KeyButtonContentTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(KeyButtonContentTemplateSelectorProperty);
            set => SetValue(KeyButtonContentTemplateSelectorProperty, value);
        }

        #endregion

        #region Target

        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
            nameof(Target), typeof(IVirtualKeyboardControlTarget), typeof(VirtualKeyboardControl), new PropertyMetadata(default(IVirtualKeyboardControlTarget), TargetPropertyChangedCallback));

        private static void TargetPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((VirtualKeyboardControl)dependencyObject).OnTargetChanged(
                dependencyPropertyChangedEventArgs.OldValue as IVirtualKeyboardControlTarget,
                dependencyPropertyChangedEventArgs.NewValue as IVirtualKeyboardControlTarget);
        }

        public IVirtualKeyboardControlTarget Target
        {
            get => (IVirtualKeyboardControlTarget)GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }

        #endregion

        #region ControlKeyCommand

        public static readonly DependencyProperty ControlKeyCommandProperty = DependencyProperty.Register(
            nameof(ControlKeyCommand), typeof(ICommand), typeof(VirtualKeyboardControl), new PropertyMetadata(default(ICommand)));

        public ICommand ControlKeyCommand
        {
            get => (ICommand)GetValue(ControlKeyCommandProperty);
            set => SetValue(ControlKeyCommandProperty, value);
        }

        #endregion

        /// <summary>
        /// For usage in template.
        /// </summary>
        public ICommand KeyPressedCommand { get; }

        private IVirtualKeyboardLayoutProvider _currentKeyboardLayoutProvider;

        private void OnTargetChanged(IVirtualKeyboardControlTarget previousTarget, IVirtualKeyboardControlTarget currentTarget)
        {
            if (previousTarget != null
                && previousTarget is DependencyObject previousTargetControl)
            {
                VirtualKeyboard.SetIsTarget(previousTargetControl, false);
            }

            // reset keyboard layout/visibility
            if (_currentKeyboardLayoutProvider != null)
            {
                _currentKeyboardLayoutProvider.LayoutChanged -= OnLayoutChanged;
                _currentKeyboardLayoutProvider = null;
            }

            Render(null);
            Visibility = Visibility.Collapsed;

            var targetControl = currentTarget as DependencyObject;
            if (targetControl == null)
            {
                return;
            }

            // render keyboard layout basing on target's type
            var keyboardType = VirtualKeyboard.GetType(targetControl);
            switch (keyboardType)
            {
                case VirtualKeyboardLayoutTypeEnum.Digits:
                    _currentKeyboardLayoutProvider = new DigitsLayoutProvider();
                    break;

                case VirtualKeyboardLayoutTypeEnum.Custom:
                default:
                    _currentKeyboardLayoutProvider = VirtualKeyboard.GetCustomLayoutProvider(targetControl);
                    if (_currentKeyboardLayoutProvider == null)
                    {
                        Log.Error(LogContextEnum.Ui, $"Custom layout provider is not set for '{targetControl.GetType().Name}'.");
                    }

                    break;
            }

            if (_currentKeyboardLayoutProvider != null)
            {
                try
                {
                    _currentKeyboardLayoutProvider.LayoutChanged += OnLayoutChanged;
                    _currentKeyboardLayoutProvider.InitLayout(LanguageManager.Current.CurrentAppLanguage);
                    Visibility = Visibility.Visible;
                    VirtualKeyboard.SetIsTarget(targetControl, true);
                }
                catch (Exception ex)
                {
                    Log.Error(LogContextEnum.Ui, $"Setting of keyboard layout with provider '{_currentKeyboardLayoutProvider.GetType().Name}' failed.", ex);
                }
            }
        }

        private void OnKeyPressed(VirtualKey virtualKey)
        {
            if (virtualKey == null)
            {
                return;
            }

            var keyType = virtualKey.Type;
            switch (keyType)
            {
                case VirtualKeyTypeEnum.Text:
                    OnTextKeyPressed(virtualKey.Value);
                    break;

                case VirtualKeyTypeEnum.Special:
                    OnSpecialKeyPressed(virtualKey.SpecialKeyType);
                    break;

                case VirtualKeyTypeEnum.Control:
                    OnControlKeyPressed(virtualKey.ControlCommand);
                    break;

                case VirtualKeyTypeEnum.Placeholder:
                    // do nothing
                    break;
            }
        }

        private void OnTextKeyPressed(string text)
        {
            Target?.AddText(text);
        }

        private void OnSpecialKeyPressed(VirtualSpecialKeyTypeEnum? specialKeyType)
        {
            if (specialKeyType == null
                || Target == null)
            {
                return;
            }

            if (specialKeyType.Value == VirtualSpecialKeyTypeEnum.Space)
            {
                OnTextKeyPressed(" ");
                return;
            }

            switch (specialKeyType.Value)
            {
                case VirtualSpecialKeyTypeEnum.Backspace:
                    Target.ProcessBackspace();
                    break;

                default:
                    Log.Error(LogContextEnum.Ui, $"Keyboard special key '{specialKeyType}' is not supported.");
                    break;
            }
        }

        private void OnControlKeyPressed(string controlCommand)
        {
            if (_currentKeyboardLayoutProvider == null)
            {
                return;
            }

            try
            {
                _currentKeyboardLayoutProvider.ProcessControlCommand(controlCommand, ControlKeyCommand);
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.Ui, $"Control command processing by provider '{_currentKeyboardLayoutProvider?.GetType().Name}' failed.", ex);
            }
        }

        private void OnLayoutChanged(object sender, VirtualKeyboardLayout layout)
        {
            ThreadHelper.RunInUiThreadAsync(() => Render(layout));
        }

        private void Render(VirtualKeyboardLayout layout)
        {
            if (layout == null)
            {
                Content = null;
                return;
            }

            var rowsContainer = new StackPanel();
            foreach (var layoutRow in layout.Rows)
            {
                var rowContainer = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    };

                foreach (var virtualKey in layoutRow)
                {
                    var virtualKeyButton = new Button()
                        {
                            Content = virtualKey,
                            Style = KeyButtonStyleSelector?.SelectStyle(virtualKey),
                            Command = KeyPressedCommand,
                            CommandParameter = virtualKey,
                        };
                    virtualKeyButton.ContentTemplate = KeyButtonContentTemplateSelector.SelectTemplate(virtualKey, virtualKeyButton);

                    rowContainer.Children.Add(virtualKeyButton);
                }

                rowsContainer.Children.Add(rowContainer);
            }

            Content = rowsContainer;
        }
    }
}
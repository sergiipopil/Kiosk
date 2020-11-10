using Windows.UI.Xaml;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts;

namespace KioskBrains.Kiosk.Core.Ui.VirtualKeyboard
{
    public class VirtualKeyboard : DependencyObject
    {
        #region Type (Attached)

        public static readonly DependencyProperty TypeProperty = DependencyProperty.RegisterAttached(
            "Type", typeof(VirtualKeyboardLayoutTypeEnum), typeof(VirtualKeyboard), new PropertyMetadata(default(VirtualKeyboardLayoutTypeEnum)));

        public static void SetType(DependencyObject element, VirtualKeyboardLayoutTypeEnum value)
        {
            element.SetValue(TypeProperty, value);
        }

        public static VirtualKeyboardLayoutTypeEnum GetType(DependencyObject element)
        {
            return (VirtualKeyboardLayoutTypeEnum)element.GetValue(TypeProperty);
        }

        #endregion

        #region CustomLayoutProvider (Attached)

        public static readonly DependencyProperty CustomLayoutProviderProperty = DependencyProperty.RegisterAttached(
            "CustomLayoutProvider", typeof(IVirtualKeyboardLayoutProvider), typeof(VirtualKeyboard), new PropertyMetadata(default(IVirtualKeyboardLayoutProvider)));

        public static void SetCustomLayoutProvider(DependencyObject element, IVirtualKeyboardLayoutProvider value)
        {
            element.SetValue(CustomLayoutProviderProperty, value);
        }

        public static IVirtualKeyboardLayoutProvider GetCustomLayoutProvider(DependencyObject element)
        {
            return (IVirtualKeyboardLayoutProvider)element.GetValue(CustomLayoutProviderProperty);
        }

        #endregion

        #region IsTarget (Attached)

        public static readonly DependencyProperty IsTargetProperty = DependencyProperty.RegisterAttached(
            "IsTarget", typeof(bool), typeof(VirtualKeyboard), new PropertyMetadata(default(bool)));

        public static void SetIsTarget(DependencyObject element, bool value)
        {
            element.SetValue(IsTargetProperty, value);
        }

        public static bool GetIsTarget(DependencyObject element)
        {
            return (bool)element.GetValue(IsTargetProperty);
        }

        #endregion
    }
}
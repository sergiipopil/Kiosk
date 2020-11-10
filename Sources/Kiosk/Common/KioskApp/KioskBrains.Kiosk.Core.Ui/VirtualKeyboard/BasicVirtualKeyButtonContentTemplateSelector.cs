using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Keys;

namespace KioskBrains.Kiosk.Core.Ui.VirtualKeyboard
{
    public class BasicVirtualKeyButtonContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Text { get; set; }

        public DataTemplate Special { get; set; }

        public DataTemplate Control { get; set; }

        public DataTemplate Placeholder { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var virtualKey = item as VirtualKey;
            if (virtualKey == null)
            {
                return null;
            }

            switch (virtualKey.Type)
            {
                case VirtualKeyTypeEnum.Text:
                    return Text;
                case VirtualKeyTypeEnum.Special:
                    return Special;
                case VirtualKeyTypeEnum.Control:
                    return Control;
                case VirtualKeyTypeEnum.Placeholder:
                    return Placeholder;
                default:
                    return null;
            }
        }
    }
}
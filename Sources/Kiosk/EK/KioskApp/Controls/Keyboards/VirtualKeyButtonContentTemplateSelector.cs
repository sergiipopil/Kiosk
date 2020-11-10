using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Keys;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Controls.Keyboards
{
    public class VirtualKeyButtonContentTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var virtualKey = item as VirtualKey;
            if (virtualKey == null)
            {
                return null;
            }

            string resourceKey = null;
            switch (virtualKey.Type)
            {
                case VirtualKeyTypeEnum.Text:
                case VirtualKeyTypeEnum.Placeholder:
                    resourceKey = "VirtualTextKeyButtonContentTemplate";
                    break;
                case VirtualKeyTypeEnum.Special:
                {
                    switch (virtualKey.SpecialKeyType)
                    {
                        case VirtualSpecialKeyTypeEnum.Backspace:
                            resourceKey = "VirtualBackspaceKeyButtonContentTemplate";
                            break;
                        default:
                            resourceKey = "VirtualTextKeyButtonContentTemplate";
                            break;
                    }

                    break;
                }
                case VirtualKeyTypeEnum.Control:
                {
                    switch (virtualKey.ControlCommand)
                    {
                        case CommonControlCommands.SwitchLanguage:
                            resourceKey = "VirtualSwitchLanguageKeyButtonContentTemplate";
                            break;
                        case CommonControlCommands.Confirm:
                            resourceKey = "VirtualConfirmKeyButtonContentTemplate";
                            break;
                        default:
                            resourceKey = "VirtualTextKeyButtonContentTemplate";
                            break;
                    }

                    break;
                }
            }

            if (resourceKey == null)
            {
                return null;
            }

            return ResourceHelper.GetStaticResourceFromUIThread<DataTemplate>(resourceKey);
        }
    }
}
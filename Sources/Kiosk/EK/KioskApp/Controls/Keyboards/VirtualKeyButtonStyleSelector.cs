using Windows.UI.Xaml;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Keys;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Controls.Keyboards
{
    public class VirtualKeyButtonStyleSelector : IVirtualKeyButtonStyleSelector
    {
        public Style SelectStyle(VirtualKey virtualKey)
        {
            if (virtualKey == null)
            {
                return null;
            }

            string styleName = null;
            switch (virtualKey.Type)
            {
                case VirtualKeyTypeEnum.Text:
                    styleName = "VirtualTextKeyButtonStyle";
                    break;

                case VirtualKeyTypeEnum.Special:
                {
                    switch (virtualKey.SpecialKeyType)
                    {
                        case VirtualSpecialKeyTypeEnum.Space:
                            styleName = "VirtualSpaceKeyButtonStyle";
                            break;
                        case VirtualSpecialKeyTypeEnum.Backspace:
                            styleName = "VirtualBackspaceKeyButtonStyle";
                            break;
                    }

                    break;
                }

                case VirtualKeyTypeEnum.Control:
                {
                    switch (virtualKey.ControlCommand)
                    {
                        case CommonControlCommands.SwitchLanguage:
                            styleName = "VirtualSwitchLanguageKeyButtonStyle";
                            break;
                        case CommonControlCommands.Confirm:
                            styleName = "VirtualConfirmKeyButtonStyle";
                            break;
                        default:
                            styleName = "VirtualControlKeyButtonStyle";
                            break;
                    }

                    break;
                }

                case VirtualKeyTypeEnum.Placeholder:
                    styleName = "VirtualPlaceholderKeyButtonStyle";
                    break;
            }

            if (styleName == null)
            {
                return null;
            }

            return ResourceHelper.GetStaticResourceFromUIThread<Style>(styleName);
        }
    }
}
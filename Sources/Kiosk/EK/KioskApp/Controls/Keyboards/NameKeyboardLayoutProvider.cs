using System.Windows.Input;
using Windows.Globalization;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Keys;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts.Text;

namespace KioskApp.Controls.Keyboards
{
    public class NameKeyboardLayoutProvider : VirtualKeyboardLayoutProviderBase
    {
        private readonly object _layoutLocker = new object();

        private void SetLayoutByLanguage(Language currentAppLanguage)
        {
            // 1. letters
            IVirtualKeyboardLayoutProvider lettersLayoutProvider;
            switch (currentAppLanguage.LanguageTag)
            {
                case "ru":
                    lettersLayoutProvider = new RussianLettersProvider();
                    break;
                case "uk":
                    lettersLayoutProvider = new UkrainianLettersProvider();
                    break;
                case "en":
                default:
                    lettersLayoutProvider = new UsLettersProvider();
                    break;
            }

            lettersLayoutProvider.InitLayout(currentAppLanguage);
            var lettersLayout = lettersLayoutProvider.Layout;
            var lettersRows = lettersLayout.Rows;

            // 2. space and backspace
            var spaceRow = new VirtualKeyboardLayoutRow
                {
                    CreatePlaceholder(),
                    CreatePlaceholder(),
                    new VirtualKey()
                        {
                            Type = VirtualKeyTypeEnum.Special,
                            SpecialKeyType = VirtualSpecialKeyTypeEnum.Space,
                        },
                    CreatePlaceholder(),
                    new VirtualKey()
                        {
                            Type = VirtualKeyTypeEnum.Special,
                            SpecialKeyType = VirtualSpecialKeyTypeEnum.Backspace,
                        },
                };

            var layout = new VirtualKeyboardLayout();
            layout.Rows.AddRange(lettersRows);
            layout.Rows.Add(spaceRow);

            Layout = layout;
        }

        private VirtualKey CreatePlaceholder()
        {
            return new VirtualKey()
                {
                    Type = VirtualKeyTypeEnum.Placeholder,
                };
        }

        public override void InitLayout(Language currentAppLanguage)
        {
            lock (_layoutLocker)
            {
                SetLayoutByLanguage(currentAppLanguage);
            }
        }

        public override void ProcessControlCommand(string controlCommand, ICommand controlKeyCommand)
        {
        }

        public override void OnAppLanguageChanged(Language currentAppLanguage)
        {
            lock (_layoutLocker)
            {
                SetLayoutByLanguage(currentAppLanguage);
            }
        }
    }
}
using System.Linq;
using System.Windows.Input;
using Windows.Globalization;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Keys;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts.Text;

namespace KioskApp.Controls.Keyboards
{
    public class AddressKeyboardLayoutProvider : VirtualKeyboardLayoutProviderBase
    {
        private readonly object _layoutLocker = new object();

        private void SetLayoutByLanguage(Language currentAppLanguage)
        {
            // 1. numbers and backspace
            var numberSymbols = "1234567890";
            var numbersRow = new VirtualKeyboardLayoutRow();
            numbersRow.AddRange(numberSymbols
                .Select(x => new VirtualKey()
                    {
                        Type = VirtualKeyTypeEnum.Text,
                        Value = x.ToString(),
                    }));
            numbersRow.Add(new VirtualKey()
                {
                    Type = VirtualKeyTypeEnum.Special,
                    SpecialKeyType = VirtualSpecialKeyTypeEnum.Backspace,
                });

            // 2. letters
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

            // additional text symbols
            var _2ndLetterRow = lettersRows[1];
            _2ndLetterRow.Add(new VirtualKey()
                {
                    Type = VirtualKeyTypeEnum.Text,
                    Value = "-",
                });
            var _3rdLetterRow = lettersRows[2];
            _3rdLetterRow.Add(new VirtualKey()
                {
                    Type = VirtualKeyTypeEnum.Text,
                    Value = ",",
                });
            _3rdLetterRow.Add(new VirtualKey()
                {
                    Type = VirtualKeyTypeEnum.Text,
                    Value = ".",
                });

            // 3. space
            var spaceRow = new VirtualKeyboardLayoutRow
                {
                    new VirtualKey()
                        {
                            Type = VirtualKeyTypeEnum.Special,
                            SpecialKeyType = VirtualSpecialKeyTypeEnum.Space,
                        },
                };

            var layout = new VirtualKeyboardLayout();
            layout.Rows.Add(numbersRow);
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
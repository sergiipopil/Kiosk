using System.Linq;
using System.Windows.Input;
using Windows.Globalization;
using KioskBrains.Common.Constants;
using KioskBrains.Kiosk.Core.Languages;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Keys;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts.Text;
using KioskBrains.Kiosk.Helpers.Text;

namespace KioskApp.Controls.Keyboards
{
    public class SearchByPartNumberKeyboardLayoutProvider : VirtualKeyboardLayoutProviderBase
    {
        private readonly object _layoutLocker = new object();

        private readonly Language _englishLanguage = new Language(Languages.EnglishCode);

        private Language _currentAppLanguage;

        private Language _selectedAppLanguage;

        private void SetLayoutByLanguage(Language language)
        {
            _selectedAppLanguage = language;

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
            switch (language.LanguageTag)
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

            lettersLayoutProvider.InitLayout(_currentAppLanguage);
            var lettersLayout = lettersLayoutProvider.Layout;
            var lettersRows = lettersLayout.Rows;

            // extra symbols
            if (language.LanguageTag == "en")
            {
                var _2ndLettersRow = lettersRows[1];
                _2ndLettersRow.Add(new VirtualKey()
                    {
                        Type = VirtualKeyTypeEnum.Text,
                        Value = "-",
                    });
                var _3rdLettersRow = lettersRows[2];
                _3rdLettersRow.Add(new VirtualKey()
                    {
                        Type = VirtualKeyTypeEnum.Text,
                        Value = "+",
                    });
                _3rdLettersRow.Add(new VirtualKey()
                    {
                        Type = VirtualKeyTypeEnum.Text,
                        Value = "/",
                    });
                _3rdLettersRow.Add(new VirtualKey()
                    {
                        Type = VirtualKeyTypeEnum.Text,
                        Value = ".",
                    });
            }

            // 3. space, languages and confirm
            var lastRow = new VirtualKeyboardLayoutRow
                {
                    new VirtualKey()
                        {
                            Type = VirtualKeyTypeEnum.Special,
                            SpecialKeyType = VirtualSpecialKeyTypeEnum.Space,
                        },
                    new VirtualKey()
                        {
                            Type = VirtualKeyTypeEnum.Control,
                            Value = language.NativeName.ToUpper().FirstNSymbols(3),
                            ControlCommand = CommonControlCommands.SwitchLanguage,
                        },
                    new VirtualKey()
                        {
                            Type = VirtualKeyTypeEnum.Control,
                            Value = LanguageManager.Current.GetLocalizedString("VirtualKeyboard_SearchKey_Text"),
                            ControlCommand = CommonControlCommands.Confirm,
                            Icon = KeyboardIcons.Search,
                        }
                };

            var layout = new VirtualKeyboardLayout();
            layout.Rows.Add(numbersRow);
            layout.Rows.AddRange(lettersRows);
            layout.Rows.Add(lastRow);

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
                _currentAppLanguage = currentAppLanguage;
                SetLayoutByLanguage(_englishLanguage);
            }
        }

        public override void ProcessControlCommand(string controlCommand, ICommand controlKeyCommand)
        {
            lock (_layoutLocker)
            {
                switch (controlCommand)
                {
                    case CommonControlCommands.SwitchLanguage:
                    {
                        if (_currentAppLanguage == _selectedAppLanguage)
                        {
                            if (_selectedAppLanguage != _englishLanguage)
                            {
                                // switch to english
                                SetLayoutByLanguage(_englishLanguage);
                            }
                        }
                        else
                        {
                            // switch back to app language
                            SetLayoutByLanguage(_currentAppLanguage);
                        }

                        break;
                    }

                    default:
                        controlKeyCommand?.Execute(controlCommand);
                        break;
                }
            }
        }

        public override void OnAppLanguageChanged(Language currentAppLanguage)
        {
            lock (_layoutLocker)
            {
                _currentAppLanguage = currentAppLanguage;
                SetLayoutByLanguage(_englishLanguage);
            }
        }
    }
}
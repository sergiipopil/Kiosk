using System.Linq;
using System.Windows.Input;
using Windows.Globalization;
using KioskBrains.Kiosk.Core.Languages;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Keys;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts.Text;

namespace KioskApp.Controls.Keyboards
{
    public class SearchByVinCodeKeyboardLayoutProvider : VirtualKeyboardLayoutProviderBase
    {
        private readonly object _layoutLocker = new object();

        private void SetLayoutByLanguage(Language language)
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
            var lettersLayoutProvider = new UsLettersProvider();
            lettersLayoutProvider.InitLayout(language);
            var lettersLayout = lettersLayoutProvider.Layout;
            var lettersRows = lettersLayout.Rows;

            // confirm
            var lastRow = lettersRows.Last();
            lastRow.Insert(0, CreatePlaceholder());
            lastRow.Insert(0, CreatePlaceholder());
            lastRow.Add(new VirtualKey()
                    {
                        Type = VirtualKeyTypeEnum.Control,
                        Value = LanguageManager.Current.GetLocalizedString("VirtualKeyboard_SearchKey_Text"),
                        ControlCommand = CommonControlCommands.Confirm,
                        Icon = KeyboardIcons.Search,
                    }
            );

            var layout = new VirtualKeyboardLayout();
            layout.Rows.Add(numbersRow);
            layout.Rows.AddRange(lettersRows);

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
            controlKeyCommand?.Execute(controlCommand);
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
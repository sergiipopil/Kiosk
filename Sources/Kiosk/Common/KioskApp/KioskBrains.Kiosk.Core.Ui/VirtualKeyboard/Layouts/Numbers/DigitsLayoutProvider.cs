using System.Linq;
using System.Windows.Input;
using Windows.Globalization;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Keys;

namespace KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts.Numbers
{
    public class DigitsLayoutProvider : VirtualKeyboardLayoutProviderBase
    {
        private void UpdateLayout()
        {
            var layout = new VirtualKeyboardLayout();

            var symbols = @"123
456
789
0";
            var symbolsLines = symbols.Split('\n')
                .Select(x => x.Trim())
                .ToArray();

            for (var i = 0; i < symbolsLines.Length; i++)
            {
                var symbolsLine = symbolsLines[i];

                var row = new VirtualKeyboardLayoutRow(
                    symbolsLine
                        .Select(x => new VirtualKey()
                            {
                                Type = VirtualKeyTypeEnum.Text,
                                Value = x.ToString(),
                            }));

                // add backspace to the last row
                if (i == symbolsLines.Length - 1)
                {
                    row.Add(new VirtualKey()
                        {
                            Type = VirtualKeyTypeEnum.Special,
                            SpecialKeyType = VirtualSpecialKeyTypeEnum.Backspace,
                        });
                }

                layout.Rows.Add(row);
            }

            Layout = layout;
        }

        public override void InitLayout(Language currentAppLanguage)
        {
            UpdateLayout();
        }

        public override void ProcessControlCommand(string controlCommand, ICommand controlKeyCommand)
        {
            // no control keys
        }

        public override void OnAppLanguageChanged(Language currentAppLanguage)
        {
            // not language-dependent
        }
    }
}
using System.Linq;
using System.Windows.Input;
using Windows.Globalization;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Keys;

namespace KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts.Text
{
    public abstract class SymbolLayoutProviderBase : VirtualKeyboardLayoutProviderBase
    {
        /// <summary>
        /// Letters should be returned in upper case.
        /// Line of symbol corresponds to its row.
        /// </summary>
        protected abstract string GetSymbolsString();

        private void UpdateLayout()
        {
            var symbols = GetSymbolsString();
            var symbolsLines = symbols.Split('\n')
                .Select(x => x.Trim())
                .ToArray();

            var layout = new VirtualKeyboardLayout();
            foreach (var symbolsLine in symbolsLines)
            {
                var row = new VirtualKeyboardLayoutRow(
                    symbolsLine
                        .Select(x => new VirtualKey()
                            {
                                Type = VirtualKeyTypeEnum.Text,
                                Value = x.ToString(),
                            }));
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
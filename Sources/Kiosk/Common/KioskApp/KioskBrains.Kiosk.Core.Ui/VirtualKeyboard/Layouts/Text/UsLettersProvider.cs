namespace KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts.Text
{
    public class UsLettersProvider : SymbolLayoutProviderBase
    {
        protected override string GetSymbolsString()
        {
            // ReSharper disable StringLiteralTypo
            return @"QWERTYUIOP
ASDFGHJKL
ZXCVBNM";
            // ReSharper restore StringLiteralTypo
        }
    }
}
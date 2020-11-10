namespace KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts.Text
{
    public class RussianLettersProvider : SymbolLayoutProviderBase
    {
        protected override string GetSymbolsString()
        {
            // ReSharper disable StringLiteralTypo
            return @"ЙЦУКЕНГШЩЗХЪ
ФЫВАПРОЛДЖЭ
ЯЧСМИТЬБЮЁ";
            // ReSharper restore StringLiteralTypo
        }
    }
}
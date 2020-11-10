namespace KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts.Text
{
    public class UkrainianLettersProvider : SymbolLayoutProviderBase
    {
        protected override string GetSymbolsString()
        {
            // ReSharper disable StringLiteralTypo
            return @"ЙЦУКЕНГШЩЗХЇ
ФІВАПРОЛДЖЄҐ
ЯЧСМИТЬБЮ";
            // ReSharper restore StringLiteralTypo
        }
    }
}
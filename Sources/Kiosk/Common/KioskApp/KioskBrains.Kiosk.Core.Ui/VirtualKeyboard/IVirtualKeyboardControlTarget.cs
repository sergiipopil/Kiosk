namespace KioskBrains.Kiosk.Core.Ui.VirtualKeyboard
{
    public interface IVirtualKeyboardControlTarget
    {
        void AddText(string textAddition);

        void ProcessBackspace();

        void ProcessLeftArrow();

        void ProcessRightArrow();
    }
}
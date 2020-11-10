namespace KioskBrains.Kiosk.Helpers.Ui
{
    public class PropertyChangedEventArgs<TValue>
    {
        public TValue OldValue { get; }

        public TValue NewValue { get; }

        public PropertyChangedEventArgs(TValue oldValue, TValue newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
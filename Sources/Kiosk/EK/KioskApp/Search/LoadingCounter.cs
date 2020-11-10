using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Search
{
    public class LoadingCounter : UiBindableObject
    {
        #region IsLoading

        private bool _IsLoading;

        public bool IsLoading
        {
            get => _IsLoading;
            set => SetProperty(ref _IsLoading, value);
        }

        #endregion

        private int _counter;

        private readonly object _locker = new object();

        public void Increment()
        {
            lock (_locker)
            {
                _counter++;
                IsLoading = _counter > 0;
            }
        }

        public void Decrement()
        {
            lock (_locker)
            {
                if (_counter > 0)
                {
                    _counter--;
                }

                IsLoading = _counter > 0;
            }
        }
    }
}
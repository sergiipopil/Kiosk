using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Helpers
{
    public class BooleanCounter : UiBindableObject
    {
        #region IsTrue

        private bool _IsTrue;

        public bool IsTrue
        {
            get => _IsTrue;
            set => SetProperty(ref _IsTrue, value);
        }

        #endregion

        private int _counter;

        private readonly object _locker = new object();

        public void Increment()
        {
            lock (_locker)
            {
                _counter++;
                IsTrue = _counter > 0;
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

                IsTrue = _counter > 0;
            }
        }

        public void Reset()
        {
            lock (_locker)
            {
                _counter = 0;
                IsTrue = false;
            }
        }
    }
}
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Search
{
    public class SelectedCategoryValue : UiBindableObject
    {
        public string Id { get; set; }

        #region Name1

        private string _Name1;

        public string Name1
        {
            get => _Name1;
            set => SetProperty(ref _Name1, value);
        }

        #endregion

        #region Name2

        private string _Name2;

        public string Name2
        {
            get => _Name2;
            set => SetProperty(ref _Name2, value);
        }

        #endregion

        #region Context

        public string ContextCarModelId { get; set; }

        public string ContextCarModelModificationId { get; set; }

        public string ContextVinCode { get; set; }

        #endregion
    }
}
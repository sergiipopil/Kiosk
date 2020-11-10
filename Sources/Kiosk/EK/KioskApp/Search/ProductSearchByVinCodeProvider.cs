using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using KioskBrains.Common.EK.Api;
using KioskBrains.Kiosk.Helpers.Threads;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Search
{
    public class ProductSearchByVinCodeProvider : SearchProviderBase, ISearchStateProvider
    {
        public ProductSearchByVinCodeProvider()
        {
            ModificationSelectedCommand = new RelayCommand(
                nameof(ModificationSelectedCommand),
                parameter => OnModificationSelectedCommand(parameter as CarModelModification));

            RetryOnErrorCommand = new RelayCommand(
                nameof(RetryOnErrorCommand),
                parameter => UpdateSearchResults(_termUpdatedOn, _termValue));

            ResetState();
        }

        public override bool IsAutocompleteSupported => false;

        protected override Task<string[]> GetAutocompleteOptionsAsync(string term, CancellationToken cancellationToken)
        {
            return null;
        }

        public override bool IsSearchManualRun => true;

        #region SearchState

        private SearchStateEnum _SearchState = SearchStateEnum.TermIsRequired;

        public SearchStateEnum SearchState
        {
            get => _SearchState;
            set => SetProperty(ref _SearchState, value);
        }

        #endregion

        public ICommand RetryOnErrorCommand { get; }

        private readonly object _stateLocker = new object();

        private DateTime _termUpdatedOn;

        private string _termValue;

        private SearchProviderRequest _currentApiRequest;

        protected override void UpdateSearchResults(DateTime termTime, string term)
        {
            lock (_stateLocker)
            {
                _termUpdatedOn = termTime;
                _termValue = term;
                ResetState();
                _currentApiRequest?.Cancel();

                if (term == null
                    || term.Length < 8)
                {
                    SearchState = SearchStateEnum.TermIsRequired;
                }
                else
                {
                    _currentApiRequest = new SearchProviderRequest(nameof(ModificationsRequestAsync), ModificationsRequestAsync);
                    _currentApiRequest.RunRequest(_termUpdatedOn, _termValue);
                }
            }
        }

        private void ResetState()
        {
            Modifications = null;
        }

        #region Modifications

        private CarModelModification[] _Modifications;

        public CarModelModification[] Modifications
        {
            get => _Modifications;
            set => SetProperty(ref _Modifications, value);
        }

        #endregion

        private async Task ModificationsRequestAsync(DateTime termTime, string term, CancellationToken cancellationToken)
        {
            try
            {
                SearchState = SearchStateEnum.Searching;

                var response = await ServerApiHelper.ProductSearchByVinCodeAsync(
                    new EkKioskProductSearchByVinCodeGetRequest()
                        {
                            VinCode = term,
                        },
                    cancellationToken);
                var modifications = response.ModelModifications;
                if (modifications != null
                    && modifications.Length == 0)
                {
                    modifications = null;
                }

                lock (_stateLocker)
                {
                    if (termTime != _termUpdatedOn)
                    {
                        // request is expired
                        return;
                    }

                    Modifications = modifications?
                        .Select(x => new CarModelModification(x))
                        .ToArray();

                    SearchState = Modifications == null
                        ? SearchStateEnum.NoResults
                        : SearchStateEnum.Results;
                }
            }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException))
                {
                    SearchState = SearchStateEnum.Error;
                }

                throw;
            }
        }

        public ICommand ModificationSelectedCommand { get; }

        private void OnModificationSelectedCommand(CarModelModification carModelModification)
        {
            if (carModelModification == null)
            {
                return;
            }

            OnCategorySelected(new SelectedCategoryValue()
                {
                    Name1 = $"vin {_termValue}",
                    Name2 = carModelModification.Name,
                    Id = carModelModification.Id.ToString(),
                    ContextCarModelId = carModelModification.ModelId.ToString(),
                    ContextVinCode = _termValue,
                });
        }

        public event EventHandler<SelectedCategoryValue> CategorySelected;

        protected virtual void OnCategorySelected(SelectedCategoryValue eventArgs)
        {
            ThreadHelper.RunInUiThreadAsync(() =>
                {
                    CategorySelected?.Invoke(this, eventArgs);
                });
        }
    }
}
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using KioskApp.Clients.NovaPoshtaUkraine;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Search
{
    public sealed class NovaPoshtaUkraineBranchSearchProvider : SearchProviderBase, ISearchStateProvider
    {
        public NovaPoshtaUkraineBranchSearchProvider()
        {
            RetryOnErrorCommand = new RelayCommand(
                nameof(RetryOnErrorCommand),
                parameter => RequestBranches());

            RequestBranches();
        }

        public override bool IsSearchManualRun => false;

        public override bool IsAutocompleteSupported => false;

        protected override Task<string[]> GetAutocompleteOptionsAsync(string term, CancellationToken cancellationToken)
        {
            return null;
        }

        #region SearchState

        private SearchStateEnum _SearchState;

        public SearchStateEnum SearchState
        {
            get => _SearchState;
            set => SetProperty(ref _SearchState, value);
        }

        #endregion

        public ICommand RetryOnErrorCommand { get; }

        #region OriginalBranches

        private NovaPoshtaUkraineBranch[] _OriginalBranches;

        public NovaPoshtaUkraineBranch[] OriginalBranches
        {
            get => _OriginalBranches;
            set => SetProperty(ref _OriginalBranches, value);
        }

        #endregion

        #region Branches

        private NovaPoshtaUkraineBranch[] _Branches;

        public NovaPoshtaUkraineBranch[] Branches
        {
            get => _Branches;
            set => SetProperty(ref _Branches, value);
        }

        #endregion

        private readonly object _stateLocker = new object();

        private DateTime _termUpdatedOn;

        private string _termValue;

        protected override void UpdateSearchResults(DateTime termTime, string term)
        {
            lock (_stateLocker)
            {
                _termUpdatedOn = termTime;
                _termValue = term;

                UpdateBranches();
            }
        }

        private SearchProviderRequest _currentUpdateRequest;

        private void UpdateBranches()
        {
            lock (_stateLocker)
            {
                _currentUpdateRequest?.Cancel();

                _currentUpdateRequest = new SearchProviderRequest(nameof(UpdateBranchesAsync), UpdateBranchesAsync);
                _currentUpdateRequest.RunRequest(_termUpdatedOn, _termValue);
            }
        }

#pragma warning disable 1998 // async without await
        private async Task UpdateBranchesAsync(DateTime termTime, string term, CancellationToken cancellationToken)
#pragma warning restore 1998
        {
            var originalBranches = OriginalBranches;

            lock (_stateLocker)
            {
                // just in case - should not happen
                if (originalBranches == null)
                {
                    Branches = null;
                    return;
                }

                if (string.IsNullOrWhiteSpace(term))
                {
                    // some term is required (3K+ branches in Ukraine on Feb 19)
                    Branches = null;
                    SearchState = SearchStateEnum.TermIsRequired;
                    return;
                }

                SearchState = SearchStateEnum.Searching;
            }

            var termParts = term.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            var filteredBranches = originalBranches
                .Select(x =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var score = 0;
                        foreach (var termPart in termParts)
                        {
                            if (x.Number?.Equals(termPart, StringComparison.OrdinalIgnoreCase) == true)
                            {
                                score += 20;
                            }

                            if (x.City?.Equals(termPart, StringComparison.OrdinalIgnoreCase) == true)
                            {
                                score += 20;
                            }
                            else if (x.City?.Contains(termPart, StringComparison.OrdinalIgnoreCase) == true)
                            {
                                score += 5;
                            }

                            if (x.AddressLine1?.Contains(termPart, StringComparison.OrdinalIgnoreCase) == true)
                            {
                                score += 1;
                            }
                        }

                        return new
                            {
                                Score = score,
                                Branch = x,
                            };
                    })
                .Where(x => x.Score != 0)
                .OrderByDescending(x => x.Score)
                .Select(x => x.Branch)
                .ToArray();
            if (filteredBranches.Length == 0)
            {
                filteredBranches = null;
            }

            lock (_stateLocker)
            {
                if (termTime != _termUpdatedOn)
                {
                    // request is expired
                    return;
                }

                Branches = filteredBranches
                    ?.Take(100)
                    .ToArray();

                SearchState = Branches?.Length > 0
                    ? SearchStateEnum.Results
                    : SearchStateEnum.NoResults;
            }
        }

        private SearchProviderRequest _currentApiRequest;

        private void RequestBranches()
        {
            lock (_stateLocker)
            {
                OriginalBranches = null;
                UpdateBranches();

                _currentApiRequest?.Cancel();

                _currentApiRequest = new SearchProviderRequest(nameof(RequestBranchesAsync), RequestBranchesAsync);
                _currentApiRequest.RunRequest(DateTime.Now, null);
            }
        }

        private async Task RequestBranchesAsync(DateTime termTime, string term, CancellationToken cancellationToken)
        {
            try
            {
                SearchState = SearchStateEnum.Searching;

                var warehouseSearchItems = await NovaPoshtaUkraineClient.Current.GetAllWarehousesAsync(cancellationToken);

                lock (_stateLocker)
                {
                    OriginalBranches = warehouseSearchItems
                        .Select(x => new NovaPoshtaUkraineBranch(x))
                        .ToArray();
                    UpdateBranches();
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
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using KioskBrains.Kiosk.Helpers.Ui;
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Search
{
    public abstract class SearchProviderBase : UiBindableObject
    {
        protected SearchProviderBase()
        {
            AutocompleteOptionCommand = new RelayCommand(
                nameof(AutocompleteOptionCommand),
                parameter => OnAutocompleteOptionSelected(parameter as string));
            SearchManualRunCommand = new RelayCommand(
                nameof(SearchManualRunCommand),
                parameter => OnSearchManualRun());
        }

        public abstract bool IsAutocompleteSupported { get; }

        public abstract bool IsSearchManualRun { get; }

        #region AutocompleteOptions

        private string[] _AutocompleteOptions;

        public string[] AutocompleteOptions
        {
            get => _AutocompleteOptions;
            set => SetProperty(ref _AutocompleteOptions, value);
        }

        #endregion

        #region IsAutocompleteOptionsLoading

        private bool _IsAutocompleteOptionsLoading;

        public bool IsAutocompleteOptionsLoading
        {
            get => _IsAutocompleteOptionsLoading;
            set => SetProperty(ref _IsAutocompleteOptionsLoading, value);
        }

        #endregion

        #region Term

        private string _Term;

        public string Term
        {
            get => _Term;
            set => SetProperty(ref _Term, value);
        }

        #endregion

        public string ProviderName => GetType().Name;

        protected override void OnOwnPropertyChanged(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(Term):
                    OnTermChanged();
                    break;
            }
        }

        private readonly object _providerStateLocker = new object();

        /// <summary>
        /// Term value that is synced with <see cref="_termUpdatedOn"/>.
        /// </summary>
        private string _termValue;

        private DateTime _termUpdatedOn;

        private void OnTermChanged()
        {
            lock (_providerStateLocker)
            {
                _termValue = Term;
                _termUpdatedOn = DateTime.Now;

                if (IsAutocompleteSupported)
                {
                    // cancel current request
                    // clear options
                    // init new request
                    _autocompleteRequest?.Cancel();
                    AutocompleteOptions = null;
                    _autocompleteRequest = new SearchProviderRequest($"{ProviderName} Autocomplete", AutocompleteRequestAsync);
                    _autocompleteRequest.RunRequest(_termUpdatedOn, _termValue);
                }

                if (!IsSearchManualRun)
                {
                    UpdateSearchResults(_termUpdatedOn, _termValue);
                }
            }
        }

        private void OnSearchManualRun()
        {
            lock (_providerStateLocker)
            {
                UpdateSearchResults(_termUpdatedOn, _termValue);
            }
        }

        protected abstract void UpdateSearchResults(DateTime termTime, string term);

        public ICommand SearchManualRunCommand { get; }

        #region Autocomplete Options Logic

        private SearchProviderRequest _autocompleteRequest;

        private async Task AutocompleteRequestAsync(DateTime termTime, string term, CancellationToken cancellationToken)
        {
            IsAutocompleteOptionsLoading = true;

            try
            {
                var autocompleteOptions = await GetAutocompleteOptionsAsync(term, cancellationToken);
                if (autocompleteOptions != null
                    && autocompleteOptions.Length == 0)
                {
                    autocompleteOptions = null;
                }

                lock (_providerStateLocker)
                {
                    if (termTime != _termUpdatedOn)
                    {
                        // request is expired
                        return;
                    }

                    AutocompleteOptions = autocompleteOptions;
                }
            }
            finally
            {
                IsAutocompleteOptionsLoading = false;
            }
        }

        protected abstract Task<string[]> GetAutocompleteOptionsAsync(string term, CancellationToken cancellationToken);

        private void OnAutocompleteOptionSelected(string autocompleteOption)
        {
            if (string.IsNullOrEmpty(autocompleteOption))
            {
                return;
            }

            Term = ReplaceLastWord(Term, autocompleteOption.ToUpper() + " ");
        }

        public ICommand AutocompleteOptionCommand { get; }

        #endregion

        #region Helpers

        protected string GetLastWord(string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
            {
                return phrase;
            }

            var words = phrase.Split(' ');
            var lastWord = words[words.Length - 1];
            return lastWord;
        }

        private string ReplaceLastWord(string phrase, string newLastWord)
        {
            if (string.IsNullOrEmpty(phrase))
            {
                return newLastWord;
            }

            if (newLastWord == null)
            {
                newLastWord = "";
            }

            var currentLastWord = GetLastWord(phrase);
            return phrase.Substring(0, phrase.Length - currentLastWord.Length) + newLastWord;
        }

        #endregion
    }
}
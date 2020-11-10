using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Helpers.Threads;
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskBrains.Kiosk.Core.Languages
{
    public class LanguageManager : UiBindableObject
    {
        #region Singleton

        public static LanguageManager Current { get; } = new LanguageManager();

        private LanguageManager()
        {
        }

        #endregion

        public void Initialize(string[] kioskLanguageCodes)
        {
            if (kioskLanguageCodes == null || kioskLanguageCodes.Length == 0)
            {
                kioskLanguageCodes = new[] { Common.Constants.Languages.GlobalLanguageCode };
            }

            _kioskLanguages = kioskLanguageCodes
                .Select(x => new Language(x))
                .ToArray();
            _kioskLocalLanguage = _kioskLanguages[0];
            _globalLanguage = _kioskLanguages.FirstOrDefault(x => x.LanguageTag == Common.Constants.Languages.GlobalLanguageCode)
                              ?? _kioskLocalLanguage;

#pragma warning disable 4014
            SetAppLanguageAsync(_globalLanguage);
#pragma warning restore 4014
        }

        private void CheckIfInitialized()
        {
            // design-mode support
            if (DesignMode.DesignModeEnabled
                && _kioskLanguages == null)
            {
                Initialize(new[] { "uk", "ru", "en" });
            }

            Assure.CheckFlowState(_kioskLanguages != null, $"'{nameof(LanguageManager)}' is not initialized. Run '{nameof(Initialize)}' first.");
        }

        private Language[] _kioskLanguages;

        public Language[] KioskLanguages
        {
            get
            {
                CheckIfInitialized();
                return _kioskLanguages;
            }
        }

        private Language _kioskLocalLanguage;

        public Language KioskLocalLanguage
        {
            get
            {
                CheckIfInitialized();
                return _kioskLocalLanguage;
            }
        }

        private Language _globalLanguage;

        public Language GlobalLanguage
        {
            get
            {
                CheckIfInitialized();
                return _globalLanguage;
            }
        }

        public Language CurrentAppLanguage => KioskLanguages.FirstOrDefault(x => x.LanguageTag == CurrentAppLanguageCode)
                                              ?? _globalLanguage;

        public string CurrentAppLanguageCode => ApplicationLanguages.PrimaryLanguageOverride;

        public event EventHandler<Language> LanguageChanged;

        private readonly object _setAppLanguageLocker = new object();

        private bool _isSetAppLanguageInProgress;

        public async Task SetAppLanguageAsync(Language language)
        {
            Assure.ArgumentNotNull(language, nameof(language));

            lock (_setAppLanguageLocker)
            {
                if (_isSetAppLanguageInProgress)
                {
                    Log.Error(LogContextEnum.Application, "Attempt to change language while it's change is in progress.");
                    return;
                }

                _isSetAppLanguageInProgress = true;
            }

            try
            {
                // run in non-UI thread to ensure it's set after return to UI thread
                await ThreadHelper.RunInBackgroundThreadAsync(async () =>
                    {
                        ApplicationLanguages.PrimaryLanguageOverride = language.LanguageTag;

                        // ensure UI thread is run once after language change
                        await ThreadHelper.RunInUiThreadAsync(() => { });

                        await OnPropertyChangedAsync(nameof(CurrentAppLanguage));

                        LanguageChanged?.Invoke(this, CurrentAppLanguage);
                    });
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.Application, $"{nameof(SetAppLanguageAsync)} failed.", ex);
            }
            finally
            {
                lock (_setAppLanguageLocker)
                {
                    _isSetAppLanguageInProgress = false;
                }
            }
        }

        public string GetLocalizedString(string stringKey, Language specificLanguage = null)
        {
            var context = ResourceContext.GetForViewIndependentUse();
            if (specificLanguage != null)
            {
                context.Languages = new[] { specificLanguage.LanguageTag };
            }
            else if (!string.IsNullOrWhiteSpace(CurrentAppLanguageCode))
            {
                context.Languages = new[] { CurrentAppLanguageCode };
            }
            else
            {
                context.Reset();
            }

            var resourceStringMap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");
            return resourceStringMap.GetValue(stringKey, context)?.ValueAsString ?? stringKey;
        }
    }
}
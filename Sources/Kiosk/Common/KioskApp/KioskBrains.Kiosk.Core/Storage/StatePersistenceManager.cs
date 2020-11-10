using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Common.Constants;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Core.Components.Operations;

namespace KioskBrains.Kiosk.Core.Storage
{
    public class StatePersistenceManager<TState>
        where TState : class, new()
    {
        static StatePersistenceManager()
        {
            var prohibitedRecordNameCharacters = new List<char>()
                {
                    // legal filename symbols but prohibited to make more flexible component role renaming
                    ' ',
                    '-',
                    '(',
                    ')',
                };
            prohibitedRecordNameCharacters.AddRange(Path.GetInvalidFileNameChars());
            ProhibitedRecordNameCharacters = prohibitedRecordNameCharacters
                .Select(x => x.ToString())
                .ToArray();
        }

        // ReSharper disable StaticMemberInGenericType
        private static string[] ProhibitedRecordNameCharacters { get; }
        // ReSharper restore StaticMemberInGenericType

        private readonly string _stateRecordName;

        private readonly Func<TState, TState> _statePostProcessor;

        private readonly string _kioskAppDataFolderName;

        /// <param name="stateRecordName">Record name. Prohibited symbols will be removed.</param>
        /// <param name="statePostProcessor">
        /// Post processing of state when loading.
        /// If state record was not found or it was read with error, 'null' is passed as argument - it's supposed that new state will be created and returned.
        /// </param>
        public StatePersistenceManager(string stateRecordName, Func<TState, TState> statePostProcessor)
            : this(stateRecordName, statePostProcessor, KioskFolderNames.AppData_State)
        {
        }

        internal StatePersistenceManager(string stateRecordName, Func<TState, TState> statePostProcessor, string kioskAppDataFolderName)
        {
            Assure.ArgumentNotNull(stateRecordName, nameof(stateRecordName));
            Assure.ArgumentNotNull(statePostProcessor, nameof(statePostProcessor));
            Assure.ArgumentNotNull(kioskAppDataFolderName, nameof(kioskAppDataFolderName));

            var recordNameBuilder = new StringBuilder(stateRecordName);
            foreach (var prohibitedRecordNameCharacter in ProhibitedRecordNameCharacters)
            {
                recordNameBuilder.Replace(prohibitedRecordNameCharacter, "");
            }

            _stateRecordName = recordNameBuilder.ToString();
            Assure.CheckFlowState(_stateRecordName.Length > 0, $"{nameof(stateRecordName)} '{stateRecordName}' is empty or consists only of prohibited symbols.");

            _statePostProcessor = statePostProcessor;
            _kioskAppDataFolderName = kioskAppDataFolderName;
        }

        public async Task<TState> LoadAsync(ComponentOperationContext context)
        {
            TState state;
            try
            {
                state = await AppDataStorage.Current.LoadRecordAsync<TState>(
                    _kioskAppDataFolderName,
                    _stateRecordName,
                    false,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Reading of '{_stateRecordName}' failed.";
                if (context == null)
                {
                    Log.Error(LogContextEnum.File, errorMessage, ex);
                }
                else
                {
                    context.Log.Error(LogContextEnum.File, errorMessage, ex);
                }

                state = null;
            }

            try
            {
                state = _statePostProcessor(state);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Post-processing of '{_stateRecordName}' failed.", ex);
            }

            return state;
        }

        /// <returns>'true' if record was saved successfully, 'false' is not.</returns>
        public async Task<bool> SaveAsync(TState state, ComponentOperationContext context)
        {
            try
            {
                await AppDataStorage.Current.SaveRecordAsync(
                    _kioskAppDataFolderName,
                    _stateRecordName,
                    state,
                    CancellationToken.None);

                return true;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Saving of '{_stateRecordName}' failed.";
                if (context == null)
                {
                    Log.Error(LogContextEnum.File, errorMessage, ex);
                }
                else
                {
                    context.Log.Error(LogContextEnum.File, errorMessage, ex);
                }

                return false;
            }
        }
    }
}
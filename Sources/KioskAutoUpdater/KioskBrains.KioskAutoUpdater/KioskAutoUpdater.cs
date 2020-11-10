using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using KioskBrains.Common.Api;
using KioskBrains.Common.Constants;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.CommonNet.Helpers;
using KioskBrains.Kiosk.CommonNet.Logging;
using KioskBrains.KioskAutoUpdater.Helpers;
using KioskBrains.KioskAutoUpdater.ServerApi;
using Newtonsoft.Json;

namespace KioskBrains.KioskAutoUpdater
{
    public class KioskAutoUpdater
    {
        #region Singleton

        public static KioskAutoUpdater Current { get; } = new KioskAutoUpdater();

        private KioskAutoUpdater()
        {
        }

        #endregion

        public Task RunAsync()
        {
            return Task.Run((Func<Task>)RunImplementationAsync);
        }

        #region Paths

        private static readonly string ConfigurationFilePath = Path.Combine(
            KioskFolderNames.KioskLocation,
            KioskFolderNames.Kiosk,
            KioskFolderNames.AppData,
            KioskFolderNames.AppData_Configuration,
            $"{KioskAppNames.KioskAutoUpdater}.json");

        private static readonly string KioskSerialKeyFilePath = Path.Combine(
            KioskFolderNames.KioskLocation,
            KioskFolderNames.Kiosk,
            KioskFileNames.Key);

        private static readonly string StateFilePath = Path.Combine(
            KioskFolderNames.KioskLocation,
            KioskFolderNames.Kiosk,
            KioskFolderNames.AppData,
            KioskFolderNames.AppData_State,
            $"{KioskAppNames.KioskAutoUpdater}.json");

        private static readonly string NotReadyForUpdateSignPath = Path.Combine(
            KioskFolderNames.KioskLocation,
            KioskFolderNames.Kiosk,
            KioskFolderNames.AppData,
            KioskFolderNames.AppData_State,
            $"{KioskFileNames.KioskAppNotReadyForUpdateSignWithoutExtension}.json");

        private static readonly string UpdatesFolderPath = Path.Combine(
            KioskFolderNames.KioskLocation,
            KioskFolderNames.Kiosk,
            KioskFolderNames.Updates);

        #endregion

        private KioskAutoUpdaterConfiguration _configuration;

        private string _kioskSerialKey;

        private async Task RunImplementationAsync()
        {
            JsonDefaultSettings.Initialize();
            Log.Initialize(new Logger(), VersionHelper.GetEntryAssemblyVersion());

            // read configuration
            try
            {
                _configuration = JsonConvert.DeserializeObject<KioskAutoUpdaterConfiguration>(File.ReadAllText(ConfigurationFilePath, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.KioskAutoUpdater, "Configuration read failed.", ex);
                return;
            }

            // read serial key
            try
            {
                _kioskSerialKey = File.ReadAllText(KioskSerialKeyFilePath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.KioskAutoUpdater, $"'{KioskFileNames.Key}' read failed.", ex);
                return;
            }

            ServerApiProxy.Current.Initialize(_configuration.ServerUri, _kioskSerialKey);

            Log.Info(LogContextEnum.KioskAutoUpdater, $"'{nameof(KioskAutoUpdater)}' started.");

            await RunUpdateCheckWorkerAsync();
        }

        private bool _isPreviousWebRequestFailed;

        private async Task RunUpdateCheckWorkerAsync()
        {
            while (true)
            {
                try
                {
                    // get current state
                    KioskAutoUpdaterState state = null;
                    if (File.Exists(StateFilePath))
                    {
                        try
                        {
                            state = JsonConvert.DeserializeObject<KioskAutoUpdaterState>(File.ReadAllText(StateFilePath, Encoding.UTF8));
                        }
                        catch (Exception ex)
                        {
                            Log.Error(LogContextEnum.KioskAutoUpdater, "State read failed.", ex);
                        }
                    }

                    // default state (first run or error)
                    if (state == null)
                    {
                        state = new KioskAutoUpdaterState();
                    }

                    // request kiosk version
                    KioskVersionGetResponse versionResponse;
                    try
                    {
                        versionResponse = await ServerApiProxy.Current.GetKioskVersionAsync(new EmptyRequest());
                        _isPreviousWebRequestFailed = false;
                    }
                    catch (Exception ex)
                    {
                        if (!_isPreviousWebRequestFailed)
                        {
                            Log.Error(LogContextEnum.KioskAutoUpdater, $"'{nameof(ServerApiProxy.GetKioskVersionAsync)}' failed.", ex);
                            _isPreviousWebRequestFailed = true;
                        }

                        goto EndOfIteration;
                    }

                    // check if any version is assigned
                    if (string.IsNullOrEmpty(versionResponse.AssignedKioskVersion))
                    {
                        goto EndOfIteration;
                    }

                    if (string.IsNullOrEmpty(versionResponse.AssignedKioskVersionUpdateUrl))
                    {
                        Log.Error(LogContextEnum.KioskAutoUpdater, $"'{nameof(ServerApiProxy.GetKioskVersionAsync)}' returned empty '{nameof(KioskVersionGetResponse.AssignedKioskVersionUpdateUrl)}' for version '{versionResponse.AssignedKioskVersion}'.");
                        // save assigned version in case of error (to avoid re-trying of broken update)
                        UpdateAutoUpdaterState(state, versionResponse);
                        goto EndOfIteration;
                    }

                    // todo: fix - download and update on first run (if AssignedKioskVersion is not null)
                    // check if a first run
                    if (state.LastAppliedKioskVersion == null)
                    {
                        // just save assigned version
                        UpdateAutoUpdaterState(state, versionResponse);
                        goto EndOfIteration;
                    }

                    // check if an update is required
                    if (state.LastAppliedKioskVersion == versionResponse.AssignedKioskVersion)
                    {
                        goto EndOfIteration;
                    }

                    Log.Trace(LogContextEnum.KioskAutoUpdater, $"New version '{versionResponse.AssignedKioskVersion}' is assigned.");

                    // get update file/folder names
                    string updateFilePath;
                    string updateFolderPath;
                    try
                    {
                        var updateFileName = Path.GetFileName(versionResponse.AssignedKioskVersionUpdateUrl);
                        if (Path.GetExtension(updateFileName) != ".zip")
                        {
                            throw new ServerApiException($"Update file '{updateFileName}' is not a zip archive.");
                        }

                        updateFilePath = Path.Combine(UpdatesFolderPath, updateFileName);
                        updateFolderPath = Path.Combine(UpdatesFolderPath, Path.GetFileNameWithoutExtension(updateFileName));
                    }
                    catch (Exception ex)
                    {
                        Log.Error(LogContextEnum.KioskAutoUpdater, "Update file/folder processing failed.", ex);
                        // save assigned version in case of error (to avoid re-trying of broken update)
                        UpdateAutoUpdaterState(state, versionResponse);
                        goto EndOfIteration;
                    }

                    // delete update file/folder (if exist)
                    if (!RemoveUpdateFiles(updateFolderPath, updateFilePath))
                    {
                        // save assigned version in case of error (to avoid re-trying of broken update)
                        UpdateAutoUpdaterState(state, versionResponse);
                        goto EndOfIteration;
                    }

                    // download update
                    try
                    {
                        Log.Trace(LogContextEnum.KioskAutoUpdater, $"Download of '{versionResponse.AssignedKioskVersionUpdateUrl}' started.");
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        // use simple legacy .net to download the file
                        using (var webClient = new WebClient())
                        {
                            await webClient.DownloadFileTaskAsync(versionResponse.AssignedKioskVersionUpdateUrl, updateFilePath);
                            _isPreviousWebRequestFailed = false;
                        }

                        stopwatch.Stop();
                        Log.Trace(LogContextEnum.KioskAutoUpdater, $"Download of '{versionResponse.AssignedKioskVersionUpdateUrl}' completed ({stopwatch.Elapsed}).");
                    }
                    catch (Exception ex)
                    {
                        if (!_isPreviousWebRequestFailed)
                        {
                            Log.Error(LogContextEnum.KioskAutoUpdater, $"Download of '{versionResponse.AssignedKioskVersionUpdateUrl}' failed.", ex);
                            _isPreviousWebRequestFailed = true;
                        }

                        goto EndOfIteration;
                    }

                    try
                    {
                        // unzip update
                        try
                        {
                            ZipFile.ExtractToDirectory(updateFilePath, updateFolderPath);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(LogContextEnum.KioskAutoUpdater, "Update unzip failed.", ex);
                            // save assigned version in case of error (to avoid re-trying of broken update)
                            UpdateAutoUpdaterState(state, versionResponse);
                            goto EndOfIteration;
                        }

                        // update is ready - wait for 'ready for update' state
                        while (true)
                        {
                            var lockFileInfo = new FileInfo(NotReadyForUpdateSignPath);
                            if (!lockFileInfo.Exists)
                            {
                                break;
                            }

                            if (lockFileInfo.CreationTime < (DateTime.Now - _configuration.MaxNotReadyForUpdateLockDuration))
                            {
                                Log.Warning(LogContextEnum.KioskAutoUpdater, $"Max lock file duration '{_configuration.MaxNotReadyForUpdateLockDuration}' is exceeded (lock file was created on '{lockFileInfo.CreationTime}').");
                                break;
                            }

                            await Task.Delay(_configuration.ReadyForUpdateCheckInterval);
                        }

                        // save assigned version before the update applying in case if Restart-Computer is presented in the update script
                        UpdateAutoUpdaterState(state, versionResponse);

                        // apply update
                        try
                        {
                            var setLocationScript = $"Set-Location \"{updateFolderPath}\"";
                            var installScript = await FileHelper.ReadFileAsync(Path.Combine(updateFolderPath, "ApplyUpdate.ps1"));
                            var response = await PowerShellHelper.RunScriptsAsync(
                                setLocationScript,
                                installScript);
                            if (response.HadErrors)
                            {
                                throw new PowerShellErrorException(response);
                            }

                            Log.Info(LogContextEnum.KioskAutoUpdater, $"Update '{versionResponse.AssignedKioskVersion}' successfully applied.");
                        }
                        catch (Exception ex)
                        {
                            Log.Error(LogContextEnum.KioskAutoUpdater, $"Update '{versionResponse.AssignedKioskVersion}' application failed.", ex);
                        }
                    }
                    finally
                    {
                        // clean up update files
                        RemoveUpdateFiles(updateFolderPath, updateFilePath);
                        Log.Trace(LogContextEnum.KioskAutoUpdater, "Update files were cleaned up.");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(LogContextEnum.KioskAutoUpdater, $"'{nameof(RunUpdateCheckWorkerAsync)}' iteration unhandled exception.", ex);
                }

                EndOfIteration:
                await Task.Delay(_configuration.UpdateCheckInterval);
            }

            // ReSharper disable FunctionNeverReturns
        }
        // ReSharper restore FunctionNeverReturns

        private void UpdateAutoUpdaterState(
            KioskAutoUpdaterState state,
            KioskVersionGetResponse versionResponse)
        {
            state.LastAppliedKioskVersion = versionResponse.AssignedKioskVersion;
            var stateString = JsonConvert.SerializeObject(state);
            File.WriteAllText(StateFilePath, stateString, Encoding.UTF8);
            Log.Info(LogContextEnum.KioskAutoUpdater, $"Last applied version was changed to '{versionResponse.AssignedKioskVersion}'.");
        }

        private bool RemoveUpdateFiles(string updateFolderPath, string updateFilePath)
        {
            try
            {
                if (Directory.Exists(updateFolderPath))
                {
                    Directory.Delete(updateFolderPath, true);
                }

                if (File.Exists(updateFilePath))
                {
                    File.Delete(updateFilePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.KioskAutoUpdater, "Existing update file/folder removal failed.", ex);
                return false;
            }
        }
    }
}
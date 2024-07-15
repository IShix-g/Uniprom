
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using CMSuniVortex;
using Uniprom.CMSuniVortex;
using CMSuniVortex.Addressable;
#endif

#if UNITY_EDITOR && ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using UnityEditor;
using Uniprom.Editor;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using AddressableHelper = Uniprom.Addressable.Editor.AddressableHelper;
#endif

namespace Uniprom
{
    public sealed class UnipromSettingsExporter : ScriptableObject
    {
        public const string ExporterPath = "Assets/Uniprom/UnipromSettingsExporter.asset";
        public const string ImporterName = "UnipromCuvImporter.asset";
        public const string SourceProjectSymbol = "UNIPROM_SOURCE_PROJECT";
        public const string ReleaseProfileName = "Uniprom-Release";
        public const string TestProfileName = "Uniprom-Test";
        public const string ReleaseRemoteBuildPath = "_UnipromReleaseServerData";
        public const string TestRemoteBuildPath = "_UnipromTestServerData";
        public const string InterstitialAddressableKey = "UnipromInterstitial";
        public const string InterstitialPrefabName = "UnipromInterstitial.prefab";
        public const string PackageRootPath = "Packages/com.ishix.uniprom/";
        public const string InterstitialPackagePrefabPath = PackageRootPath + "Editor/Prefabs/" + InterstitialPrefabName;
        public const string WallAddressableKey = "UnipromWall";
        public const string WallPrefabName = "UnipromWall.prefab";
        public const string WallPackagePrefabPath = PackageRootPath + "Editor/Prefabs/" + WallPrefabName;
        public const string SampleSceneName = "UnipromSamples.unity";
        public const string SamplePackageScenePath = PackageRootPath + "Samples/" + SampleSceneName;

#if UNITY_EDITOR && ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
        public static readonly AddressableCuvSettings DefaultCuvSettings
            = new ()
            {
                AddressableType = AddressableType.Remote,
                BundlePackingMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately,
                BuildCompressionMode = BundledAssetGroupSchema.BundleCompressionMode.LZ4,
                UpdateRestriction = AddressableCuvSettings.UpdateRestrictionType.CanChangePostRelease,
                Labels = new []{ UnipromSettings.Label }
            };
        
        [SerializeField, Tooltip("[Test] Specify the Url of the server on which to load Addressable assets.")]
        string _testRemoteLoadUrl;
        [SerializeField, Tooltip("[Production] Specify the Url of the server on which to load Addressable assets.")]
        string _releaseRemoteLoadUrl;
        [SerializeField, Tooltip("There is generally no need to increase this version. Use this only if you want to eliminate compatibility with already released applications.")]
        string _overridePlayerVersion = "1.0.0";
        [SerializeField, CuvFilePath("json")] string _releaseFtpSettingPath;
        [SerializeField, CuvFilePath("json")] string _testFtpSettingPath;
        [SerializeField, CuvReadOnly] UnipromCuvImporter _cuvImporter;
        [SerializeField, CuvReadOnly] ScriptableObject _reference;
        [SerializeField, CuvReadOnly] UnipromSettings _settings;
        
        public UnipromCuvImporter CuvImporter => _cuvImporter;
        public bool IsSendingFiles { get; private set; }
        public string ReleaseFtpSettingPath => _releaseFtpSettingPath;
        public string TestFtpSettingPath => _testFtpSettingPath;

        public static UnipromSettingsExporter GetInstance()
            => AssetDatabase.LoadAssetAtPath<UnipromSettingsExporter>(ExporterPath);

        public void SetReference(ScriptableObject reference)
        {
            _reference = reference;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }

        public void CreateImporterIfNeeded()
        {
            if (_cuvImporter == default)
            {
                var path = GetImporterPath();
                if (!string.IsNullOrEmpty(path))
                {
                    _cuvImporter = AssetDatabaseHelper.CreateOrLoadAsset<UnipromCuvImporter>(path);
                }
            }
        }

        public void ResolveReference()
        {
            if (_cuvImporter == default)
            {
                var path = GetImporterPath();
                if (!string.IsNullOrEmpty(path))
                {
                    _cuvImporter = AssetDatabaseHelper.LoadAsset<UnipromCuvImporter>(path);
                }
            }

            if (_cuvImporter != default)
            {
                _cuvImporter.SetSettings(this);
            }
        }

        public bool CanIStartBuild()
        {
            if (string.IsNullOrEmpty(_releaseRemoteLoadUrl))
            {
                UnipromDebug.LogError("Release Remote Load Url must be set.");
                return false;
            }
            if (string.IsNullOrEmpty(_overridePlayerVersion))
            {
                UnipromDebug.LogError("Override Player Version must be specified. If you do not know, specify 1.0.0.");
                return false;
            }
            return true;
        }
        
        public void BuildTest(bool isBatchMode = false)
        {
            UnipromDebug.IsBatchMode = Application.isBatchMode;
            if (string.IsNullOrEmpty(_testRemoteLoadUrl))
            {
                UnipromDebug.LogError("Enter the Test Remote Load Url.");
                return;
            }

            if (!isBatchMode)
            {
                CopyToItemsIfNeeded();
            }
            
            UnipromDebug.Log("Start Build-Test");
            
            var settingsPath = GetSettingsPath();
            AssetDatabaseHelper.CreateDirectory(settingsPath);
            var obj = AssetDatabaseHelper.CreateOrLoadAsset<UnipromSettings>(settingsPath);
            obj.hideFlags = HideFlags.NotEditable;
            obj.Set(UnipromBuildType.Test, _testRemoteLoadUrl, _overridePlayerVersion, _reference);
            _settings = obj;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);

            SetUpAddressable();
            UnipromDebug.Log("Build Path:" + settingsPath);
            AddressableHelper.StartNewBuildByProfileName(TestProfileName);
            if (!isBatchMode && !Application.isBatchMode)
            {
                AddressableHelper.ChangePlayMode<BuildScriptFastMode>();
            }
        }

        public void BuildRelease(bool isBatchMode = false)
        {
            UnipromDebug.IsBatchMode = Application.isBatchMode;
            if (string.IsNullOrEmpty(_releaseRemoteLoadUrl))
            {
                UnipromDebug.LogError("Enter the Release Remote Load Url.");
                return;
            }

            if (!isBatchMode)
            {
                CopyToItemsIfNeeded();
            }

            UnipromDebug.Log("Start Build-Release");
            
            var settingsPath = GetSettingsPath();
            AssetDatabaseHelper.CreateDirectory(settingsPath);
            var obj = AssetDatabaseHelper.CreateOrLoadAsset<UnipromSettings>(settingsPath);
            obj.hideFlags = HideFlags.NotEditable;
            obj.Set(UnipromBuildType.Release, _releaseRemoteLoadUrl, _overridePlayerVersion, _reference);
            _settings = obj;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);

            SetUpAddressable();
            UnipromDebug.Log("Build Path:" + settingsPath);
            AddressableHelper.StartNewBuildByProfileName(ReleaseProfileName);
            if (!isBatchMode && !Application.isBatchMode)
            {
                AddressableHelper.ChangePlayMode<BuildScriptFastMode>();
            }
        }

        void SetUpAddressable()
        {
            AddressableHelper.EnableBuildRemoteCatalog(_overridePlayerVersion);
            AddressableHelper.CreateProfileIfNeeded(ReleaseProfileName, ReleaseRemoteBuildPath, _releaseRemoteLoadUrl, true);
            AddressableHelper.CreateProfileIfNeeded(TestProfileName, TestRemoteBuildPath, _testRemoteLoadUrl, false);
        }

        static void CopyToItemsIfNeeded()
        {
            CopyToInterstitialPrefabIfNeeded();
            CopyToWallPrefabIfNeeded();
            CopyToSampleSceneIfNeeded();
        }
        
        public static void CopyToInterstitialPrefabIfNeeded()
        {
            var path = GetInterstitialPrefabPath();
            if (AssetDatabaseHelper.IsAttachedToPrefab<IUnipromInterstitialView>(path))
            {
                return;
            }
            AssetDatabaseHelper.CreateDirectory(path);
            AssetDatabase.CopyAsset(InterstitialPackagePrefabPath, path);
            AssetDatabase.Refresh();
            var guid = AssetDatabase.AssetPathToGUID(path);
            if (guid != default)
            {
                AddressableHelper.AddToDefaultByGuid(guid, InterstitialAddressableKey);
            }
        }
        
        public static void CopyToWallPrefabIfNeeded()
        {
            var path = GetWallPrefabPath();
            if (AssetDatabaseHelper.IsAttachedToPrefab<IUnipromWallView>(path))
            {
                return;
            }
            AssetDatabaseHelper.CreateDirectory(path);
            AssetDatabase.CopyAsset(WallPackagePrefabPath, path);
            AssetDatabase.Refresh();
            var guid = AssetDatabase.AssetPathToGUID(path);
            if (guid != default)
            {
                AddressableHelper.AddToDefaultByGuid(guid, WallAddressableKey);
            }
        }
        
        static void CopyToSampleSceneIfNeeded()
        {
            var path = GetSampleScenePath();
            AssetDatabaseHelper.CreateDirectory(path);
            AssetDatabase.CopyAsset(SamplePackageScenePath, path);
            AssetDatabase.Refresh();
        }
        
        public bool HasSettings() => _settings != default;
        
        public UnipromBuildType GetBuildType() => _settings.BuildType;
        
        public bool CanISendTestServer()
        {
            if (string.IsNullOrEmpty(_testFtpSettingPath))
            {
                UnipromDebug.LogError("Set the \"FTP Settings path\".");
                return false;
            }
            if (!File.Exists(_testFtpSettingPath))
            {
                UnipromDebug.LogError("Please specify the correct \"FTP Settings path\".");
                return false;
            }
            if (_settings == default)
            {
                UnipromDebug.LogError("Settings does not exist. Please execute \"Build\".");
                return false;
            }
            if (_settings.BuildType != UnipromBuildType.Test)
            {
                UnipromDebug.LogError("Execute \"Build - Test\".");
                return false;
            }
            var rootPath = AddressableHelper.GetRemoteBuildPath();
            if (!AssetDatabaseHelper.IsDirectoryContainsAnyFile(rootPath))
            {
                UnipromDebug.LogError("Asset file does not exist. Execute \"Build - Test\".");
                return false;
            }
            return true;
        }

        public void SendToTestServer()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(_testFtpSettingPath);
            SendToReleaseServer(asset.text)
                .SafeContinueWith(task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        AddressableHelper.ChangePlayMode<BuildScriptPackedPlayMode>();
                    }
                    else if(task.Exception != default)
                    {
                        UnipromDebug.LogError("SendToTestServer error: " + task.Exception.ToString());
                    }
                });
        }
        
        public Task SendToTestServer(string ftpJsonString) => SendToServer(_testRemoteLoadUrl, _overridePlayerVersion, ftpJsonString);
        
        public bool CanISendReleaseServer()
        {
            if (string.IsNullOrEmpty(_releaseFtpSettingPath))
            {
                UnipromDebug.LogError("Set the \"FTP Settings path\".");
                return false;
            }
            if (!File.Exists(_releaseFtpSettingPath))
            {
                UnipromDebug.LogError("Please specify the correct \"FTP Settings path\".");
                return false;
            }
            if (_settings == default)
            {
                UnipromDebug.LogError("Settings does not exist. Please execute \"Build\".");
                return false;
            }
            if (_settings.BuildType != UnipromBuildType.Release)
            {
                UnipromDebug.LogError("Execute \"Build - Release\".");
                return false;
            }
            var rootPath = AddressableHelper.GetRemoteBuildPath();
            if (!AssetDatabaseHelper.IsDirectoryContainsAnyFile(rootPath))
            {
                UnipromDebug.LogError("Asset file does not exist. Execute \"Build - Release\".");
                return false;
            }
            return true;
        }

        public void SendToReleaseServer()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(_releaseFtpSettingPath);
            SendToReleaseServer(asset.text)
                .SafeContinueWith(task =>
            {
                if (task.Status == TaskStatus.RanToCompletion)
                {
                    AddressableHelper.ChangePlayMode<BuildScriptPackedPlayMode>();
                }
                else if(task.Exception != default)
                {
                    UnipromDebug.LogError("SendToReleaseServer error: " + task.Exception.ToString());
                }
            });
        }
        
        public Task SendToReleaseServer(string ftpJsonString) => SendToServer(_releaseRemoteLoadUrl, _overridePlayerVersion, ftpJsonString);

        public async Task SendToServer(string remoteLoadUrl, string catalogVersion, string ftpSettingsJsonString)
        {
            UnipromDebug.IsBatchMode = Application.isBatchMode;
            var rootPath = AddressableHelper.GetRemoteBuildPath();
            var files = AddressableHelper.GetFilesForServerUpload(remoteLoadUrl, catalogVersion);
            var uploader = new FtpUploader(ftpSettingsJsonString);
            try
            {
                IsSendingFiles = true;
                await uploader.UploadFilesAsync(rootPath, files);
            }
            finally
            {
                IsSendingFiles = false;
            }
        }
        
        public bool CanICreatePackage()
        {
            if (_settings == default)
            {
                UnipromDebug.LogError("Settings does not exist. Please execute \"Build\".");
                return false;
            }
            if (_settings.BuildType != UnipromBuildType.Release)
            {
                UnipromDebug.LogError("Execute \"Build - Release\".");
                return false;
            }
            return true;
        }

        public void CreatePackage(string directory)
        {
            var assetPaths = new HashSet<string>
            {
                AssetDatabase.GetAssetPath(_reference),
                AssetDatabase.GetAssetPath(_settings),
                GetSampleScenePath()
            };
            var savePath = Path.Combine(directory, "UnipromInitSettings.unitypackage");
            AssetDatabase.ExportPackage(assetPaths.ToArray(), savePath, ExportPackageOptions.Interactive);
            UnipromDebug.Log("Package Exported successfully path:" + savePath);
        }

        string GetImporterPath()
        {
            var dir = default(string);
            if (_cuvImporter != default
                && !string.IsNullOrEmpty(_cuvImporter.BuildPath))
            {
                dir = _cuvImporter.BuildPath;
            }
            else
            {
                try
                {
                    dir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(this));
                }
                catch (ArgumentException)
                {
                    return string.Empty;
                }
            }

            return Path.Combine(dir, ImporterName);
        }

        public static string GetSettingsPath()
        {
            var dir = Path.GetDirectoryName(ExporterPath);
            return Path.Combine(dir, "Resources", UnipromSettings.SettingsName + ".asset");
        }
        
        static string GetInterstitialPrefabPath(string rootPath) => Path.Combine(rootPath, "prefabs", InterstitialPrefabName);
        
        static string GetWallPrefabPath(string rootPath) => Path.Combine(rootPath, "prefabs", WallPrefabName);
        
        static string GetSampleScenePath(string rootPath) => Path.Combine(rootPath, SampleSceneName);

        public static string GetInterstitialPrefabPath() => GetInterstitialPrefabPath(Path.GetDirectoryName(ExporterPath));
        
        public static string GetWallPrefabPath() => GetWallPrefabPath(Path.GetDirectoryName(ExporterPath));
        
        public static string GetSampleScenePath() => GetSampleScenePath(Path.GetDirectoryName(ExporterPath));
#endif
    }
}
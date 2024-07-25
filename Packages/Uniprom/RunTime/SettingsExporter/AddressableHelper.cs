#if UNITY_EDITOR && ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.Exceptions;
using ArgumentException = System.ArgumentException;

namespace Uniprom.Addressable.Editor
{
    public static class AddressableHelper
    {
        public const string BuildTargetPlaceholder = "[BuildTarget]";
        
        public static void EnableBuildRemoteCatalog(string overridePlayerVersion)
        {
            var settings = GetSettings();
            settings.BuildRemoteCatalog = true;
            settings.RemoteCatalogBuildPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteBuildPath);
            settings.RemoteCatalogLoadPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteLoadPath);
            settings.OverridePlayerVersion = overridePlayerVersion;
        }
        
        public static void CreateProfileIfNeeded(string profileName, string remoteBuildPath, string remoteLoadPath, bool isActive)
        {
            var settings = GetSettings();
            var profileSettings = settings.profileSettings;
            var profileId = profileSettings.GetProfileId(profileName);
            var mEvent = default(AddressableAssetSettings.ModificationEvent);
            if (string.IsNullOrEmpty(profileId))
            {
                mEvent = AddressableAssetSettings.ModificationEvent.EntryAdded;
                Debug.Log(profileName + " profile generated.");
                profileId = profileSettings.AddProfile(profileName, default);
            }
            else
            {
                mEvent = AddressableAssetSettings.ModificationEvent.EntryModified;
            }
            
            profileSettings.SetValue(profileId, AddressableAssetSettings.kRemoteBuildPath, EnsureBuildTargetPlaceholder(remoteBuildPath));
            profileSettings.SetValue(profileId, AddressableAssetSettings.kRemoteLoadPath, EnsureBuildTargetPlaceholder(remoteLoadPath));
            if (isActive)
            {
                settings.activeProfileId = profileId;
            }
            settings.SetDirty(mEvent, profileId, true, true);

            static string EnsureBuildTargetPlaceholder(string path)
            {
                if (path.EndsWith(BuildTargetPlaceholder))
                {
                    return path;
                }
                if (!path.EndsWith("/"))
                {
                    path += "/";
                }
                path += BuildTargetPlaceholder;
                return path;
            }
        }
        
        public static Dictionary<AddressableAssetEntry, List<AddressableAssetEntry>> GetGatherModifiedEntries()
        {
            var settings = GetSettings();
            var path = GetContentStateDataPath();
            return ContentUpdateScript.GatherModifiedEntriesWithDependencies(settings, path);
        }
        
        public static void UpdatePreviousBuild()
        {
            var settings = GetSettings();
            var path = GetContentStateDataPath();
            ContentUpdateScript.BuildContentUpdate(settings, path);
        }
        
        public static void StartNewBuildByProfileName(string profileName)
        {
            var settings = GetSettings();
            var profileId = settings.profileSettings.GetProfileId(profileName);
            StartNewBuildByProfileID(settings, profileId);
        }

        public static void StartNewBuildByProfileID(string profileID)
            => StartNewBuildByProfileID(GetSettings(), profileID);
        
        public static void StartNewBuildByProfileID(AddressableAssetSettings settings, string profileID)
        {
            settings.activeProfileId = profileID;
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.ActiveProfileSet, profileID, true, true);
            var builder = AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder;
            AddressableAssetSettings.CleanPlayerContent(builder);
            AddressableAssetSettings.BuildPlayerContent();
        }

        public static ContentCatalogData GetCatalogDataByPath(string catalogPath)
        {
            var jsonString = File.ReadAllText(catalogPath);
            return JsonUtility.FromJson<ContentCatalogData>(jsonString);
        }
        
        public static string[] GetFilesForServerUpload(string remoteLoadUrl, string catalogVersion)
        {
            remoteLoadUrl = remoteLoadUrl.TrimEnd('/');
            var remoteBuildPath = GetRemoteBuildPath();
            var rootPath = Path.GetDirectoryName(remoteBuildPath);
            var catalogPath = UnipromSettings.GetCatalogPath(rootPath, catalogVersion);
            if (!File.Exists(catalogPath))
            {
                throw new OperationException("catalog.json does not exist. path:" + catalogPath + "assetsCatalogVersion:" + catalogVersion + " remoteLoadUrl:" + remoteLoadUrl + " remoteBuildPath:" + remoteBuildPath);
            }
            var catalog = GetCatalogDataByPath(catalogPath);
            if (catalog.InternalIds.Length == 0)
            {
                return Array.Empty<string>();
            }
            
            var files = catalog.InternalIds
                .Where(x => x.StartsWith(remoteLoadUrl))
                .Select(x => x.Replace(remoteLoadUrl, rootPath));
            
            return new []
                {
                    catalogPath,
                    catalogPath.Replace(".json", ".hash")
                }
                .Concat(files)
                .ToArray();
        }
        
        public static string GetRemoteBuildPath()
        {
            var settings = GetSettings();
            return GetRemoteBuildPathByProfileId(settings, settings.activeProfileId);
        }

        public static string GetRemoteBuildPathByProfileName(string profileName)
        {
            var settings = GetSettings();
            var profileSettings = settings.profileSettings;
            var profileId = profileSettings.GetProfileId(profileName);
            return GetRemoteBuildPathByProfileId(settings, profileId);
        }

        public static string GetRemoteBuildPathByProfileId(string profileId)
            => GetRemoteBuildPathByProfileId(GetSettings(), profileId);
        
        public static string GetRemoteBuildPathByProfileId(AddressableAssetSettings settings, string profileId)
        {
            var profileSettings = settings.profileSettings;
            if (string.IsNullOrEmpty(profileId))
            {
                throw new OperationException("No active profile exists.");
            }
            var remoteBuildPath = profileSettings.GetValueByName(profileId, AddressableAssetSettings.kRemoteBuildPath);
            var directoryName = Path.GetDirectoryName(Application.dataPath);
            return Path.Combine(directoryName, remoteBuildPath.Replace(BuildTargetPlaceholder, UnipromSettings.GetPlatform()));
        }

        public static void ChangePlayMode<T>() where T : BuildScriptBase
        {
            var settings = GetSettings();
            if (settings.ActivePlayModeDataBuilder is T)
            {
                return;
            }
            for (var i = 0; i < settings.DataBuilders.Count; i++)
            {
                if (settings.GetDataBuilder(i) is not T)
                {
                    continue;
                }
                ProjectConfigData.ActivePlayModeIndex = i;
                Debug.Log("Play Mode Script was changed to " + typeof(T));
                return;
            }
            Debug.LogError("Play Mode Script that does not exist in DataBuilders. type: " + typeof(T));
        }

        public static bool HasAsset(UnityEngine.Object obj)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            return !string.IsNullOrEmpty(path)
                   && HasAssetByPath(path);
        }
        
        public static bool HasAssetByPath(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }
            
            var guid = AssetDatabase.AssetPathToGUID(path);
            return !string.IsNullOrEmpty(guid)
                   && HasAssetByGuid(guid);
        }
        
        public static bool HasAssetByGuid(string guid)
        {
            var settings = GetSettings();
            var entry = settings.FindAssetEntry(guid);
            return entry != default;
        }

        public static void AddToDefault(UnityEngine.Object obj, string newAddress)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            AddToDefaultByPath(path, newAddress);
        }
        
        public static void AddToDefaultByPath(string path, string newAddress)
        {
            var guid = AssetDatabase.AssetPathToGUID(path);
            AddToDefaultByGuid(guid, newAddress);
        }
        
        public static void AddToDefaultByGuid(string guid, string newAddress)
        {
            var settings = GetSettings();
            AddTo(settings, settings.DefaultGroup, guid, newAddress);
        }
        
        public static void AddTo(AddressableAssetSettings settings, AddressableAssetGroup group, string guid, string newAddress)
        {
            var entry = settings.CreateOrMoveEntry(guid, group, false, false);
            entry.address = newAddress;

            if (entry.labels is {Count: > 0})
            {
                var labelList = new List<string>(entry.labels);
                foreach (var label in labelList)
                {
                    if (!string.IsNullOrEmpty(label))
                    {
                        entry.SetLabel(label, false);
                    }
                }
            }
            var entries = new List<AddressableAssetEntry> { entry };
            group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entries, false, true);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entries, true, false);
        }
        
        public static bool IsInstalled()
            => AddressableAssetSettingsDefaultObject.Settings != default;
        
        public static AddressableAssetSettings GetSettings()
        {
            if (!IsInstalled())
            {
                throw new InvalidOperationException("Addressables is not installed or has not been initially configured.");
            }
            return AddressableAssetSettingsDefaultObject.Settings;
        }
        
        public static string GetContentStateDataPath()
        {
            var path = ContentUpdateScript.GetContentStateDataPath(false);
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("addressables_content_state.bin is empty.");
            }
            return path;
        }
    }
}
#endif
#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
#endif

namespace Uniprom
{
    public sealed class UnipromSettings : ScriptableObject
    {
        public const string Label = "UnipromItems";
        public const string SettingsName = "UnipromSettings";
        public const string ResourcesPath = SettingsName;
        
        [SerializeField] UnipromBuildType _buildType;
        [SerializeField] string _remoteLoadUrl;
        [SerializeField] string _assetsCatalogVersion;
        [SerializeField] ScriptableObject _scriptableObject;
        
        public bool IsInitialized => Reference.IsInitialized;
        public UnipromBuildType BuildType => _buildType;
        public IUnipromReference Reference => _reference ??= _scriptableObject as IUnipromReference;
        public string RemoteCatalogUrl => GetCatalogPath(_remoteLoadUrl, _assetsCatalogVersion);
        
        IUnipromReference _reference;
        
        internal void Set(
            UnipromBuildType buildType,
            string remoteLoadUrl,
            string assetsCatalogVersion,
            ScriptableObject scriptableObject)
        {
            _buildType = buildType;
            _remoteLoadUrl = remoteLoadUrl;
            _assetsCatalogVersion = assetsCatalogVersion;
            _scriptableObject = scriptableObject;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
#endif
        }

        public IEnumerator Initialize()
        {
#if !UNIPROM_SOURCE_PROJECT
            var loadCatalogHandle = Addressables.LoadContentCatalogAsync(RemoteCatalogUrl, true);
            yield return loadCatalogHandle;
#endif
            
            var downloadSizeHandle = Addressables.GetDownloadSizeAsync(Label);
            yield return downloadSizeHandle;
            
            if (downloadSizeHandle.Result > 0)
            {
                var downloadDependencies = Addressables.DownloadDependenciesAsync(Label);
                yield return downloadDependencies;
            }
            
            yield return Reference.Initialize();
        }
        
        public async Task InitializeAsync()
        {
#if !UNIPROM_SOURCE_PROJECT
            var loadCatalogHandle = Addressables.LoadContentCatalogAsync(RemoteCatalogUrl, true);
            await loadCatalogHandle.Task;
#endif

            // var handle = Addressables.LoadResourceLocationsAsync(Label);
            // await handle.Task;
            // if (handle.Status == AsyncOperationStatus.Succeeded) 
            // {
            //     var locations = handle.Result;
            //     if (locations is {Count: > 0}) 
            //     {
            //         foreach (var location in locations) 
            //         {
            //             var loadHandle = Addressables.LoadAssetAsync<Object>(location);
            //             await loadHandle.Task;
            //             Debug.Log(location.InternalId + " Status: " + loadHandle.Status );
            //             Addressables.Release(loadHandle);
            //         }
            //     }
            //     else
            //     {
            //         Debug.LogWarning("Could not find any locations for this asset reference.");
            //     }
            // }
            // else
            // {
            //     Debug.LogError("Could not load resource locations.");
            // }
            //
            var downloadSizeHandle = Addressables.GetDownloadSizeAsync(Label);
            await downloadSizeHandle.Task;

            Debug.Log("Download Size:" + downloadSizeHandle.Result);
            
            if (downloadSizeHandle.Result > 0)
            {
                var downloadDependencies = Addressables.DownloadDependenciesAsync(Label);
                await downloadDependencies.Task;
            }
            
            await Reference.InitializeAsync();
        }
        
        public static UnipromSettings Current
        {
            get {
                if (s_current == default)
                {
                    s_current = Resources.Load<UnipromSettings>(ResourcesPath);
                }
                return s_current;
            }
        }
        [NonSerialized] static UnipromSettings s_current;

        public static string GetCatalogPath(string remoteUrl, string catalogVersion) 
            => Path.Combine(remoteUrl, GetPlatform(), $"catalog_{catalogVersion}.json");
        
        public static string GetPlatform()
        {
#if UNITY_IOS
            return "iOS";
#else
            return "Android";
#endif
        }
    }
}
#endif
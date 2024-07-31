#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Networking;
using Uniprom.Addressable.Editor;
using UnityEditor.AddressableAssets.Build.DataBuilders;
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
#if UNIPROM_SOURCE_PROJECT && UNITY_EDITOR
            AddressableHelper.ChangePlayMode<BuildScriptFastMode>();
            
            var request = UnityWebRequest.Head(RemoteCatalogUrl);
            yield return request.SendWebRequest();

            var hasCatalog = request.result == UnityWebRequest.Result.Success
                             && request.responseCode != 404;
            Debug.Log(hasCatalog ? "Loading RemoteCatalog" : "Using Locale's RemoteCatalog");

            if(hasCatalog)
#endif
            {
                var loadCatalogHandle = Addressables.LoadContentCatalogAsync(RemoteCatalogUrl, true);
                yield return loadCatalogHandle;
            }

            var label = Label + "_" + Reference.FindLanguage(Application.systemLanguage);
            var downloadSizeHandle = Addressables.GetDownloadSizeAsync(label);
            yield return downloadSizeHandle;

#if DEBUG
            Debug.Log("Addressables assets download size: " +  ((downloadSizeHandle.Result / 1024f) / 1024f) + "MB | Label: " + label);
#endif

            if (downloadSizeHandle.Result > 0)
            {
                var downloadDependencies = Addressables.DownloadDependenciesAsync(label, true);
                yield return downloadDependencies;
            }

            yield return Reference.Initialize();
        }
        
        public async Task InitializeAsync()
        {
#if UNIPROM_SOURCE_PROJECT && UNITY_EDITOR
            AddressableHelper.ChangePlayMode<BuildScriptFastMode>();

            var request = UnityWebRequest.Head(RemoteCatalogUrl);
            await Task.Yield();
            request.SendWebRequest();

            while (!request.isDone)
            {
                await Task.Delay(100);
            }
            
            var hasCatalog = request.result == UnityWebRequest.Result.Success
                             && request.responseCode != 404;
            Debug.Log(hasCatalog ? "Loading RemoteCatalog" : "Using Locale's RemoteCatalog");

            if(hasCatalog)
#endif
            {
                var loadCatalogHandle = Addressables.LoadContentCatalogAsync(RemoteCatalogUrl, true);
                await loadCatalogHandle.Task;
            }

            var label = Label + "_" + Reference.FindLanguage(Application.systemLanguage);
            var downloadSizeHandle = Addressables.GetDownloadSizeAsync(label);
            await downloadSizeHandle.Task;

#if DEBUG
            Debug.Log("Addressables assets download size: " +  ((downloadSizeHandle.Result / 1024f) / 1024f) + "MB | Label: " + label);
#endif
            
            if (downloadSizeHandle.Result > 0)
            {
                var downloadDependencies = Addressables.DownloadDependenciesAsync(label, true);
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
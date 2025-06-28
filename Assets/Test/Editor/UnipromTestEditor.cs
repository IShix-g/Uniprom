#if ENABLE_CMSUNIVORTEX
using System.Collections.Generic;
using System.Linq;
using Uniprom;
using Uniprom.Addressable.Editor;
using Uniprom.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Tests
{
    public sealed class UnipromTestEditor
    {
        // TODO : Addressables Groupに含まれるPrefabの影響で再生しても問題が出るのをなんとかしたい。
        [MenuItem("Tests/Get All Group")]
        static void GetAllGroup()
        {
            var settings = AddressableHelper.GetSettings();
            var defaultGroupName = settings.DefaultGroup.Name;
            var groups = AddressableHelper.GetGroups(settings, new HashSet<string>() { AddressableAssetSettings.PlayerDataGroupName, defaultGroupName });
            Debug.Log(groups.Select(x => x.Name).Aggregate((a, b) => a + "\n" + b));
        }
        
        [MenuItem("Tests/Build Asset Test - Release")]
        static void BuildAssetTestRelease()
        {
            var options = new Dictionary<string, string>() {{AssetBuilder.FtpJsonStringName, "Assets/FtpSettings/ftp-setting-release.json"}};
            AssetBuilder.Build(true, options);
        }
        
        [MenuItem("Tests/Build Asset Test - Test")]
        static void BuildAssetTestTest()
        {
            var options = new Dictionary<string, string>() {{AssetBuilder.FtpJsonStringName, "Assets/FtpSettings/ftp-setting-test.json"}};
            AssetBuilder.Build(false, options);
        }
        
        [MenuItem("Tests/Create Canvas")]
        static void CreateCanvas()
        {
            Debug.Log(EventSystem.current);
            {
                var go = new GameObject("Uniprom Canvas", new []{ typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster) });
                var canvas = go.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.pixelPerfect = false;
                canvas.sortingOrder = 99;
                canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;
                var scaler = go.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                scaler.referencePixelsPerUnit = 100;
                var rayCaster = go.GetComponent<GraphicRaycaster>();
                rayCaster.ignoreReversedGraphics = true;
                rayCaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
                rayCaster.blockingMask = ~0;
            }
            
            if (EventSystem.current == default)
            {
                _ = new GameObject("EventSystem", new []{ typeof(EventSystem), typeof(StandaloneInputModule) });
            }
            Debug.Log(EventSystem.current.name);
        }
        
        [MenuItem("Tests/Copy To Prefab")]
        static void CopyToPrefab()
        {
            var path = "Assets/Uniprom/Prefabs/UnipromInterstitial.prefab";
            AssetDatabaseHelper.CreateDirectory(path);
            AssetDatabase.CopyAsset(
                "Packages/com.ishix.uniprom/RunTime/Prefabs/UnipromInterstitial.prefab",
                path
            );
            AssetDatabase.Refresh();
            
            var guid = AssetDatabase.AssetPathToGUID(path);
            if (guid != default)
            {
                AddressableHelper.AddToDefaultByGuid(guid, UnipromSettingsExporter.InterstitialAddressableKey);
            }
            Debug.Log(AssetDatabaseHelper.IsAttachedToPrefab<IUnipromInterstitialView>(path));
        }
        
        [MenuItem("Tests/Get Content Catalog Data")]
        static void GetContentCatalogData()
        {
            var remoteLoadUrl = "https://devx.myonick.biz/uniprom/addressables/release/";
            var catalogVersion = "1.0.0";
            Debug.Log(AddressableHelper.GetFilesForServerUpload(remoteLoadUrl, catalogVersion).Aggregate((a, b) => a + "\n" + b));
        }
        
        [MenuItem("Tests/Get All Build Paths")]
        static void GetAllBuildPaths()
        {
            var rootPath = AddressableHelper.GetRemoteBuildPath();
            var files = AssetDatabaseHelper.GetAllAssetsPath(rootPath);
            Debug.Log(files.Aggregate((a, b) => a + "\n" + b));
        }
        
        [MenuItem("Tests/Load Ftp Setting")]
        static void LoadFtpSetting()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/UnipromTest/ftpSetting.json");
            var obj = JsonUtility.FromJson<FtpUploader.ServerInfo>(asset.text);
            Debug.Log(obj.ServerAddress);
        }
        
        [MenuItem("Tests/Upload To Server")]
        static async void UploadToServer()
        {
            var rootPath = AddressableHelper.GetRemoteBuildPath();
            var files = AssetDatabaseHelper.GetAllAssetsPath(rootPath);
            
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/UnipromTest/ftpSetting.json");
            var obj = JsonUtility.FromJson<FtpUploader.ServerInfo>(asset.text);
            
            var uploader = new FtpUploader(obj);
            await uploader.UploadFilesAsync(rootPath, files);
        }

        [MenuItem("Tests/Create And Set Profile")]
        static void CreateAndSetProfile()
        {
            AddressableHelper.CreateProfileIfNeeded("Uniprom-Test", "UnipromReleaseServerData", "https://devx.myonick.biz/uniprom/addressables/test/[BuildTarget]", true);
            AddressableHelper.CreateProfileIfNeeded("Uniprom-Release", "UnipromTesetServerData", "https://devx.myonick.biz/uniprom/addressable/release/[BuildTarget]", false);
        }

        [MenuItem("Tests/Enable Build Remote Catalog")]
        public static void EnableBuildRemoteCatalog()
        {
            AddressableHelper.EnableBuildRemoteCatalog("1.0.0");
        }

        [MenuItem("Tests/Start New Build")]
        static void StartNewBuild()
        {
            AddressableHelper.StartNewBuildByProfileName("Uniprom-Test");
        }

        [MenuItem("Tests/Get Gather Modified Entries")]
        static void GetGatherModifiedEntries()
        {
            var entrys = AddressableHelper.GetGatherModifiedEntries();
            if (entrys.Count > 0)
            {
                Debug.Log(entrys.Select(x => x.Key.address).Aggregate((a, b) => a + ", " + b)); 
            }
            else
            {
                Debug.Log("No entries changed.");
            }
        }
        
        [MenuItem("Tests/Update Previous Build")]
        static void UpdatePreviousBuild()
        {
            AddressableHelper.UpdatePreviousBuild();
        }
        
        [MenuItem("Tools/Change Play Mode")]
        public static void ChangePlayMode()
        {
            AddressableHelper.ChangePlayMode<BuildScriptPackedPlayMode>();
        }
    }
}
#endif
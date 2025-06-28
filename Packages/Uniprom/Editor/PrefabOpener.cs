#if ENABLE_CMSUNIVORTEX
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Uniprom.Editor
{
    public static class PrefabOpener
    {
        const string _interstitialMenuName = "Window/Uniprom/open Interstitial prefab";
        const string _wallMenuName = "Window/Uniprom/open Wall prefab";
        
        [MenuItem(_interstitialMenuName)]
        public static void OpenInterstitial()
        {
            var path = UnipromSettingsExporter.GetDefaultInterstitialPrefabPath();

            if (!EditorUtility.DisplayDialog("Open a Interstitial prefab", "May I open a Interstitial prefab? \nPath: " + path, "Open", "Close"))
            {
                return;
            }
            
            if (File.Exists(path))
            {
                EditorUtility.FocusProjectWindow();
                var asset = AssetDatabase.LoadMainAssetAtPath(path);
                AssetDatabase.OpenAsset(asset);
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
            else
            {
                Debug.LogError("Could not open because the file does not exist. Please execute Build with \"Window > Uniprom > open Settings Exporter\".");
            }
        }

        [MenuItem(_interstitialMenuName, isValidateFunction:true)]
        public static bool IsValidInterstitial()
        {
            var path = UnipromSettingsExporter.GetDefaultInterstitialPrefabPath();
            return File.Exists(path);
        }

        [MenuItem(_wallMenuName)]
        public static void OpenWall()
        {
            var path = UnipromSettingsExporter.GetDefaultWallPrefabPath();

            if (!EditorUtility.DisplayDialog("Open a Wall prefab", "May I open a Wall prefab? \nPath: " + path, "Open", "Close"))
            {
                return;
            }
            
            if (File.Exists(path))
            {
                EditorUtility.FocusProjectWindow();
                var asset = AssetDatabase.LoadMainAssetAtPath(path);
                AssetDatabase.OpenAsset(asset);
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
            else
            {
                Debug.LogError("Could not open because the file does not exist. Please execute Build with \"Window > Uniprom > open Settings Exporter\".");
            }
        }

        [MenuItem(_wallMenuName, isValidateFunction:true)]
        public static bool IsValidWall()
        {
            var path = UnipromSettingsExporter.GetDefaultWallPrefabPath();
            return File.Exists(path);
        }
    }
}
#endif
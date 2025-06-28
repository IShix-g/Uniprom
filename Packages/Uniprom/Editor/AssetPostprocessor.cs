
using System;
using UnityEngine;

namespace Uniprom.Editor
{
    public sealed class AssetPostprocessor : UnityEditor.AssetPostprocessor
    {    
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
#if ENABLE_CMSUNIVORTEX
            if (importedAssets.Length == 0)
            {
                return;
            }
            
            var settingsPath = UnipromSettingsExporter.GetSettingsPath();
            foreach(var assetPath in importedAssets)
            {
                if (assetPath != settingsPath)
                {
                    continue;
                }
                try
                {
                    UnipromSettingsExporter.CopyToInterstitialPrefabIfNeeded();
                    UnipromSettingsExporter.CopyToWallPrefabIfNeeded();
                    break;
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Prefab import failed. Please correct the error and perform \"Right Click > Reimport\". If you are not sure, delete the Uniprom directory and start over after fixing the problem.");
                    Debug.LogError(e);
                }
                return;
            }
#endif
        }
    }
}

using System.IO;
using UnityEngine;
using UnityEditor;

namespace Editor
{
    public class OpenSavePath : EditorWindow
    {
        [MenuItem("Tools/open path/persistent")]
        public static void Open()
        {
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                System.Diagnostics.Process.Start(Application.persistentDataPath);
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                EditorUtility.RevealInFinder(Application.persistentDataPath);
            }
        }

        [MenuItem("Tools/open path/temporary cache")]
        public static void OpenTmp()
        {
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                System.Diagnostics.Process.Start(Application.temporaryCachePath);
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                EditorUtility.RevealInFinder(Application.temporaryCachePath);
            }
        }
		
        [MenuItem("Tools/open path/default cache")]
        public static void OpenCache()
        {
            var cachePath = Caching.defaultCache.path;
            if (!Directory.Exists(cachePath))
            {
                Debug.LogWarning("cache path not found path: " + cachePath);
                return;
            }
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                System.Diagnostics.Process.Start(cachePath);
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                EditorUtility.RevealInFinder(cachePath);
            }
        }
    }
}
#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Uniprom.Editor
{
    public static class AssetDatabaseHelper
    {
        public static bool IsDirectoryContainsAnyFile(string path)
            => Path.HasExtension(path)
               || Directory.GetFiles(path).Length > 0;
        
        public static void CreateDirectory(string path)
        {
            if (!path.StartsWith("Assets"))
            {
                Debug.LogError("The path must start with 'Assets/' ");
                return;
            }

            path = Path.HasExtension(path) ? Path.GetDirectoryName(path) : path;
            path = path?.Replace("\\", "/");

            if (string.IsNullOrEmpty(path)
                || AssetDatabase.IsValidFolder(path))
            {
                return;
            }
    
            var folders = path.Split('/');
            var parentFolder = folders[0];
    
            for (var i = 1; i < folders.Length; i++)
            {
                var newFolder = parentFolder + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(newFolder))
                {
                    AssetDatabase.CreateFolder(parentFolder, folders[i]);
                }
                parentFolder = newFolder;
            }
        }
        
        public static bool IsAttachedToPrefab<T>(string prefabPath) where T : class
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (asset != default
                && asset.TryGetComponent<T>(out _))
            {
                return true;
            }
            return AssetDatabase.FindAssets("t:prefab", new[] { Path.GetDirectoryName(prefabPath) })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
                .Any(x => x.TryGetComponent<T>(out _));
        }
        
        [CanBeNull]
        public static T LoadAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != default)
            {
                return asset;
            }
            var currentGuids = AssetDatabase.FindAssets("t:" + typeof(T),
                new[] {Path.GetDirectoryName(path)});

            if (currentGuids is {Length: > 0})
            {
                path = AssetDatabase.GUIDToAssetPath(currentGuids[0]);
                asset = AssetDatabase.LoadAssetAtPath<T>(path);
            }
            return asset;
        }
        
        public static T CreateOrLoadAsset<T>(string path) where T : ScriptableObject
        {
            var asset = LoadAsset<T>(path);
            if (asset != default)
            {
                return asset;
            }
            
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            return asset;
        }
        
        public static void AddSymbol(BuildTargetGroup group, string symbol)
        {
            var symbols = default(string);
#if UNITY_2023_1_OR_NEWER
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(group);
            symbols = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
#else
            symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
#endif
            var symbolsList = symbols.Split(';').ToList();

            if (symbolsList.Contains(symbol))
            {
                return;
            }
            symbolsList.Add(symbol);
            symbols = string.Join(";", symbolsList.ToArray());
#if UNITY_2023_1_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, symbols);
#else
            symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, symbols);
#endif
        }
        
        public static string[] GetAllAssetsPath(string path)
        {
            if (Path.HasExtension(path))
            {
                path = Path.GetDirectoryName(path);
                path = path?.Replace("\\", "/");
            }

            return !string.IsNullOrEmpty(path)
                ? Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Where(x => !x.EndsWith(".meta"))
                    .Where(x => !x.EndsWith(".DS_Store"))
                    .Select(x => x.Replace(@"\", "/"))
                    .ToArray()
                : Array.Empty<string>();
        }
    }
}
#endif
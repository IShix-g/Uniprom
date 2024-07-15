
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Uniprom.Editor
{
    public static class SampleSceneOpener
    {
        const string _menuName = "Window/Uniprom/open Sample scene";
        
        [MenuItem(_menuName)]
        public static void Open()
        {
            var path = UnipromSettingsExporter.GetSampleScenePath();

            if (!EditorUtility.DisplayDialog("Open a sample scene", "May I open a sample scene? \nPath: " + path, "Open", "Close"))
            {
                return;
            }
            
            if (File.Exists(path))
            {
                EditorSceneManager.OpenScene(path);
                EditorUtility.FocusProjectWindow();
                var asset = AssetDatabase.LoadMainAssetAtPath(path);
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
            else
            {
                Debug.LogError("Could not open because the file does not exist. Please execute Build with \"Window > Uniprom > open Settings Exporter\".");
            }
        }

        [MenuItem(_menuName, isValidateFunction:true)]
        public static bool IsValid()
        {
            var path = UnipromSettingsExporter.GetSampleScenePath();
            return File.Exists(path);
        }
    }
}
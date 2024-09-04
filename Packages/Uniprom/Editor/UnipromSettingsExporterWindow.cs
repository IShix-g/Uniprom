
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Uniprom.Editor
{
    public sealed class UnipromSettingsExporterWindow : EditorWindow
    {
        UnityEditor.Editor _editor;
        Vector2 _scrollPos;
        
        [MenuItem("Window/Uniprom/open Settings Exporter")]
        public static void ShowWindow()
        {
            var window = GetWindow<UnipromSettingsExporterWindow>("Uniprom Settings Exporter");
            window.minSize = new Vector2(480, 600);
            window.Show();
        }
        
        void OnEnable()
        {
            if (!File.Exists(UnipromSettingsExporter.ExporterPath))
            {
                AssetDatabaseHelper.CreateDirectory(UnipromSettingsExporter.ExporterPath);
            }
            var obj = AssetDatabaseHelper.CreateOrLoadAsset<UnipromSettingsExporter>(UnipromSettingsExporter.ExporterPath);
            _editor = UnityEditor.Editor.CreateEditor(obj);
        }
        
        void OnGUI()
        {
            if (_editor != default)
            {
                _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));
                GUILayout.BeginVertical(new GUIStyle() { padding = new RectOffset(5, 5, 5, 5) });
                _editor.OnInspectorGUI();
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            else
            {
                Close();
            }
        }
    }
}
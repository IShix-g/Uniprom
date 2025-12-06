
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Uniprom.Editor
{
    public sealed class UnipromExporterWindow : EditorWindow
    {
        UnityEditor.Editor _editor;
        Vector2 _scrollPos;
        
        [MenuItem("Window/Uniprom/open Settings Exporter", false, 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<UnipromExporterWindow>("Uniprom Settings Exporter");
            window.minSize = new Vector2(480, 600);
            window.Show();
        }
        
        void OnEnable()
        {
            if (!File.Exists(UnipromExporter.ExporterPath))
            {
                AssetDatabaseHelper.CreateDirectory(UnipromExporter.ExporterPath);
            }
            var obj = AssetDatabaseHelper.CreateOrLoadAsset<UnipromExporter>(UnipromExporter.ExporterPath);
            _editor = UnityEditor.Editor.CreateEditor(obj);
        }
        
        void OnGUI()
        {
            if (_editor != default)
            {
                _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));
                GUILayout.BeginVertical(new GUIStyle() { padding = new RectOffset(5, 5, 5, 5) });
                if (_editor != null)
                {
                    _editor.OnInspectorGUI();
                }
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
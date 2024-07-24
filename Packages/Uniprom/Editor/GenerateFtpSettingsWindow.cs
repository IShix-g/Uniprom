
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Uniprom.Editor
{
    public class GenerateFtpSettingsWindow : EditorWindow
    {
        public static void ShowWindow()
        {
            var window = GetWindow<GenerateFtpSettingsWindow>("Generate Ftp Settings Window");
            window.Show();
            window.minSize = new Vector2(480, 480);
        }
        
        public static string SettingFilePath
        {
            get => EditorPrefs.GetString("UnipromSettingsEditor_SettingFilePath", Application.dataPath);
            set => EditorPrefs.SetString("UnipromSettingsEditor_SettingFilePath", value);
        }
        
        public static string SettingFileObject
        {
            get => EditorPrefs.GetString("UnipromSettingsEditor_SettingFileObject");
            set => EditorPrefs.SetString("UnipromSettingsEditor_SettingFileObject", value);
        }

        FtpUploader.ServerInfo _serverInfo = new ();

        string _fileName;
        
        void OnEnable()
        {
            _fileName = "New ftp-setting.json";
            if (!string.IsNullOrEmpty(SettingFileObject))
            {
                _serverInfo = JsonUtility.FromJson<FtpUploader.ServerInfo>(SettingFileObject);
            }
        }

        void OnDestroy()
        {
            if (!string.IsNullOrEmpty(_serverInfo.ServerAddress)
                || !string.IsNullOrEmpty(_serverInfo.UserName)
                || !string.IsNullOrEmpty(_serverInfo.ServerBaseDirectory))
            {
                _serverInfo.Password = string.Empty;
                SettingFileObject = JsonUtility.ToJson(_serverInfo);
            }
        }

        void OnGUI()
        {
            var boxStyle = new GUIStyle()
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
            GUILayout.BeginVertical(boxStyle);
            GUILayout.BeginVertical();
            {
                var style = new GUIStyle(GUI.skin.label)
                {
                    padding = new RectOffset(5, 5, 10, 20),
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 15
                };
                GUILayout.Label("Generate FTP configuration file", style);
            }
            GUILayout.EndVertical();
            
            _serverInfo.ServerAddress = EditorGUILayout.TextField("Server Address", _serverInfo.ServerAddress);
            _serverInfo.UserName = EditorGUILayout.TextField("Username", _serverInfo.UserName);
            _serverInfo.Password = EditorGUILayout.PasswordField("Password", _serverInfo.Password);
            _serverInfo.ServerBaseDirectory = EditorGUILayout.TextField("Server Base Directory", _serverInfo.ServerBaseDirectory);
            _serverInfo.EnableSSL = EditorGUILayout.Toggle("Enable SSL", _serverInfo.EnableSSL);

            GUILayout.Space(10);
            _fileName = EditorGUILayout.TextField("Json file name", _fileName);
            GUILayout.Space(10);
            
            EditorGUI.BeginDisabledGroup(!_serverInfo.IsValid()
                                         || string.IsNullOrEmpty(_fileName)
                                         || !_fileName.EndsWith(".json"));
            
            if (GUILayout.Button("Generate", GUILayout.Height(38)))
            {
                if (!Directory.Exists(SettingFilePath))
                {
                    SettingFilePath = Application.dataPath;
                }
                SettingFilePath = EditorUtility.OpenFolderPanel(
                    "Path to save the file",
                    SettingFilePath,
                    string.Empty);

                var index = SettingFilePath.IndexOf("Assets", System.StringComparison.Ordinal);

                if (index >= 0)
                {
                    var path = Path.Combine(SettingFilePath.Substring(index), _fileName);
                    var jsonString = JsonUtility.ToJson(_serverInfo, true);
                    File.WriteAllText(path, jsonString);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
                else
                {
                    SettingFilePath = Application.dataPath;
                    Debug.LogError("Specify the directory under the Assets directory.");
                }
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();
        }
    }
}
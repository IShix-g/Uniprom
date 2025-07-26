
using System;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;

#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using Uniprom.Addressable.Editor;
#endif

namespace Uniprom.Editor
{
    [CustomEditor(typeof(UnipromExporter))]
    public sealed class UnipromSettingsEditor : UnityEditor.Editor
    {
        const string _packageUrl = "https://raw.githubusercontent.com/IShix-g/Uniprom/main/Packages/Uniprom/package.json";
        const string _gitUrl = "https://github.com/IShix-g/Uniprom";
        const string _gitInstallUrl = _gitUrl + ".git?path=Packages/Uniprom";
        const string _packagePath = "Packages/com.ishix.uniprom/";
        const string _gitCuvInstallUrl =  "https://github.com/IShix-g/CMSuniVortex.git?path=Packages/CMSuniVortex#2.0.9";
        const string _cuvPackagePath = "Packages/com.ishix.cmsunivortex/";
        static readonly string[] s_propertiesToExclude = { "m_Script", "_reference", "_settings", "_releaseFtpSettingPath", "_testFtpSettingPath" };

        static string PackageSaveDir
        {
            get => EditorPrefs.GetString("UnipromSettingsEditor_PackageSavePath", Application.dataPath);
            set => EditorPrefs.SetString("UnipromSettingsEditor_PackageSavePath", value);
        }

        SerializedProperty _referenceProp;
        SerializedProperty _settingsProp;
        SerializedProperty _releaseFtpPathProp;
        SerializedProperty _testFtpPathProp;
        SerializedProperty _scriptProp;
        UnipromExporter _exporter;
        UnityEditor.Editor _cuvImporterEditor;
        Texture2D _logo;
        Texture2D _buildIcon;
        Texture2D _unityIcon;
        Texture2D _ftpIcon;
        CancellationTokenSource _tokenSource;
        string _currentVersion;
        bool _isNowLoading;
        bool _isCheckedCuvVersion;
        bool _isNeedUpdateCuvVersion;
        readonly PackageInstaller _packageInstaller = new ();

        void OnEnable()
        {
#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
            SetProperties();
            _exporter = (UnipromExporter) target;
            _exporter.ResolveReference();
            EditorApplication.delayCall += () => { _cuvImporterEditor = CreateEditor(_exporter.CuvImporter); };
#endif
            _logo = GetTexture("UnipromLogo");
            _buildIcon = GetTexture("UnipromBuildIcon");
            _unityIcon = GetTexture("UnipromUnityIcon");
            _ftpIcon = GetTexture("UnipromFtpIcon");
            _scriptProp = serializedObject.FindProperty("m_Script");
            _currentVersion = "v" + CheckVersion.GetCurrent(_packagePath);
            _isCheckedCuvVersion = false;
        }

        void OnDisable()
        {
            _tokenSource.SafeCancelAndDispose();
        }
        
        void SetProperties()
        {
            _referenceProp = serializedObject.FindProperty(s_propertiesToExclude[1]);
            _settingsProp = serializedObject.FindProperty(s_propertiesToExclude[2]);
            _releaseFtpPathProp = serializedObject.FindProperty(s_propertiesToExclude[3]);
            _testFtpPathProp = serializedObject.FindProperty(s_propertiesToExclude[4]);
        }
        
        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                var style = new GUIStyle(GUI.skin.label)
                {
                    padding = new RectOffset(5, 5, 5, 5),
                    alignment = TextAnchor.MiddleCenter,
                };
                GUILayout.Label(_logo, style, GUILayout.MinWidth(430), GUILayout.Height(75));
            }
            {
                var style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                };
                EditorGUILayout.LabelField(_currentVersion, style);
            }
            GUILayout.EndVertical();
            
#if !ENABLE_ADDRESSABLES
            EditorGUILayout.HelpBox("This plugin requires Addressables.", MessageType.Error);
            return;
#elif !UNITY_IOS && !UNITY_ANDROID
            EditorGUILayout.HelpBox("Please switch platforms from Build Settings. (iOS or Android only)", MessageType.Error);
            return;
#elif !UNIPROM_SOURCE_PROJECT
            EditorGUILayout.HelpBox("Uniprom changes the Addressable settings. Builds must be run in a dedicated source project.", MessageType.Error);
            {
                var style = new GUIStyle(GUI.skin.button)
                {
                    padding = new RectOffset(0, 0, 15, 15)
                };
                if (GUILayout.Button("This project is a source project.", style))
                {
                    AssetDatabaseHelper.AddSymbol(BuildTargetGroup.Android, UnipromExporter.SourceProjectSymbol);
                    AssetDatabaseHelper.AddSymbol(BuildTargetGroup.iOS, UnipromExporter.SourceProjectSymbol);
                }
            }
            return;
#elif !ENABLE_CMSUNIVORTEX
            {
                EditorGUILayout.HelpBox("This plugin requires CMSuniVortex.", MessageType.Error);
                var style = new GUIStyle(GUI.skin.button)
                {
                    padding = new RectOffset(0, 0, 10, 10)
                };
                
                if (!_isNowLoading
                    && GUILayout.Button("Install CMSuniVortex.", style))
                {
                    _isNowLoading = true;
                    _tokenSource = new CancellationTokenSource();
                    _packageInstaller.Install(
                            new []{ _gitCuvInstallUrl },
                            _tokenSource.Token
                        )
                        .Handled(() => _isNowLoading = false);
                }
                else if(_isNowLoading)
                {
                    GUILayout.Button("Now Installing...", style);
                }
            }
            return;
#else

            if (!_isCheckedCuvVersion)
            {
                _isCheckedCuvVersion = true;
                var current = CheckVersion.GetCurrent(_cuvPackagePath);
                var request = CheckVersion.GetVersionFromUrl(_gitCuvInstallUrl);
                _isNeedUpdateCuvVersion = IsVersionUpdated(current, request);
                if (_isNeedUpdateCuvVersion)
                {
                    Debug.LogWarning("CMSuniVortex version v" + current + " -> v" + request);
                }
            }

            if (_isNeedUpdateCuvVersion)
            {
                EditorGUILayout.HelpBox("CMSuniVortex needs to be updated.", MessageType.Error);
                var style = new GUIStyle(GUI.skin.button)
                {
                    padding = new RectOffset(0, 0, 10, 10)
                };
                
                if (!_isNowLoading
                    && GUILayout.Button("Update CMSuniVortex.", style))
                {
                    _isNowLoading = true;
                    _tokenSource = new CancellationTokenSource();
                    _packageInstaller.Install(
                            new []{ _gitCuvInstallUrl },
                            _tokenSource.Token
                        )
                        .ContinueOnMainThread(onSuccess: task =>
                        {
                            _isNeedUpdateCuvVersion = true;
                            _isNeedUpdateCuvVersion = false;
                        },
                        onCompleted: () => _isNowLoading = false);
                }
                else if(_isNowLoading)
                {
                    GUILayout.Button("Now Installing...", style);
                }
                return;
            }
            
            GUILayout.Space(10);
            ShowMenu();
            GUILayout.Space(10);
            
            serializedObject.UpdateIfRequiredOrScript();
            
            if (_exporter.CuvImporter != default)
            {
                if (_cuvImporterEditor != default)
                {
                    _cuvImporterEditor.OnInspectorGUI();
                }
            }
            else
            {
                GUILayout.Space(20);
                        
                var style = new GUIStyle(GUI.skin.button)
                {
                    padding = new RectOffset(0, 0, 15, 15)
                };
                if (GUILayout.Button("create CuvImporter", style))
                {
                    CreateCuvImporterEditor();
                }
            }

            GUILayout.Space(20);
            GUILayout.Box(string.Empty, new [] {GUILayout.ExpandWidth(true), GUILayout.Height(3)});
            GUILayout.Space(20);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_scriptProp, true);
            }
            GUILayout.Space(10);
            
            EditorGUILayout.HelpBox("This is a Uniprom source project.", MessageType.Info);
            
            GUILayout.Space(5);
            
            var prop = serializedObject.GetIterator();
            if (prop.NextVisible(true))
            {
                do
                {
                    if (!s_propertiesToExclude.Contains(prop.name)) 
                    {
                        EditorGUILayout.PropertyField(prop, true);
                    }
                }
                while (prop.NextVisible(false));
            }
#endif

#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
            GUILayout.BeginVertical();
            {
                var style = new GUIStyle(GUI.skin.label)
                {
                    padding = new RectOffset(0, 0, 15, 10),
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 15
                };
                GUILayout.Label("Build Asset Bundles", style);
            }
            GUILayout.EndVertical();
            
            EditorGUILayout.PropertyField(_referenceProp);
            EditorGUILayout.PropertyField(_settingsProp);
            
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!SampleSceneOpener.IsValid());
            if (GUILayout.Button("Sample Scene"))
            {
                SampleSceneOpener.Open();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(!PrefabOpener.IsValidInterstitial());
            if (GUILayout.Button("Interstitial prefab"))
            {
                PrefabOpener.OpenInterstitial();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(!PrefabOpener.IsValidWall());
            if (GUILayout.Button("Wall prefab"))
            {
                PrefabOpener.OpenWall();
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);

            if(_exporter.HasSettings())
            {
                var style = new GUIStyle("AppToolbar")
                {
                    padding = new RectOffset(0, 0, 8, 8),
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 13
                };
                GUILayout.Label("Current Build : " + _exporter.GetBuildType() + " (" + UnipromSettings.GetPlatform() + ")", style, GUILayout.ExpandWidth(true));
            }
            GUILayout.Space(5);
            
            EditorGUI.BeginDisabledGroup(_exporter.CuvImporter == default
                                         || !_exporter.CuvImporter.IsBuildCompleted
                                         || _exporter.IsSendingFiles);
            
            {
                var content = new GUIContent("Build - Test (" + UnipromSettings.GetPlatform() + ")", _buildIcon);
                if (GUILayout.Button(content, GUILayout.Height(38)))
                {
                    if (_exporter.CanIStartBuild())
                    {
                        _exporter.BuildTest();
                        var path = AddressableHelper.GetRemoteBuildPathByProfileName(UnipromExporter.TestProfileName);
                        Debug.Log("Addressables remote path:" + path);
                        ProcessHelper.OpenFolder(path);
                        SetProperties();
                    }
                }
            }
            {
                var content = new GUIContent("Build - Release (" + UnipromSettings.GetPlatform() + ")", _buildIcon);
                if (GUILayout.Button(content, GUILayout.Height(38)))
                {
                    if (_exporter.CanIStartBuild())
                    {
                        _exporter.BuildRelease();
                        var path = AddressableHelper.GetRemoteBuildPathByProfileName(UnipromExporter.ReleaseProfileName);
                        Debug.Log("Addressables remote path:" + path);
                        ProcessHelper.OpenFolder(path);
                        SetProperties();
                    }
                }
            }
            EditorGUI.EndDisabledGroup();
            
            GUILayout.Space(5);
            
            EditorGUILayout.HelpBox("If you want to use it on another platform, switch platform from Build Settings. (iOS or Android only)", MessageType.Info);

            GUILayout.BeginVertical();
            {
                var style = new GUIStyle(GUI.skin.label)
                {
                    padding = new RectOffset(0, 0, 15, 10),
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 15
                };
                GUILayout.Label("Send To Server (FTP)", style);
            }
            GUILayout.EndVertical();

            if (GUILayout.Button("Generate configuration file templates", GUILayout.Height(23)))
            {
                GenerateFtpSettingsWindow.ShowWindow();
            }

            GUILayout.Space(5);
            EditorGUILayout.HelpBox("Please ensure not to publicly share your FTP configuration files. Take caution not to include them in repositories such as Git.", MessageType.Info);
            GUILayout.Space(5);
            
            {
                var label = new GUIContent("FTP Settings path");
                EditorGUILayout.PropertyField(_testFtpPathProp, label);
            }
            GUILayout.Space(5);
            
            EditorGUI.BeginDisabledGroup(_exporter.CuvImporter == default
                                         || !_exporter.CuvImporter.IsBuildCompleted
                                         || _exporter.IsSendingFiles
                                         || _exporter.GetBuildType() != UnipromBuildType.Test);
            
            {
                var content = new GUIContent("Send to Test (" + UnipromSettings.GetPlatform() + ")", _ftpIcon);
                if (GUILayout.Button(content, GUILayout.Height(38))
                    && _exporter.CanISendTestServer())
                {
                    _exporter.SendToTestServer();
                }
            }
            EditorGUI.EndDisabledGroup();
            
            GUILayout.Space(10);
            {
                var label = new GUIContent("FTP Settings path");
                EditorGUILayout.PropertyField(_releaseFtpPathProp, label);
            }
            GUILayout.Space(5);
            
            EditorGUI.BeginDisabledGroup(_exporter.CuvImporter == default
                                         || !_exporter.CuvImporter.IsBuildCompleted
                                         || _exporter.IsSendingFiles
                                         || _exporter.GetBuildType() != UnipromBuildType.Release);
            
            {
                var content = new GUIContent("Send to Release (" + UnipromSettings.GetPlatform() + ")", _ftpIcon);
                if (GUILayout.Button(content, GUILayout.Height(38))
                    && _exporter.CanISendReleaseServer())
                {
                    _exporter.SendToReleaseServer();
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.HelpBox("If an error occurs, please wait some time and try again. If the error occurs repeatedly, please check your server settings.", MessageType.Info);

            GUILayout.BeginVertical();
            {
                var style = new GUIStyle(GUI.skin.label)
                {
                    padding = new RectOffset(0, 0, 15, 10),
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 15
                };
                GUILayout.Label("Export Package", style);
            }
            GUILayout.EndVertical();

            EditorGUI.BeginDisabledGroup(_exporter.CuvImporter == default
                                         || !_exporter.CuvImporter.IsBuildCompleted
                                         || _exporter.IsSendingFiles);
            {
                var content = new GUIContent("Export - Uniprom init settings", _unityIcon);
                if (GUILayout.Button(content, GUILayout.Height(38))
                    && _exporter.CanICreatePackage())
                {
                    if (!Directory.Exists(PackageSaveDir))
                    {
                        PackageSaveDir = Application.dataPath;
                    }
                    PackageSaveDir = EditorUtility.OpenFolderPanel(
                        "Path to save the package", 
                        PackageSaveDir,
                        string.Empty);
                    
                    if (!string.IsNullOrEmpty(PackageSaveDir))
                    {
                        _exporter.CreatePackage(PackageSaveDir);
                    }
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.HelpBox("Export the plugin for the project for which you want to use Uniprom. This operation is required when you build the Release version. After that, it is not necessary unless you change the URL or language.", MessageType.Info);
#endif

            serializedObject.ApplyModifiedProperties();
        }
        
        void ShowMenu()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Github"))
            {
                Application.OpenURL(_gitUrl);
            }

            if (!_isNowLoading
                && GUILayout.Button("Check for Update"))
            {
                _isNowLoading = true;
                CheckVersion.GetVersionOnServer(_packageUrl)
                    .ContinueOnMainThread(onSuccess : request =>
                        {
                            var version = request?.Result;
                            if (string.IsNullOrEmpty(version))
                            {
                                return;
                            }
                            var comparisonResult = 0;
                            if (!string.IsNullOrEmpty(version))
                            {
                                var current = new Version(_currentVersion.TrimStart('v').Trim());
                                var server = new Version(version.Trim());
                                comparisonResult = current.CompareTo(server);
                                version = "v" + version;
                            }
                                
                            if (comparisonResult >= 0)
                            {
                                EditorUtility.DisplayDialog("You have the latest version.", "Editor: " + _currentVersion + " | GitHub: " + version + "\nThe current version is the latest release.", "Close");
                            }
                            else
                            {
                                var isOpen = EditorUtility.DisplayDialog(_currentVersion + " -> " + version, "There is a newer version " + version + ".", "Update", "Close");
    
                                if (isOpen)
                                {
                                    _isNowLoading = true;
                                    _tokenSource = new CancellationTokenSource();
                                    _packageInstaller.Install(
                                            new []{ _gitInstallUrl },
                                            _tokenSource.Token
                                        )
                                        .Handled(() => _isNowLoading = false);
                                }
                            }
                        },
                    onCompleted : () => _isNowLoading = false);
            }
            else if (_isNowLoading)
            {
                GUILayout.Button("Now Loading...");
            }
            GUILayout.EndHorizontal();
        }

#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
        void CreateCuvImporterEditor()
        {
            _exporter.CreateImporterIfNeeded();
            _exporter.ResolveReference();
            EditorApplication.delayCall += () =>
            {
                _cuvImporterEditor = CreateEditor(_exporter.CuvImporter);
            };
        }
#endif
        
        static Texture2D GetTexture(string textureName)
            => AssetDatabase.LoadAssetAtPath<Texture2D>(_packagePath + "Editor/Textures/" + textureName + ".png");
        
        bool IsVersionUpdated(string current, string request)
        {
            var currentVersion = new Version(current);
            var requestVersion = new Version(request);
            return currentVersion < requestVersion;
        }
    }
}
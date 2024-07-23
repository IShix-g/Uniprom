
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using CMSuniVortex;
using Uniprom.GoogleSheet;
using UnityEditor;
using UnityEngine;

namespace Uniprom.Editor
{
    public static class AssetBuilder
    {
        public const string FtpJsonStringName = "ftpJsonFilePath";
        public const string GoogleJsonKeyPath = "googleJsonKeyPath";

#if UNIPROM_SOURCE_PROJECT
        [MenuItem("Window/Uniprom/Build test of Github Action")]
        static void StartGithubBuildTest()
        {
            var exporter = UnipromSettingsExporter.GetInstance();
            var options = new Dictionary<string, string> { {FtpJsonStringName, exporter.TestFtpSettingPath} };
            if (exporter.CuvImporter.Client is UnipromModelsCustomGoogleSheetCuvAddressableClient client)
            {
                options.Add(GoogleJsonKeyPath, client.JsonKeyPath);
            }
            Build(false, options);
        }
#endif

        public static void BuildRelease()
        {
            var options = ArgumentsParser.GetValidatedOptions();
            Build(true, options);
        }

        public static void BuildTest()
        {
            var options = ArgumentsParser.GetValidatedOptions();
            Build(false, options);
        }

        public static void Build(bool isRelease, Dictionary<string, string> options)
        {
            var exporter = UnipromSettingsExporter.GetInstance();
            if (exporter.CuvImporter == default)
            {
                UnipromDebug.LogError("CuvImporter does not exist. Please complete the setup.");
                return;
            }

            if (options.TryGetValue(GoogleJsonKeyPath, out var googleJsonKeyPath)
                && !string.IsNullOrEmpty(googleJsonKeyPath)
                && exporter.CuvImporter.Client is UnipromModelsCustomGoogleSheetCuvAddressableClient client)
            {
                var directoryName = Path.GetDirectoryName(Application.dataPath);
                var path = Path.Combine(directoryName, googleJsonKeyPath);
                UnipromDebug.Log("Set Google json key path: " + path);
                client.JsonKeyPath = path;
            }
            else
            {
                UnipromDebug.Log("Google json key path is not set.");
            }
            
            var importer = (ICuvImporter) exporter.CuvImporter;
            
            exporter.ResolveReference();
            
            importer.StartImport(() =>
            {
                UnipromDebug.Log("Completion of import");
                
                try
                {
                    if (isRelease)
                    {
                        exporter.BuildRelease(true);
                    }
                    else
                    {
                        exporter.BuildTest(true);
                    }
                    
                    if (!options.TryGetValue(FtpJsonStringName, out var jsonStringPath))
                    {
                        UnipromDebug.LogWarning("Could not obtain Json string.");
                        return;
                    }
                    
                    var jsonString = default(string);
                    if (!string.IsNullOrEmpty(jsonStringPath))
                    {
                        var directoryName = Path.GetDirectoryName(Application.dataPath);
                        var path = Path.Combine(directoryName, jsonStringPath);
                        jsonString = File.ReadAllText(path);
                        UnipromDebug.Log("Reading Json string path: " + path);
                    }
                    else
                    {
                        UnipromDebug.LogWarning("Could not read Json string.");
                        return;
                    }
                    
                    if (isRelease)
                    {
                        exporter.SendToReleaseServer(jsonString)
                            .SafeContinueWith(task =>
                            {
                                if (task.Status == TaskStatus.RanToCompletion)
                                {
                                    UnipromDebug.Log("Completion of release build");
                                    if (UnipromDebug.IsBatchMode)
                                    {
                                        EditorApplication.Exit(0);
                                    }
                                }
                                else if (task.IsFaulted)
                                {
                                    UnipromDebug.LogError(task.Exception != default ? task.Exception.ToString() : "Unknown error");
                                }
                            });
                    }
                    else
                    {
                        exporter.SendToTestServer(jsonString)
                            .SafeContinueWith(task =>
                            {
                                if (task.Status == TaskStatus.RanToCompletion)
                                {
                                    UnipromDebug.Log("Completion of test build");
                                    if (UnipromDebug.IsBatchMode)
                                    {
                                        EditorApplication.Exit(0);
                                    }
                                }
                                else if (task.IsFaulted)
                                {
                                    UnipromDebug.LogError(task.Exception != default ? task.Exception.ToString() : "Unknown error");
                                }
                            });
                    }
                }
                catch (Exception e)
                {
                    UnipromDebug.LogError(e.ToString());
                    throw;
                }
            });
        }
    }
}
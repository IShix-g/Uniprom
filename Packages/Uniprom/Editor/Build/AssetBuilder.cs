
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CMSuniVortex;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Uniprom.Editor
{
    public static class AssetBuilder
    {
        const string _ftpJsonStringName = "ftpJsonFilePath";

#if UNIPROM_SOURCE_PROJECT
        [MenuItem("Window/Uniprom/Build test of Github Action")]
        static void StartGithubBuildTest()
        {
            var exporter = UnipromSettingsExporter.GetInstance();
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(exporter.TestFtpSettingPath);
            Build(false, asset.text)
                .SafeContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    var error = task.Exception != default
                        ? task.Exception.ToString()
                        : "Unknown error";
                    UnipromDebug.LogError(error);
                }
                else
                {
                    UnipromDebug.Log("Completion of release build");
                }
            });
        }
#endif

        public static void BuildRelease()
        {
            var options = ArgumentsParser.GetValidatedOptions();
            Build(true, default, options)
                .SafeContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        var error = task.Exception != default
                            ? task.Exception.ToString()
                            : "Unknown error";
                        UnipromDebug.LogError(error);
                    }
                    else
                    {
                        UnipromDebug.Log("Completion of release build");
                        EditorApplication.Exit(0);
                    }
                });
        }

        public static void BuildTest()
        {
            var options = ArgumentsParser.GetValidatedOptions();
            Build(false, default, options)
                .SafeContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        var error = task.Exception != default
                            ? task.Exception.ToString()
                            : "Unknown error";
                        UnipromDebug.LogError(error);
                    }
                    else
                    {
                        UnipromDebug.Log("Completion of test build");
                        EditorApplication.Exit(0);
                    }
                });
        }

        public static Task Build(bool isRelease, string jsonString = default, Dictionary<string, string> options = default)
        {
            var exporter = UnipromSettingsExporter.GetInstance();
            if (exporter.CuvImporter == default)
            {
                UnipromDebug.LogError("CuvImporter does not exist. Please complete the setup.");
                return Task.CompletedTask;
            }
            
            var tcs = new TaskCompletionSource<bool>();
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
                    
                    var jsonStringPath = default(string);
                    if (string.IsNullOrEmpty(jsonString)
                        && (options == default
                            || !options.TryGetValue(_ftpJsonStringName, out jsonStringPath)))
                    {
                        UnipromDebug.LogWarning("Could not obtain Json string.");
                        tcs.SetResult(true);
                        return;
                    }
                    
                    if (string.IsNullOrEmpty(jsonString))
                    {
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
                            tcs.SetResult(true);
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    UnipromDebug.LogError(e.ToString());
                    tcs.SetException(e);
                    throw;
                }
                
                if (isRelease)
                {
                    exporter.SendToReleaseServer(jsonString)
                        .SafeContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            tcs.SetException(task.Exception);
                        }
                        else
                        {
                            tcs.SetResult(true);
                        }
                    });
                }
                else
                {
                    exporter.SendToTestServer(jsonString)
                        .SafeContinueWith(task =>
                        {
                            if (task.IsFaulted)
                            {
                                tcs.SetException(task.Exception);
                            }
                            else
                            {
                                tcs.SetResult(true);
                            }
                        });
                }
            });
            
            return tcs.Task;
        }
    }
}
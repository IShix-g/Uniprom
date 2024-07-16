
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CMSuniVortex;
using UnityEditor;
using UnityEngine;

namespace Uniprom.Editor
{
    public static class AssetBuilder
    {
        const string _ftpJsonStringName = "ftpJsonString";

#if UNIPROM_SOURCE_PROJECT
        [MenuItem("Window/Uniprom/Build test of Github Action")]
        static async void StartGithubBuildTest()
        {
            var exporter = UnipromSettingsExporter.GetInstance();
            var obj = AssetDatabase.LoadAssetAtPath<TextAsset>(exporter.TestFtpSettingPath);
            await Build(false, obj.text);
            UnipromDebug.Log("Completion of release build");
        }
#endif

        public static async void BuildRelease()
        {
            try
            {
                await Build(true, default, ArgumentsParser.GetValidatedOptions(new []{ _ftpJsonStringName }));
                UnipromDebug.Log("Completion of release build");
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                UnipromDebug.LogError(e.ToString());
                throw;
            }
        }

        public static async void BuildTest()
        {
            try
            {
                await Build(false, default, ArgumentsParser.GetValidatedOptions(new []{ _ftpJsonStringName }));
                UnipromDebug.Log("Completion of test build");
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                UnipromDebug.LogError(e.ToString());
                throw;
            }
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
            
            importer.StartImport(() =>
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
                        jsonString = File.ReadAllText(jsonStringPath);
                    }
                    else
                    {
                        UnipromDebug.LogWarning("Could not read Json string.");
                        tcs.SetResult(true);
                        return;
                    }
                }
                
                try
                {
                    if ((isRelease && !exporter.CanISendReleaseServer())
                        || (!isRelease && !exporter.CanISendTestServer()))
                    {
                        tcs.SetResult(true);
                        return;
                    }
                }
                catch (Exception e)
                {
                    tcs.SetResult(true);
                    UnipromDebug.LogError(e.ToString());
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
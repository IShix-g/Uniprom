
using System.Collections.Generic;
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
            UnipromDebug.Log("Completed build.");
        }
#endif

        public static async void BuildRelease()
        {
            await Build(true, default, ArgumentsParser.GetValidatedOptions(new []{ _ftpJsonStringName }));
            UnipromDebug.Log("Completed release build.");
        }

        public static async void BuildTest()
        {
            await Build(false, default, ArgumentsParser.GetValidatedOptions(new []{ _ftpJsonStringName }));
            UnipromDebug.Log("Completed test build.");
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
                
                if (string.IsNullOrEmpty(jsonString)
                    && (options == default
                        || !options.TryGetValue(_ftpJsonStringName, out jsonString)))
                {
                    UnipromDebug.LogWarning("Could not obtain Json String.");
                    tcs.SetResult(true);
                    return;
                }
                
                if ((isRelease && !exporter.CanISendReleaseServer())
                    || (!isRelease && !exporter.CanISendTestServer()))
                {
                    tcs.SetResult(true);
                    return;
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
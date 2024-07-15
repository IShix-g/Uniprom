
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
        static void StartGithubBuildTest()
        {
            var exporter = UnipromSettingsExporter.GetInstance();
            var obj = AssetDatabase.LoadAssetAtPath<TextAsset>(exporter.TestFtpSettingPath);
            Build(false, obj.text);
        }
#endif

        public static void BuildRelease() => Build(true, default, ArgumentsParser.GetValidatedOptions(new []{ _ftpJsonStringName }));

        public static void BuildTest() => Build(false, default, ArgumentsParser.GetValidatedOptions(new []{ _ftpJsonStringName }));

        public static void Build(bool isRelease, string jsonString = default, Dictionary<string, string> options = default)
        {
            var exporter = UnipromSettingsExporter.GetInstance();
            if (exporter.CuvImporter == default)
            {
                UnipromDebug.LogError("CuvImporter does not exist. Please complete the setup.");
                return;
            }
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
                    return;
                }
                
                if ((isRelease && !exporter.CanISendReleaseServer())
                    || (!isRelease && !exporter.CanISendTestServer()))
                {
                    return;
                }
                
                if (isRelease)
                {
                    exporter.SendToReleaseServer(jsonString).SafeContinueWith(SendCompleted);
                }
                else
                {
                    exporter.SendToTestServer(jsonString).SafeContinueWith(SendCompleted);
                }
            });
        }

        static void SendCompleted(Task task)
        {
            if (task.Status != TaskStatus.RanToCompletion
                && task.Exception != default)
            {
                UnipromDebug.LogError("SendToServer error: " + task.Exception);
            }
        }
    }
}
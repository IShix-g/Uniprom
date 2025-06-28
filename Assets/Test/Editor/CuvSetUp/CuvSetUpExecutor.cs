#if ENABLE_CMSUNIVORTEX
using Uniprom;
using Uniprom.GoogleSheet;
using UnityEditor;
using UnityEngine;

namespace Test
{
    public static class CuvSetUpExecutor
    {
        [MenuItem("CuvSetUp/GoogleSheet")]
        static void SetGoogleSheet()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/ClientSettings/client-settings-google-sheet.json");
            var model = JsonUtility.FromJson<GoogleSheetCuvClientSettings>(asset.text);
            var exporter = UnipromSettingsExporter.GetInstance();
            var client = new UnipromModelsCustomGoogleSheetCuvAddressableClient
            {
                SheetUrl = model.SheetUrl,
                JsonKeyPath = model.JsonKeyPath
            };
            client.SetSettings(UnipromSettingsExporter.DefaultCuvSettings);
            exporter.CuvImporter.Client = client;
            exporter.CuvImporter.Output = new UnipromModelsCustomGoogleSheetCuvAddressableOutput();
        }
        
        [MenuItem("CuvSetUp/Cockpit")]
        static void SetCockpit()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/ClientSettings/client-settings-cockpit.json");
            var model = JsonUtility.FromJson<CockpitCuvClientSettings>(asset.text);
            var exporter = UnipromSettingsExporter.GetInstance();
            var client = new UnipromModelsCockpitCuvAddressableClient
            {
                BaseUrl = model.BaseUrl,
                ApiKey = model.ApiKey,
                ModelName = model.ModelName
            };
            client.SetSettings(UnipromSettingsExporter.DefaultCuvSettings);
            exporter.CuvImporter.Client = client;
            exporter.CuvImporter.Output = new UnipromModelsCockpitCuvAddressableOutput();
        }
    }
}
#endif
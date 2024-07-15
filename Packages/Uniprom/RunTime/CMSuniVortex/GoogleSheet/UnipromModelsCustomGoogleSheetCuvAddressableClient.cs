#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using CMSuniVortex;
using CMSuniVortex.GoogleSheet;

namespace Uniprom.GoogleSheet
{
    [CuvDisplayName("Uniprom - Google Sheet Client")]
    public sealed class UnipromModelsCustomGoogleSheetCuvAddressableClient : CustomGoogleSheetCuvAddressableClient<UnipromGoogleSheetModel, UnipromModelsCustomGoogleSheetCuvModelList>
    {
        protected override void OnSelect(string assetPath)
        {
#if UNITY_EDITOR
            base.OnSelect(assetPath);
            SetSettings(UnipromSettingsExporter.DefaultCuvSettings);
#endif
        }
    }
}
#endif
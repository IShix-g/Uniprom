#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System.ComponentModel;
using CMSuniVortex;
using CMSuniVortex.GoogleSheet;

namespace Uniprom.GoogleSheet
{
    [DisplayName("Uniprom - Google Sheet Client")]
    public sealed class UnipromModelsCustomGoogleSheetCuvAddressableClient : CustomGoogleSheetCuvAddressableLocalizedClient<UnipromGoogleSheetModel, UnipromModelsCustomGoogleSheetCuvModelList>
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
#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System.ComponentModel;
using CMSuniVortex;
using CMSuniVortex.GoogleSheet;

namespace Uniprom.GoogleSheet
{
    [DisplayName("Uniprom - Google Sheet Output")]
    public sealed class UnipromModelsCustomGoogleSheetCuvAddressableOutput : CustomGoogleSheetCuvAddressableOutput<
        UnipromGoogleSheetModel,
        UnipromModelsCustomGoogleSheetCuvModelList,
        UnipromModelsCustomGoogleSheetCuvAddressableReference>
    {
        public override void Select(string buildPath)
        {
#if UNITY_EDITOR
            base.Select(buildPath);
            SetSettings(UnipromSettingsExporter.DefaultCuvSettings);
#endif
        }
    }
}
#endif
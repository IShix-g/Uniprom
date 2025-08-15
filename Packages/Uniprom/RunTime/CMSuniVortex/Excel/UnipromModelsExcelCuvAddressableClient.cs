#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System.ComponentModel;
using CMSuniVortex.Excel;

namespace Uniprom.Excel
{
    [DisplayName("Uniprom - Excel Client")]
    public sealed class UnipromModelsExcelCuvAddressableClient
        : ExcelCuvAddressableLocalizedClient<
            UnipromExcelModel,
            UnipromModelsExcelCuvModelList>
    {
        protected override void OnSelect(string assetPath)
        {
#if UNITY_EDITOR
            base.OnSelect(assetPath);
            SetSettings(UnipromExporter.DefaultCuvSettings);
#endif
        }
    }
}
#endif
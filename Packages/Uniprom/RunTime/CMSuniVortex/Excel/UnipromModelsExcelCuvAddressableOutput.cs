#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System.ComponentModel;
using CMSuniVortex.Excel;

namespace Uniprom.Excel
{
    [DisplayName("Uniprom - Excel Output")]
    public sealed class UnipromModelsExcelCuvAddressableOutput
        : ExcelCuvAddressableOutput<
            UnipromExcelModel,
            UnipromModelsExcelCuvModelList,
            UnipromModelsExcelCuvAddressableReference>
    {
        public override void Select(string buildPath)
        {
#if UNITY_EDITOR
            base.Select(buildPath);
            SetSettings(UnipromExporter.DefaultCuvSettings);
#endif
        }
    }
}
#endif
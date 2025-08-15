#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using CMSuniVortex.Excel;

namespace Uniprom.Excel
{
    public sealed class UnipromModelsExcelCuvAddressableReference
        : ExcelCuvAddressableReference<
            UnipromExcelModel,
            UnipromModelsExcelCuvModelList>,
          IUnipromReference
    {
        public override bool EnableAutoLocalization => false;

        public IUnipromModel GetModelByKey(string key) => GetByKey(key);

        public bool TryGetModelByKey(string key, out IUnipromModel model)
        {
            var isSuccess = TryGetByKey(key, out var result);
            model = result;
            return isSuccess;
        }

        public IUnipromModel GetModelByIndex(int index) => ActiveLocalizedList[index];
    }
}
#endif
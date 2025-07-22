#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using CMSuniVortex.GoogleSheet;

namespace Uniprom.GoogleSheet
{
    public sealed class UnipromModelsCustomGoogleSheetCuvAddressableReference : CustomGoogleSheetCuvAddressableReference<UnipromGoogleSheetModel, UnipromModelsCustomGoogleSheetCuvModelList>, IUnipromReference
    {
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
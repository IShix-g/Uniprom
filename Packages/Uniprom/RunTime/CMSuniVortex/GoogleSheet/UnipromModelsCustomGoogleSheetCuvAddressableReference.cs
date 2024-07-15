#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using CMSuniVortex.GoogleSheet;

namespace Uniprom.GoogleSheet
{
    public sealed class UnipromModelsCustomGoogleSheetCuvAddressableReference : CustomGoogleSheetCuvAddressableReference<UnipromGoogleSheetModel, UnipromModelsCustomGoogleSheetCuvModelList>, IUnipromReference
    {
        public IUnipromModel GetModelById(string id) => GetById(id);

        public bool TryGetModelById(string id, out IUnipromModel model)
        {
            var isSuccess = TryGetById(id, out var result);
            model = result;
            return isSuccess;
        }

        public IUnipromModel GetModelByIndex(int index) => GetByIndex(index);
    }
}
#endif
#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using CMSuniVortex.Cockpit;

namespace Uniprom
{
    public sealed class UnipromModelsCockpitCuvAddressableReference 
        : CockpitCuvAddressableReference<UnipromCockpitModel, UnipromModelsCockpitCuvModelList>, IUnipromReference
    {
        public override bool EnableAutoLocalization => false;
        
        public IUnipromModel GetModelByKey(string key) => GetByKey(key);

        public bool TryGetModelByKey(string id, out IUnipromModel model)
        {
            var isSuccess = TryGetByKey(id, out var result);
            model = result;
            return isSuccess;
        }

        public IUnipromModel GetModelByIndex(int index) => ActiveLocalizedList[index];
    }
}
#endif
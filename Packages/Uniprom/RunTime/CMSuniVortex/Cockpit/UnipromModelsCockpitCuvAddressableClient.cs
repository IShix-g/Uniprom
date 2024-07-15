#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using CMSuniVortex;
using CMSuniVortex.Cockpit;
using Newtonsoft.Json;

namespace Uniprom
{
    [CuvDisplayName("Uniprom - Cockpit Client")]
    public sealed class UnipromModelsCockpitCuvAddressableClient : CockpitCuvAddressableClient<UnipromCockpitModel, UnipromModelsCockpitCuvModelList>
    {
        protected override JsonConverter<UnipromCockpitModel> CreateConverter()
            => new CuvModelConverter<UnipromCockpitModel>();
        
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
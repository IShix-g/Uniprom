#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using CMSuniVortex;
using CMSuniVortex.Cockpit;

namespace Uniprom
{
    [CuvDisplayName("Uniprom - Cockpit Output")]
    public sealed class UnipromModelsCockpitCuvAddressableOutput : CockpitCuvAddressableOutput<
        UnipromCockpitModel,
        UnipromModelsCockpitCuvModelList,
        UnipromModelsCockpitCuvAddressableReference>
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
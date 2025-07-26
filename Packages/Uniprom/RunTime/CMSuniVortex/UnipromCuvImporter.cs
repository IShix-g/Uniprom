#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System.IO;
using CMSuniVortex;
using Uniprom.GoogleSheet;

#if UNITY_EDITOR
using System;
using UnityEditor;
#endif

namespace Uniprom.CMSuniVortex
{
    [CuvImporter(
        isShowMenu : false, 
        isShowLogo : false, 
        isEnabledBuildPath : false, 
        isEnabledLanguages : true, 
        isEnabledSelectClient : true, 
        isEnabledImportButton : true, 
        isEnabledSelectOutput : true, 
        isEnabledOutputButton : true
    )]
    public sealed class UnipromCuvImporter : CuvImporter
    {
        UnipromExporter _exporter;

        public void SetSettings(UnipromExporter exporter) => _exporter = exporter;
        
        protected override void OnOutputted(ICuvOutput output, string[] listGuids)
        {
#if UNITY_EDITOR
            base.OnOutputted(output, listGuids);

            switch (output)
            {
                case UnipromModelsCockpitCuvAddressableOutput cockpit:
                    _exporter.SetReference(cockpit.GetReference());
                    break;
                case UnipromModelsCustomGoogleSheetCuvAddressableOutput googleSheet:
                    _exporter.SetReference(googleSheet.GetReference());
                    break;
            }
#endif
        }
        
        protected override void Reset()
        {
#if UNITY_EDITOR
            base.Reset();
            try
            {
                Client = new UnipromModelsCockpitCuvAddressableClient();
                Output = new UnipromModelsCockpitCuvAddressableOutput();
                var dir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(this));
                SetBuildPath(dir);
            }
            catch (ArgumentException)
            {
                // Ignore
            }
#endif
        }
    }
}
#endif
#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System.IO;
using CMSuniVortex;
using Uniprom.GoogleSheet;
using UnityEngine;

#if UNITY_EDITOR
using System;
using UnityEditor;
#endif

namespace Uniprom.CMSuniVortex
{
    public sealed class UnipromCuvImporter : CuvImporter
    {
        UnipromSettingsExporter _settingsExporter;

        public void SetSettings(UnipromSettingsExporter settingsExporter) => _settingsExporter = settingsExporter;
        
        protected override void OnOutputted(ICuvOutput output, string[] listGuids)
        {
#if UNITY_EDITOR
            base.OnOutputted(output, listGuids);

            switch (output)
            {
                case UnipromModelsCockpitCuvAddressableOutput cockpit:
                    _settingsExporter.SetReference(cockpit.GetReference());
                    break;
                case UnipromModelsCustomGoogleSheetCuvAddressableOutput googleSheet:
                    _settingsExporter.SetReference(googleSheet.GetReference());
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
                SetClient(new UnipromModelsCockpitCuvAddressableClient());
                SetOutput(new UnipromModelsCockpitCuvAddressableOutput());
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
#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System;
using CMSuniVortex.Excel;
using UnityEngine.AddressableAssets;

namespace Uniprom.Excel
{
    [Serializable]
    public sealed class UnipromExcelModel : ExcelModel, IUnipromModel
    {
        public string AppName;
        public string AppDescription;
        public string IOSUrl;
        public string AndroidUrl;
        public AssetReferenceSprite Interstitial;
        public AssetReferenceSprite Icon;
        public string OptionText;

        protected override void OnDeserialize()
        {
            AppName = GetString("AppName");
            AppDescription = GetString("AppDescription");
            IOSUrl = GetString("IOSUrl");
            AndroidUrl = GetString("AndroidUrl");
            LoadSpriteReference("Interstitial", asset => Interstitial = asset);
            LoadSpriteReference("Icon", asset => Icon = asset);
            OptionText = GetString("OptionText");
        }

        public string GetAppName() => AppName;

        public string GetAppDescription() => AppDescription;

        public string GetIOSUrl() => IOSUrl;

        public string GetAndroidUrl() => AndroidUrl;

        public AssetReferenceSprite GetInterstitial() => Interstitial;

        public AssetReferenceSprite GetIcon() => Icon;
        
        public string GetOptionText() => OptionText;
    }
}
#endif
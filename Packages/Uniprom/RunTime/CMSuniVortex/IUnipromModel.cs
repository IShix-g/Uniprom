#if ENABLE_ADDRESSABLES
using UnityEngine.AddressableAssets;

namespace Uniprom
{
    public interface IUnipromModel
    {
        public string GetKey();
        public string GetAppName();
        public string GetAppDescription();
        public string GetIOSUrl();
        public string GetAndroidUrl();
        public AssetReferenceSprite GetInterstitial();
        public AssetReferenceSprite GetIcon();
        public string GetOptionText();
    }
}
#endif
#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Uniprom
{
    public sealed class DefaultUnipromInterstitialViewClient : UnipromInterstitialViewClient<UnipromInterstitialModel>
    {
        protected override void OnInitialize(){}
        
        public override UnipromInterstitialModel CreateModel()
        {
            var model = Manager.GetModel(Manager.ModelOrders.Interstitial);
            return new UnipromInterstitialModel(model);
        }
        
        public override async Task<IUnipromInterstitialView> CreatePrefabAsync(RectTransform parent)
        {
            var key = UnipromSettingsExporter.InterstitialAddressableKey;
            var handle = Addressables.InstantiateAsync(key, parent);
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result.GetComponent<IUnipromInterstitialView>();
            }
            throw handle.OperationException;
        }
        
        protected override void OnDispose() {}
    }
}
#endif
#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Uniprom
{
    public abstract class UnipromInterstitialViewClient<T>
        : UnipromViewClient<IUnipromInterstitialView>, IUnipromViewCreator<IUnipromInterstitialView>
        where T : IUnipromInterstitialModel, IUnipromModelInitializable
    {
        public abstract T CreateModel();
        public abstract Task<IUnipromInterstitialView> CreatePrefabAsync(RectTransform parent);
        
        public override async Task<IUnipromInterstitialView> LoadAsync(
            CancellationToken token = default,
            Action<UnipromViewShowStatus> showAction = default,
            Action<UnipromViewHideStatus> hideAction = default,
            Action<UnipromViewErrorStatus> errorAction = default)
        {
            try
            {
                if (!Manager.IsInitialized)
                {
                    await Manager.WaitUntilInitializeAsync(token);
                }
            }
            catch (Exception e)
            {
                if (!token.IsCancellationRequested)
                {
                    var status = new UnipromViewErrorStatus(UnipromViewType.Interstitial, UnipromViewErrorType.UnknownError, e);
                    errorAction?.Invoke(status);
#if DEBUG
                    Debug.LogWarning(e);
#endif
                }
                return default;
            }
            
            var model = CreateModel();
            try
            {
                await model.LoadAsync();
            }
            catch (Exception e)
            {
                var status = new UnipromViewErrorStatus(UnipromViewType.Interstitial, UnipromViewErrorType.ModelLoadingFailed, e);
                errorAction?.Invoke(status);
#if DEBUG
                Debug.LogWarning(e);
#endif
                return default;
            }
            
            try
            {
                var result = await CreatePrefabAsync(Manager.GetPopupParent());
                result.Initialize(Manager.GetCompanyName(), this, model, showAction, hideAction);
                return result;
            }
            catch (Exception e)
            {
                var status = new UnipromViewErrorStatus(UnipromViewType.Interstitial, UnipromViewErrorType.ViewLoadingFailed, e);
                errorAction?.Invoke(status);
#if DEBUG
                Debug.LogWarning(e);
#endif
                return default;
            }
        }

        public void Release(IUnipromInterstitialView obj)
        {
            if (obj.RootObject != default)
            {
                Addressables.ReleaseInstance(obj.RootObject);
            }
        }
    }
}
#endif
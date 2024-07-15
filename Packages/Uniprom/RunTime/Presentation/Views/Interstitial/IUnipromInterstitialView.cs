#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System;

namespace Uniprom
{
    public interface IUnipromInterstitialView : IUnipromView
    {
        string AppKey { get; }
        
        void Initialize(
            string companyName,
            IUnipromViewCreator<IUnipromInterstitialView> creator,
            IUnipromInterstitialModel interstitialModel,
            Action<UnipromViewShowStatus> showAction,
            Action<UnipromViewHideStatus> hideAction);
    }
}
#endif
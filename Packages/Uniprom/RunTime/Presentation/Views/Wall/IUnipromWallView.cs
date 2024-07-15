#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System;
using System.Collections.Generic;

namespace Uniprom
{
    public interface IUnipromWallView : IUnipromView
    {
        string[] AppKeys { get; }
        
        void Initialize(
            string companyName,
            IUnipromViewCreator<IUnipromWallView> creator,
            List<IUnipromIconModel> iconModels,
            Action<UnipromViewShowStatus> showAction,
            Action<UnipromViewHideStatus> hideAction);
    }
}
#endif
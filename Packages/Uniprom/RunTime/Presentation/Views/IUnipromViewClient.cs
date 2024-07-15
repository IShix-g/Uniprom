#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Uniprom
{
    public interface IUnipromViewClient<T> where T : IUnipromView
    {
        void Initialize(UnipromManager manager);
        Task<T> LoadAsync(CancellationToken token, Action<UnipromViewShowStatus> showAction, Action<UnipromViewHideStatus> hideAction, Action<UnipromViewErrorStatus> errorAction);
    }
}
#endif
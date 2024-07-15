#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Uniprom
{
    public abstract class UnipromViewClient<T> : IUnipromViewClient<T>, IDisposable where T : IUnipromView
    {
        protected UnipromManager Manager { get; private set; }
        
        bool _isDisposed;

        protected abstract void OnInitialize();
        protected abstract void OnDispose();
        public abstract Task<T> LoadAsync(
            CancellationToken token = default,
            Action<UnipromViewShowStatus> showAction = default,
            Action<UnipromViewHideStatus> hideAction = default,
            Action<UnipromViewErrorStatus> errorAction = default);
        
        public void Initialize(UnipromManager manager)
        {
            Manager = manager;
            OnInitialize();
        }
        
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            Manager = default;
            OnDispose();
        }
    }
}
#endif
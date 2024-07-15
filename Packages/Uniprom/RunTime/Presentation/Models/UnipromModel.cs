
using System.Threading.Tasks;

namespace Uniprom
{
    public abstract class UnipromModel : IUnipromModelInitializable
    {
        public abstract bool IsLoaded { get; }

        public IUnipromModel Model { get; private set; }

        bool _isDisposed;
        
        public UnipromModel(IUnipromModel model) { Model = model; }

        protected abstract void OnDispose();
        public abstract Task OnLoadAsync();
        
        public void Initialize(IUnipromModel model) => Model = model;
        
        public Task LoadAsync()
            => !IsLoaded
               ? OnLoadAsync()
               : Task.CompletedTask;

        public string GetStoreUrl()
        {
#if UNITY_IOS
            return Model.GetIOSUrl();
#else
            return Model.GetAndroidUrl();
#endif
        }
        
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            Model = default;
            OnDispose();
        }
    }
}
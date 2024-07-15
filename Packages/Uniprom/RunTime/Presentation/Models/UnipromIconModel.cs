
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Uniprom
{
    public sealed class UnipromIconModel : UnipromModel, IUnipromIconModel
    {
        public override bool IsLoaded => Sprite != default;
        public Sprite Sprite { get; private set; }
        
        bool _isDisposed;
        AsyncOperationHandle<Sprite> _handel;

        public UnipromIconModel(IUnipromModel model) : base(model) {}

        public override async Task OnLoadAsync()
        {
            _handel = Addressables.LoadAssetAsync<Sprite>(Model.GetIcon());
            await _handel.Task;

            if (_isDisposed)
            {
                Addressables.Release(_handel);
                throw new InvalidOperationException("Is Disposed.");
            }
            
            if (_handel.Status == AsyncOperationStatus.Succeeded)
            {
                Sprite = _handel.Result;
            }
            else
            {
                throw _handel.OperationException;
            }
        }
        
        protected override void OnDispose()
        {
            if (_handel.IsValid())
            {
                Addressables.Release(_handel);
            }
        }
    }
}
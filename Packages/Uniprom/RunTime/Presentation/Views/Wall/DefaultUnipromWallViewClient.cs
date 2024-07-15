#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Uniprom
{
    public sealed class DefaultUnipromWallViewClient : UnipromWallViewClient<UnipromIconModel>
    {
        readonly List<IUnipromModel> _reusableList = new();
        
        protected override void OnInitialize(){}
        
        public override UnipromIconModel[] CreateModels()
        {
            Manager.GetModelList(Manager.ModelOrders.Wall, 5, _reusableList);
            var models = new UnipromIconModel[_reusableList.Count];
            for (var i = 0; i < _reusableList.Count; i++)
            {
                models[i] = new UnipromIconModel(_reusableList[i]);
            }
            _reusableList.Clear();
            return models;
        }
        
        public override async Task<IUnipromWallView> CreatePrefabAsync(RectTransform parent)
        {
            var key = UnipromSettingsExporter.WallAddressableKey;
            var handle = Addressables.InstantiateAsync(key, parent);
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result.GetComponent<IUnipromWallView>();
            }
            throw handle.OperationException;
        }

        protected override void OnDispose(){}
    }
}
#endif
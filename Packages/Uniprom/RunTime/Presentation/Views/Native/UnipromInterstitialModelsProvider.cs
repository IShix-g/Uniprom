#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Uniprom
{
    public abstract class UnipromInterstitialModelsProvider : UnipromModelsProvider<UnipromInterstitialModel>
    {
        readonly Queue<string> _excludeAppKeys = new ();
        
        protected override Task<List<UnipromInterstitialModel>> LoadModelsAsync(CancellationToken token, int length)
        {
            if (HasModel)
            {
                SetUpExcludeAppKeys(length);
            }
            return UnipromManager.Instance.LoadInterstitialModelsAsync(token, length, _excludeAppKeys);
        }

        void SetUpExcludeAppKeys(int length)
        {
            foreach (var model in Models)
            {
                var key = model.Model.GetKey();
                if (!_excludeAppKeys.Contains(key))
                {
                    _excludeAppKeys.Enqueue(key);
                }
            }

            var exclude = UnipromManager.Instance.ModelLength - length - _excludeAppKeys.Count;
            if (exclude >= 0)
            {
                return;
            }
            
            var index = 0;
            var excludeCount = Mathf.Abs(exclude);
            while (excludeCount > index
                   && _excludeAppKeys.Count > 0)
            {
                index++;
                _excludeAppKeys.Dequeue();
            }
        }
    }
}
#endif
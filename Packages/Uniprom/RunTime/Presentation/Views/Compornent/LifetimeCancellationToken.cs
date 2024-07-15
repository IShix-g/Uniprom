#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System.Threading;
using UnityEngine;

namespace Uniprom
{
    public sealed class LifetimeCancellationToken : MonoBehaviour
    {
        public CancellationToken Token => _source.Token;
        CancellationTokenSource _source = new ();
        
        void OnDestroy()
        {
            _source.Cancel();
            _source.Dispose();
            _source = default;
        }
    }
}
#endif
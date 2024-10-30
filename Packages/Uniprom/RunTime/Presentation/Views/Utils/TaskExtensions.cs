#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Uniprom
{
    public static class TaskExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CancellationToken GetDestroyCancellationToken(this MonoBehaviour @this)
            => @this.gameObject.GetDestroyCancellationToken();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CancellationToken GetDestroyCancellationToken(this GameObject @this)
        {
#if UNITY_2022_2_OR_NEWER
            return @this.destroyCancellationToken;
#else
            return @this.TryGetComponent<LifetimeCancellationToken>(out var script)
                ? script.Token
                : @this.AddComponent<LifetimeCancellationToken>().Token;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeContinueWith<TResult>(
            this Task<TResult> @this,
            Action<Task<TResult>> continuationAction,
            CancellationToken cancellationToken = default)
        {
            var context = SynchronizationContext.Current;
            @this.ConfigureAwait(false)
                .GetAwaiter()
                .OnCompleted(() =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (context != null
                        && SynchronizationContext.Current != context)
                    {
                        context.Post(state => continuationAction(@this), null);
                    }
                    else
                    {
                        continuationAction(@this);
                    }
                });
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeContinueWith(
            this Task @this,
            Action<Task> continuationAction,
            CancellationToken cancellationToken = default)
        {
            var context = SynchronizationContext.Current;
            @this.ConfigureAwait(false)
                .GetAwaiter()
                .OnCompleted(() =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
        
                    if (context != null
                        && SynchronizationContext.Current != context)
                    {
                        context.Post(state => continuationAction(@this), null);
                    }
                    else
                    {
                        continuationAction(@this);
                    }
                });
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeCancelAndDispose(this CancellationTokenSource @this)
        {
            if (@this == default)
            {
                return;
            }
            
            try
            {
                if (!@this.IsCancellationRequested)
                {
                    @this.Cancel();
                }
                @this.Dispose();
            }
            catch
            {
                // Ignore
            }
        }
    }
}
#endif
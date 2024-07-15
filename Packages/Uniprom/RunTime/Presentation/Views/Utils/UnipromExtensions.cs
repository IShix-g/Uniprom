#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.Exceptions;

namespace Uniprom
{
    public static class UnipromExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WaitUntilInitializeAsync(
            this UnipromManager @this,
            CancellationToken token,
            Action initializedAction)
        {
            @this.WaitUntilInitializeAsync(token)
                .SafeContinueWith(task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        initializedAction?.Invoke();
                    }
                }, token);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShowInterstitial(
            this UnipromManager @this,
            MonoBehaviour behaviour,
            Action showAction = default,
            Action hideAction = default,
            Action<UnipromViewErrorStatus> errorAction = default)
            => @this.ShowInterstitial(behaviour.GetDestroyCancellationToken(), showAction, hideAction, errorAction);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShowInterstitial(
            this UnipromManager @this,
            GameObject gameObject,
            Action showAction = default,
            Action hideAction = default,
            Action<UnipromViewErrorStatus> errorAction = default)
            => @this.ShowInterstitial(gameObject.GetDestroyCancellationToken(), showAction, hideAction, errorAction);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShowInterstitial(
            this UnipromManager @this,
            CancellationToken token = default,
            Action showAction = default,
            Action hideAction = default,
            Action<UnipromViewErrorStatus> errorAction = default)
        {
            @this.ShowInterstitialAsync(token, showAction, hideAction, errorAction)
                .SafeContinueWith(task =>
                {
                    if (task.IsFaulted
                        && !task.IsCanceled
                        && !token.IsCancellationRequested)
                    {
                        var status = new UnipromViewErrorStatus(UnipromViewType.Interstitial, UnipromViewErrorType.UnknownError, task.Exception);
                        @this.SetErrorStatus(status);
                    }
                }, token);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task ShowInterstitialAsync(
            this UnipromManager @this,
            CancellationToken token = default,
            Action showAction = default,
            Action hideAction = default,
            Action<UnipromViewErrorStatus> errorAction = default)
        {
            var view = await @this.LoadInterstitialAsync(token, showAction, hideAction, errorAction);
            if (view != default
                && !token.IsCancellationRequested)
            {
                view.Show();
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShowWall(
            this UnipromManager @this,
            MonoBehaviour behaviour,
            Action showAction = default,
            Action hideAction = default,
            Action<UnipromViewErrorStatus> errorAction = default)
            => @this.ShowWall(behaviour.GetDestroyCancellationToken(), showAction, hideAction, errorAction);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShowWall(
            this UnipromManager @this,
            GameObject gameObject,
            Action showAction = default,
            Action hideAction = default,
            Action<UnipromViewErrorStatus> errorAction = default)
            => @this.ShowWall(gameObject.GetDestroyCancellationToken(), showAction, hideAction, errorAction);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShowWall(
            this UnipromManager @this,
            CancellationToken token = default,
            Action showAction = default,
            Action hideAction = default,
            Action<UnipromViewErrorStatus> errorAction = default)
        {
            @this.ShowWallAsync(token, showAction, hideAction, errorAction)
                .SafeContinueWith(task =>
                {
                    if (task.IsFaulted
                        && !task.IsCanceled
                        && !token.IsCancellationRequested)
                    {
                        var status = new UnipromViewErrorStatus(UnipromViewType.Wall, UnipromViewErrorType.UnknownError, task.Exception);
                        @this.SetErrorStatus(status);
                    }
                }, token);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task ShowWallAsync(
            this UnipromManager @this,
            CancellationToken token = default,
            Action showAction = default,
            Action hideAction = default,
            Action<UnipromViewErrorStatus> errorAction = default)
        {
            var view = await @this.LoadWallAsync(token, showAction, hideAction, errorAction);
            if (view != default
                && !token.IsCancellationRequested)
            {
                view.Show();
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadInterstitialModels(
            this UnipromManager @this,
            MonoBehaviour behaviour,
            int length,
            Action<List<UnipromInterstitialModel>> completedAction,
            IReadOnlyCollection<string> excludeAppKeys = default)
            => @this.LoadInterstitialModels(behaviour.GetDestroyCancellationToken(), length, completedAction, excludeAppKeys);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadInterstitialModels(
            this UnipromManager @this,
            GameObject obj,
            int length,
            Action<List<UnipromInterstitialModel>> completedAction,
            IReadOnlyCollection<string> excludeAppKeys = default)
            => @this.LoadInterstitialModels(obj.GetDestroyCancellationToken(), length, completedAction, excludeAppKeys);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadInterstitialModels(
            this UnipromManager @this,
            CancellationToken token,
            int length,
            Action<List<UnipromInterstitialModel>> completedAction,
            IReadOnlyCollection<string> excludeAppKeys = default)
        {
            @this.LoadInterstitialModelsAsync(token, length, excludeAppKeys)
                .SafeContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        completedAction?.Invoke(task.Result);
                    }
                    else if (task.IsFaulted
                        && !task.IsCanceled
                        && !token.IsCancellationRequested)
                    {
                        var status = new UnipromViewErrorStatus(UnipromViewType.Native, UnipromViewErrorType.UnknownError, task.Exception);
                        @this.SetErrorStatus(status);
                    }
                }, token);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<List<T>> LoadModelsAsync<T>(
            Func<IUnipromModel, T> createViewModelFunc,
            IEnumerable<IUnipromModel> modelsList,
            List<Task<T>> taskReusableList,
            Action<UnipromViewErrorStatus> onErrorAction)
            where T : IUnipromModelInitializable
        {
            taskReusableList.Clear();
            var error = string.Empty;

            try
            {
                var context = SynchronizationContext.Current;
                foreach(var model in modelsList)
                {
                    var viewModel = createViewModelFunc(model);
                    var tcs = new TaskCompletionSource<T>();
                    
                    context.Post(_ =>
                    {
                        try
                        {
                            viewModel.LoadAsync()
                                .ContinueWith(t =>
                                {
                                    if (t.IsFaulted)
                                    {
                                        tcs.TrySetException(t.Exception);
                                    }
                                    else if (t.IsCanceled)
                                    {
                                        tcs.TrySetCanceled();
                                    }
                                    else
                                    {
                                        tcs.TrySetResult(viewModel);
                                    }

                                }, TaskScheduler.Default);
                        }
                        catch (Exception e)
                        {
                            error += e + "\n";
                            tcs.TrySetException(e);
                        }
                    }, default);

                    taskReusableList.Add(tcs.Task);
                }

                await Task.WhenAll(taskReusableList);
            }
            catch (Exception e)
            {
                var status = new UnipromViewErrorStatus(
                    UnipromViewType.Native,
                    UnipromViewErrorType.ModelLoadingFailed,
                    e);
                onErrorAction?.Invoke(status);
#if DEBUG
                Debug.LogWarning(e);
#endif
                return default;
            }

            var results = new List<T>();
            foreach (var task in taskReusableList)
            {
                if (task.Result != null)
                {
                    results.Add(task.Result);
                }
            }
            taskReusableList.Clear();
            
            if (!string.IsNullOrEmpty(error))
            {
                var status = new UnipromViewErrorStatus(
                    UnipromViewType.Native,
                    UnipromViewErrorType.ModelLoadingFailed,
                    new OperationException(error));
                onErrorAction?.Invoke(status);
#if DEBUG
                Debug.LogWarning(error);
#endif
            }
            return results;
        }
    }
}
#endif
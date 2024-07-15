#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.Exceptions;

namespace Uniprom
{
    public abstract class UnipromWallViewClient<T>
        : UnipromViewClient<IUnipromWallView>, IUnipromViewCreator<IUnipromWallView>
        where T : IUnipromIconModel, IUnipromModelInitializable
    {
        readonly List<Task<T>> _loadModelsTaskReusableList = new();
        
        public abstract T[] CreateModels();
        public abstract Task<IUnipromWallView> CreatePrefabAsync(RectTransform parent);
        
        public override async Task<IUnipromWallView> LoadAsync(
            CancellationToken token = default,
            Action<UnipromViewShowStatus> showAction = default,
            Action<UnipromViewHideStatus> hideAction = default,
            Action<UnipromViewErrorStatus> errorAction = default)
        {
            try
            {
                if (!Manager.IsInitialized)
                {
                    await Manager.WaitUntilInitializeAsync(token);
                }
            }
            catch (Exception e)
            {
                if (!token.IsCancellationRequested)
                {
                    var status = new UnipromViewErrorStatus(UnipromViewType.Wall, UnipromViewErrorType.InitializationFailed, e);
                    errorAction?.Invoke(status);
#if DEBUG
                    Debug.LogWarning(e);
#endif
                }
                return default;
            }
            
            var error = string.Empty;
            var models = CreateModels();
            
            _loadModelsTaskReusableList.Clear();
            
            try
            {
                var context = SynchronizationContext.Current;
                foreach(var model in models)
                {
                    var tcs = new TaskCompletionSource<T>();
                    
                    context.Post(_ =>
                    {
                        try
                        {
                            model.LoadAsync()
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
                                        tcs.TrySetResult(model);
                                    }

                                }, TaskScheduler.Default);
                        }
                        catch (Exception e)
                        {
                            error += e + "\n";
                            tcs.TrySetException(e);
                        }
                    }, default);

                    _loadModelsTaskReusableList.Add(tcs.Task);
                }

                await Task.WhenAll(_loadModelsTaskReusableList);
            }
            catch (Exception e)
            {
                var status = new UnipromViewErrorStatus(UnipromViewType.Wall, UnipromViewErrorType.ModelLoadingFailed, e);
                errorAction?.Invoke(status);
#if DEBUG
                Debug.LogWarning(e);
#endif
                return default;
            }
            
            try
            {
                var completedModels = new List<IUnipromIconModel>();
                foreach (var task in _loadModelsTaskReusableList)
                {
                    if (task.Result != null)
                    {
                        completedModels.Add(task.Result);
                    }
                }
                var result = await CreatePrefabAsync(Manager.GetPopupParent());
                result.Initialize(Manager.GetCompanyName(), this, completedModels, showAction, hideAction);
                
                if (!string.IsNullOrEmpty(error))
                {
                    var status = new UnipromViewErrorStatus(UnipromViewType.Wall, UnipromViewErrorType.ModelLoadingFailed, new OperationException(error));
                    errorAction?.Invoke(status);
#if DEBUG
                    Debug.LogWarning(error);
#endif
                }
                
                return result;
            }
            catch (Exception e)
            {
                var status = new UnipromViewErrorStatus(UnipromViewType.Wall, UnipromViewErrorType.ViewLoadingFailed, e);
                errorAction?.Invoke(status);
#if DEBUG
                Debug.LogWarning(e);
#endif
                return default;
            }
        }

        public void Release(IUnipromWallView obj)
        {
            if (obj.RootObject != default)
            {
                Addressables.ReleaseInstance(obj.RootObject);
            }
        }
    }
}
#endif
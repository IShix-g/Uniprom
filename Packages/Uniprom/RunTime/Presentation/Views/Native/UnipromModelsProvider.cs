#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Uniprom
{
    public abstract class UnipromModelsProvider<T> : MonoBehaviour, IUnipromModelsProvider where T : IUnipromViewModel, IDisposable
    {
        public bool CanShow => UnipromManager.Instance.CanShow();
        public bool CanReload => CanShow && HasModel;
        public bool HasModel => Models is {Count: > 0};
        public int ModelLength => UnipromManager.Instance.ModelLength;
        public List<T> Models { get; private set; }
        
        CancellationTokenSource _cts = new ();
        float _interval;
        
        /// <summary>
        /// Number of ads, depends on internal and may be less than number
        /// </summary>
        protected abstract int GetLength();
        protected abstract bool AutoReload();
        protected abstract float ReloadInterval();
        protected abstract void OnLoadModels(List<T> models, bool isReload);
        protected abstract Task<List<T>> LoadModelsAsync(CancellationToken token, int length);
        
        protected virtual void Start()
        {
            if (UnipromManager.Instance.IsInitialized)
            {
                LoadModels();
            }
            else
            {
                UnipromManager.OnInitialized += LoadModels;
            }
            
            if (AutoReload()
                && ReloadInterval() <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ReloadInterval), "Reload Interval must be greater than zero. ReloadInterval(): " + ReloadInterval());
            }
        }

        protected virtual void Update()
        {
            if (_interval <= 0)
            {
                return;
            }

            _interval -= Time.deltaTime;
            if (_interval <= 0)
            {
                LoadModels();
            }
        }
        
        protected virtual void OnDestroy()
        {
            UnipromManager.OnInitialized -= LoadModels;
            _cts.SafeCancelAndDispose();
            _cts = default;
            foreach (var model in Models)
            {
                model.Dispose();
            }
            Models.Clear();
        }

        public void Reload()
        {
            StartAutoReload();
            LoadModels();
        }
        
        public void StartAutoReload()
        {
            if (AutoReload())
            {
                _interval = ReloadInterval();
            }
        }
        
        public void StopAutoReload() => _interval = 0;
        
        void LoadModels()
        {
            if (Models != default
                && Models.Count == ModelLength)
            {
#if DEBUG
                Debug.LogWarning("All models are being displayed. In that case, they are not reloaded to reduce processing costs.");
#endif
                return;
            }

            var length = Mathf.Clamp(GetLength() > 0 ? GetLength() : ModelLength, 1, ModelLength);
            LoadModelsAsync(_cts.Token, length)
                .SafeContinueWith(task =>
                {
                    if (task.Status != TaskStatus.RanToCompletion
                        || task.Result == default
                        || task.Result.Count == 0)
                    {
                        return;
                    }

                    var isReload = Models is {Count: > 0};
                    if (isReload)
                    {
                        foreach (var model in Models)
                        {
                            model.Dispose();
                        }
                        Models.Clear();
                    }
                    
                    Models = task.Result;
                    OnLoadModels(Models, isReload);
                    StartAutoReload();
                });
        }
    }
}
#endif
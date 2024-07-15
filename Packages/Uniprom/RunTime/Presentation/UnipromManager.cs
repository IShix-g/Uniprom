#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Uniprom
{
    public sealed class UnipromManager : MonoBehaviour
    {
        public static event Action OnInitialized = delegate {};
        public static event Action<UnipromViewShowStatus> OnShow = delegate { };
        public static event Action<UnipromViewHideStatus> OnHide = delegate { };
        public static event Action<UnipromViewErrorStatus> OnError = delegate { };
        
        public static UnipromManager Instance { get; private set; }
        
        public bool IsInitialized { get; private set; }
        public static int ModelIndex
        {
            get => PlayerPrefs.GetInt("UnipromManager_ModelIndex", -1);
            set => PlayerPrefs.SetInt("UnipromManager_ModelIndex", value);
        }
        public UnipromModelOrders ModelOrders { get; private set; }
        public int ModelLength => UnipromSettings.Current.Reference.ContentsLength;
        
        string _companyName;
        HashSet<string> _excludeAppKeys = new ();
        IUnipromViewClient<IUnipromInterstitialView> _interstitial;
        IUnipromViewClient<IUnipromWallView> _wall;
        RectTransform _popupParent;
        UnipromModelOrderType _modelListOrder;
        UnipromCanvasCreator _canvasCreator;
        readonly List<UnipromAnimation> _animations = new ();
        readonly List<int> _modelsIndexList = new ();
        readonly List<IUnipromModel> _modelsReusableList = new ();
        readonly List<Task<UnipromInterstitialModel>> _interstitialTaskReusableList = new();
        readonly List<Task<UnipromIconModel>> _iconTaskReusableList = new();
        
        public static void Initialize(
            bool createCanvas = false,
            string companyName = default,
            HashSet<string> excludeAppKeys = default,
            UnipromModelOrders modelOrders = default,
            IUnipromViewClient<IUnipromInterstitialView> interstitialViewClient = default,
            IUnipromViewClient<IUnipromWallView> wallViewClient = default)
        {
            if (Instance != default)
            {
                return;
            }
            
            InitializeAsync(createCanvas, companyName, excludeAppKeys, modelOrders, interstitialViewClient, wallViewClient)
                .SafeContinueWith(task =>
                {
                    if (task.IsFaulted
                        && !task.IsCanceled)
                    {
                        var status = new UnipromViewErrorStatus(
                            UnipromViewType.Initialize,
                            UnipromViewErrorType.InitializationFailed,
                            task.Exception);
                        OnError(status);
                    }
                });
        }

        public static async Task InitializeAsync(
            bool createCanvas = false,
            string companyName = default,
            HashSet<string> excludeAppKeys = default,
            UnipromModelOrders modelOrders = default,
            IUnipromViewClient<IUnipromInterstitialView> interstitialViewClient = default,
            IUnipromViewClient<IUnipromWallView> wallViewClient = default)
        {
            if (Instance != default)
            {
                return;
            }
            
            Instance = new GameObject("UnipromManager").AddComponent<UnipromManager>();
            DontDestroyOnLoad(Instance.gameObject);

            if (createCanvas)
            {
                Instance.CreateCanvasCreatorIfNeeded();
            }
            
            Instance._companyName = companyName;
            Instance._excludeAppKeys = excludeAppKeys ?? new HashSet<string>();
#if UNITY_IOS
            Instance._excludeAppKeys.Add(Application.identifier);
#else
            Instance._excludeAppKeys.Add(Application.productName);
#endif
            Instance.ModelOrders = modelOrders != default ? modelOrders : UnipromModelOrders.Default;
            Instance._interstitial = interstitialViewClient ?? new DefaultUnipromInterstitialViewClient();
            Instance._interstitial.Initialize(Instance);
            Instance._wall = wallViewClient ?? new DefaultUnipromWallViewClient();
            Instance._wall.Initialize(Instance);
            
            try
            {
                await UnipromSettings.Current.InitializeAsync();
                if (UnipromSettings.Current.Reference.ContentsLength <= Instance._excludeAppKeys.Count)
                {
                    throw new ArgumentException("Initialization cannot be completed because the number of excludeAppKeys exceeds the number of Contents.");
                }

                Instance.SetUpModelsIndexList(UnipromModelOrderType.InSequence);
                Instance.IsInitialized = true;
                OnInitialized();
            }
            catch (Exception e)
            {
                var status = new UnipromViewErrorStatus(
                    UnipromViewType.Initialize,
                    UnipromViewErrorType.InitializationFailed,
                    e);
                OnError(status);
#if DEBUG
                Debug.LogWarning(e);
#endif
                return;
            }
        }
        
        public void SetCompanyName(string name) => _companyName = name;
        
        public void SetParent(RectTransform parent) => _popupParent = parent;
        
        public bool CanShow() => IsInitialized && UnipromSettings.Current.IsInitialized;
        
        public IUnipromModel GetModel(UnipromModelOrderType orderType)
        {
            if (_modelsIndexList.Count == 0
                || _modelListOrder != orderType)
            {
                SetUpModelsIndexList(orderType);
            }

            ModelIndex = _modelsIndexList.Count - 1 > ModelIndex ? ModelIndex + 1 : 0;
            var index = _modelsIndexList[ModelIndex];
            return UnipromSettings.Current.Reference.GetModelByIndex(index);
        }
        
        public void GetModelList(UnipromModelOrderType orderType, int length, List<IUnipromModel> list, IReadOnlyCollection<string> excludeAppKeys = default)
        {
            if (length <= 0)
            {
                throw new AggregateException("length must be at least 1. length:" + length);
            }
            list.Clear();
            length = Mathf.Clamp(length, 1, _modelsIndexList.Count);

            if (_modelsIndexList.Count == 0
                || _modelListOrder != orderType
                || orderType == UnipromModelOrderType.Random)
            {
                SetUpModelsIndexList(orderType);
            }
            
            foreach (var index in _modelsIndexList)
            {
                var content = UnipromSettings.Current.Reference.GetModelByIndex(index);
                if (excludeAppKeys == default
                    || !Contains(excludeAppKeys, content.GetKey()))
                {
                    list.Add(content);
                }
                if (list.Count == length)
                {
                    break;
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task WaitUntilInitializeAsync(CancellationToken token)
        {
            while (!IsInitialized)
            {
                await Task.Delay(3000, token);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<IUnipromInterstitialView> LoadInterstitialAsync(
            CancellationToken token = default,
            Action showAction = default,
            Action hideAction = default,
            Action<UnipromViewErrorStatus> errorAction = default)
        {
            var view = await _interstitial.LoadAsync(
                token,
                status =>
                {
                    showAction?.Invoke();
                    OnShow(status);
                },
                status =>
                {
                    hideAction?.Invoke();
                    OnHide(status);
                },
                error =>
                {
                    errorAction?.Invoke(error);
                    OnError(error);
                });
            return view;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<IUnipromWallView> LoadWallAsync(
            CancellationToken token = default,
            Action showAction = default,
            Action hideAction = default,
            Action<UnipromViewErrorStatus> errorAction = default)
        {
            var view = await _wall.LoadAsync(
                token,
                status =>
                {
                    showAction?.Invoke();
                    OnShow(status);
                },
                status =>
                {
                    hideAction?.Invoke();
                    OnHide(status);
                },
                error =>
                {
                    errorAction?.Invoke(error);
                    OnError(error);
                });
            return view;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<List<UnipromInterstitialModel>> LoadInterstitialModelsAsync(CancellationToken token, int length, IReadOnlyCollection<string> excludeAppKeys)
        {
            try
            {
                if (!IsInitialized)
                {
                    await WaitUntilInitializeAsync(token);
                }
            }
            catch (Exception e)
            {
                if (!token.IsCancellationRequested)
                {
                    var status = new UnipromViewErrorStatus(
                        UnipromViewType.Native,
                        UnipromViewErrorType.UnknownError,
                        e);
                    OnError(status);
#if DEBUG
                    Debug.LogWarning(e);
#endif
                }
                return default;
            }
            
            GetModelList(Instance.ModelOrders.Native, length, _modelsReusableList, excludeAppKeys);

            return await UnipromExtensions.LoadModelsAsync<UnipromInterstitialModel>(
                model => new UnipromInterstitialModel(model),
                _modelsReusableList,
                _interstitialTaskReusableList,
                status =>
                {
                    OnError(status);
#if DEBUG
                    Debug.LogWarning(status.Exception);
#endif
                });
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<List<UnipromIconModel>> LoadIconModelsAsync(CancellationToken token, int length, IReadOnlyCollection<string> excludeAppKeys)
        {
            try
            {
                if (!IsInitialized)
                {
                    await WaitUntilInitializeAsync(token);
                }
            }
            catch (Exception e)
            {
                if (!token.IsCancellationRequested)
                {
                    var status = new UnipromViewErrorStatus(
                        UnipromViewType.Native,
                        UnipromViewErrorType.UnknownError,
                        e);
                    OnError(status);
#if DEBUG
                    Debug.LogWarning(e);
#endif
                }
                return default;
            }
            
            GetModelList(Instance.ModelOrders.NativeIcon, length, _modelsReusableList, excludeAppKeys);

            return await UnipromExtensions.LoadModelsAsync<UnipromIconModel>(
                model => new UnipromIconModel(model),
                _modelsReusableList,
                _iconTaskReusableList,
                status =>
                {
                    OnError(status);
#if DEBUG
                    Debug.LogWarning(status.Exception);
#endif
                });
        }
        
        public void SetHideStatus(UnipromViewHideStatus status) => OnHide(status);
        
        public void SetErrorStatus(UnipromViewErrorStatus status) => OnError(status);

        void CreateCanvasCreatorIfNeeded()
        {
            if (_canvasCreator == default)
            {
                _canvasCreator = gameObject.AddComponent<UnipromCanvasCreator>();
            }
        }
        
        public string GetCompanyName()
            => string.IsNullOrEmpty(_companyName)
                ? Application.companyName
                : _companyName;

        public RectTransform GetPopupParent()
        {
            if (_popupParent != default)
            {
                return _popupParent;
            }
            CreateCanvasCreatorIfNeeded();
            return _canvasCreator.Transform;
        }
        
        public UnipromAnimation GetAnimation(float startValue, float endValue, float duration, bool autoPlay = false)
        {
            var anim = new UnipromAnimation(startValue, endValue, duration, autoPlay);
            _animations.Add(anim);
            return anim;
        }
        
        void Update()
        {
            if (_animations.Count <= 0)
            {
                return;
            }

            var index = 0;
            while (_animations.Count > index)
            {
                var anim = _animations[index];
        
                if (anim.IsPlaying
                    && !anim.IsCompleted
                    && !anim.IsDestroyed)
                {
                    anim.Update(Time.deltaTime);
                }
                else if (anim.IsCompleted
                         || anim.IsDestroyed)
                {
                    if (!anim.IsDestroyed)
                    {
                        anim.Destroy();
                    }
                    _animations.Remove(anim);
                    continue;
                }
                index++;
            }
        }

        void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            if (_interstitial is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        
        void SetUpModelsIndexList(UnipromModelOrderType type)
        {
            _modelListOrder = type;
            var length = UnipromSettings.Current.Reference.ContentsLength;
            
            if (_modelsIndexList.Count == 0
                || type != UnipromModelOrderType.Random
                || (length > 0 && length != _modelsIndexList.Count))
            {
                _modelsIndexList.Clear();

                for (var i = 0; i < length; i++)
                {
                    var index = type == UnipromModelOrderType.InSequence ? i : (length - 1) - i;
                    var obj = UnipromSettings.Current.Reference.GetModelByIndex(index);
                    var key = obj.GetKey();
                    if (!_excludeAppKeys.Contains(key))
                    {
                        _modelsIndexList.Add(index);
                    }
                }
            }
            if (type == UnipromModelOrderType.Random)
            {
                IndexArrayShuffle(_modelsIndexList);
            }
        }
        
        void IndexArrayShuffle(List<int> array)
        {
            _modelListOrder = UnipromModelOrderType.Random;
            var rng = new System.Random();
            var n = array.Count;
            for (var i = 0; i < n; i++)
            {
                var r = i + rng.Next(n - i);
                (array[i], array[r]) = (array[r], array[i]);
            }
        }
        
        bool Contains(IEnumerable<string> collection, string value)
        {
            foreach (var item in collection)
            {
                if (item == value)
                    return true;
            }
            return false;
        }
    }
}
#endif
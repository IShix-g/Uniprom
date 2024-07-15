#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Uniprom
{
    public class UnipromInterstitialView : MonoBehaviour, IUnipromInterstitialView
    {
        [SerializeField] Image _bg;
        [SerializeField] RectTransform _parent;
        [SerializeField] CanvasGroup _canvasGroup;
        [SerializeField] Image _image;
        [SerializeField] Button _openUrlButton;
        [SerializeField] Button _hideButton;
        [SerializeField] UnipromText _appNameText;
        
        IUnipromViewCreator<IUnipromInterstitialView> _creator;
        IUnipromInterstitialModel _model;
        bool _isClicked;
        float _startColorAlpha;
        bool _isShowDetail;
        Action<UnipromViewShowStatus> _showAction;
        Action<UnipromViewHideStatus> _hideAction;

        public void Initialize(
            string companyName,
            IUnipromViewCreator<IUnipromInterstitialView> creator,
            IUnipromInterstitialModel model,
            Action<UnipromViewShowStatus> showAction,
            Action<UnipromViewHideStatus> hideAction)
        {
            _creator = creator;
            _model = model;
            _image.sprite = model.Sprite;
            _showAction = showAction;
            _hideAction = hideAction;
            _appNameText.Text = _model.Model.GetAppName();
        }

        void Awake()
        {
            var color = _bg.color;
            _startColorAlpha = color.a;
            color.a = 0;
            _bg.color = color;
            _bg.enabled = false;
            _parent.gameObject.SetActive(false);
            _canvasGroup.alpha = 0;
        }

        void OnDestroy()
        {
            _model.Dispose();
            _creator = default;
            _model = default;
        }

        void OpenUrl()
        {
            _isClicked = true;
#if UNITY_EDITOR
            Debug.Log("Click Open | iOSUrl: " + _model.Model.GetIOSUrl() + " AndroidUrl: " + _model.Model.GetAndroidUrl());
#else
            Application.OpenURL(_model.GetStoreUrl());
#endif
            Hide();
        }

        public GameObject RootObject => gameObject;

        public string AppKey => _model.Model.GetKey();

        public UnipromViewType ViewType => UnipromViewType.Interstitial;

        public void Show()
        {
            var status = new UnipromViewShowStatus(_model.Model.GetKey(), UnipromViewType.Interstitial);
            _showAction?.Invoke(status);
            var color = _bg.color;
            color.a = 0;
            _bg.color = color;
            _bg.enabled = true;
            _parent.gameObject.SetActive(true);
            _canvasGroup.alpha = 0;
            _parent.ScaleFromTo(1.025f, 1f, 0.2f);
            _canvasGroup.FadeTo(1, 0.2f).SetEase(UnipromEaseType.EaseOut);
            _bg.FadeTo(_startColorAlpha, 0.7f).SetEase(UnipromEaseType.EaseOut)
                .OnCompleted(_ =>
                {
                    _openUrlButton.onClick.AddListener(OpenUrl);
                    _hideButton.onClick.AddListener(Hide);
                });
        }

        public void Hide()
        {
            _openUrlButton.onClick.RemoveListener(OpenUrl);
            _hideButton.onClick.RemoveListener(Hide);
            var status = new UnipromViewHideStatus(_model.Model.GetKey(), UnipromViewType.Initialize, _isClicked);
            _hideAction?.Invoke(status);
            _creator.Release(this);
        }
    }
}
#endif
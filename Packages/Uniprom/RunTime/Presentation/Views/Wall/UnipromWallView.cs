#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Uniprom
{
    public sealed class UnipromWallView : MonoBehaviour, IUnipromWallView
    {
        [SerializeField] Image _bg;
        [SerializeField] RectTransform _parent;
        [SerializeField] RectTransform _contentsParent;
        [SerializeField] CanvasGroup _canvasGroup;
        [SerializeField] Button _hideButton;
        [SerializeField] UnipromWallViewContent _content;
        
        public GameObject RootObject => gameObject;
        public UnipromViewType ViewType => UnipromViewType.Wall;
        public string[] AppKeys => _appKeys;
        
        string[] _appKeys;
        IUnipromViewCreator<IUnipromWallView> _creator;
        List<IUnipromIconModel> _models;
        IUnipromModel _clickedModel;
        float _startColorAlpha;
        bool _isShowDetail;
        Action<UnipromViewShowStatus> _showAction;
        Action<UnipromViewHideStatus> _hideAction;

        public void Initialize(
            string companyName,
            IUnipromViewCreator<IUnipromWallView> creator,
            List<IUnipromIconModel> iconModels,
            Action<UnipromViewShowStatus> showAction,
            Action<UnipromViewHideStatus> hideAction)
        {
            _creator = creator;
            _models = iconModels;
            _showAction = showAction;
            _hideAction = hideAction;
            
            _appKeys = new string[iconModels.Count];
            var content = _content;
            for (var i = 0; i < iconModels.Count; i++)
            {
                var iconModel = iconModels[i];
                _appKeys[i] = iconModel.Model.GetKey();
                content.Initialize(this, iconModel);

                if (i < iconModels.Count - 1)
                {
                    content = Instantiate<UnipromWallViewContent>(_content, _contentsParent);
                }
            }
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
            foreach (var model in _models)
            {
                model.Dispose();
            }
            _creator = default;
            _models = default;
        }
        
        public void Show()
        {
            var status = new UnipromViewShowStatus(_models[0].Model.GetKey(), UnipromViewType.Wall);
            _showAction?.Invoke(status);
            var color = _bg.color;
            color.a = 0;
            _bg.color = color;
            _bg.enabled = true;
            _parent.gameObject.SetActive(true);
            _canvasGroup.alpha = 0;
            _parent.ScaleFromTo(1.025f, 1f, 0.2f);
            _canvasGroup.FadeTo(1, 0.2f).SetEase(UnipromEaseType.EaseOut);
            _bg.FadeTo(_startColorAlpha, 0.7f)
                .SetEase(UnipromEaseType.EaseOut)
                .OnCompleted(_ => _hideButton.onClick.AddListener(Hide));
        }

        public void Hide()
        {
            var status = new UnipromViewHideStatus(_clickedModel != default ? _clickedModel.GetKey() : "Unknown", UnipromViewType.Wall, _clickedModel != default);
            _hideAction?.Invoke(status);
            _creator.Release(this);
        }

        public void ClickContent(UnipromWallViewContent content)
        {
            _clickedModel = content.Model.Model;
#if UNITY_EDITOR
            Debug.Log("Click Open | iOSUrl: " + content.Model.Model.GetIOSUrl() + " AndroidUrl: " + content.Model.Model.GetAndroidUrl());
#else
            Application.OpenURL(content.Model.GetStoreUrl());
#endif
        }
    }
}
#endif
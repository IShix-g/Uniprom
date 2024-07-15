#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Uniprom
{
    [AddComponentMenu("Uniprom/View/UnipromNativeView")]
    public sealed class UnipromNativeView : UnipromInterstitialModelsProvider
    {
        [SerializeField] RectTransform _buttonsParent;
        
        Button[] _buttons;

        void Awake() => _buttons = _buttonsParent.GetComponentsInChildren<Button>();
        
        protected override int GetLength() => _buttons.Length;
        
        protected override bool AutoReload() => true;
        
        protected override float ReloadInterval() => 120;

        protected override void OnLoadModels(List<UnipromInterstitialModel> models, bool isReload)
        {
            if (isReload)
            {
                foreach (var button in _buttons)
                {
                    button.onClick.RemoveAllListeners();
                }
            }

            for (var i = 0; i < _buttons.Length; i++)
            {
                var button = _buttons[i];
                var isActive = i < models.Count;
                if (isActive)
                {
                    var interstitial = models[i];
                    button.image.sprite = interstitial.Sprite;
                    button.onClick.AddListener(() =>
                    {
                        var status = new UnipromViewHideStatus(
                            interstitial.Model.GetKey(),
                            UnipromViewType.Native,
                            true);
                        UnipromManager.Instance.SetHideStatus(status);
                        
#if UNITY_EDITOR
                        Debug.Log("Click Open | iOSUrl: " + interstitial.Model.GetIOSUrl() + " AndroidUrl: " + interstitial.Model.GetAndroidUrl());
#else
                        Application.OpenURL(interstitial.GetStoreUrl());
#endif
                    });
                }
                button.gameObject.SetActive(isActive);
            }
        }
    }
}
#endif
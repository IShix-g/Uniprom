#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using UnityEngine;
using UnityEngine.UI;

namespace Uniprom.Samples
{
    public sealed class AllPromotionsSample : MonoBehaviour
    {
        [SerializeField] Button _interstitialButton;
        [SerializeField] Button _wallButton;
        [SerializeField] UnipromNativeView _nativeView;
        [SerializeField] UnipromIconNativeView _iconNativeView;
        
        void Awake()
        {
            // Be sure to initialize it first; there is also InitializeAsync.
            UnipromManager.Initialize();
            // Passes parent element for dialog. The parent element's Canvas setting must be 1080 x 1920 or 1920 x 1080.
            // If not passed, a dedicated Canvas will be generated automatically.
            // var parent = FindObjectOfType<Canvas>().GetComponent<RectTransform>();
            // UnipromManager.Instance.SetParent(parent);
        }

        void Start()
        {
            _interstitialButton.interactable = false;
            _wallButton.interactable = false;
            _interstitialButton.onClick.AddListener(ClickInterstitial);
            _wallButton.onClick.AddListener(ClickWall);
        }

        void OnEnable()
        {
            UnipromManager.OnInitialized += OnInitialized;
            UnipromManager.OnShow += OnShow;
            UnipromManager.OnHide += OnHide;
            UnipromManager.OnError += OnError;
        }

        void OnDisable()
        {
            UnipromManager.OnInitialized -= OnInitialized;
            UnipromManager.OnShow -= OnShow;
            UnipromManager.OnHide -= OnHide;
            UnipromManager.OnError -= OnError;
        }
        
        void ClickInterstitial()
        {
            if (!UnipromManager.Instance.CanShow())
            {
                return;
            }
            UnipromManager.Instance.ShowInterstitial(
                gameObject,
                () =>
                {
                    Debug.Log("Show interstitial");
                },
                () =>
                {
                    Debug.Log("Hide interstitial");
                });
        }
        
        void ClickWall()
        {
            if (!UnipromManager.Instance.CanShow())
            {
                return;
            }
            UnipromManager.Instance.ShowWall(
                gameObject,
                () =>
                {
                    Debug.Log("Show Wall");
                },
                () =>
                {
                    Debug.Log("Hide Wall");
                });
        }
        
        void ClickNative()
        {
            if (_nativeView.CanReload)
            {
                _nativeView.Reload();
            }
        }

        void OnInitialized()
        {
            Debug.Log("Initialized");
            
            _interstitialButton.interactable = true;
            _wallButton.interactable = true;
        }

        void OnShow(UnipromViewShowStatus status) => Debug.Log("OnShow | " + status);
        
        void OnHide(UnipromViewHideStatus status) => Debug.Log("OnHide | " + status);
        
        void OnError(UnipromViewErrorStatus status) => Debug.Log("OnError | " + status);
    }
}
#endif
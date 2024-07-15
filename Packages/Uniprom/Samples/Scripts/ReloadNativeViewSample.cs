#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using UnityEngine;
using UnityEngine.UI;

namespace Uniprom.Samples
{
    public sealed class ReloadNativeViewSample : MonoBehaviour
    {
        [SerializeField] GameObject _view;
        [SerializeField] Button _reloadButton;

        IUnipromModelsProvider _provider;
        
        void Start()
        {
            if (_view.TryGetComponent<IUnipromModelsProvider>(out var provider))
            {
                _provider = provider;
            }
            else
            {
                Debug.LogError("Implement " + nameof(IUnipromModelsProvider) + " in the " + nameof(_view) + ".");
            }
            
            _reloadButton.onClick.AddListener(ClickButton);
        }

        void ClickButton()
        {
            if (_provider is {CanReload: true})
            {
                _provider.Reload();
            }
            else
            {
                Debug.LogError("Implement " + nameof(IUnipromModelsProvider) + " in the " + nameof(_view) + ".");
            }
        }
    }
}
#endif
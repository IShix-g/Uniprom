#if ENABLE_CMSUNIVORTEX
using UnityEngine;

namespace Uniprom
{
    [AddComponentMenu("Uniprom/Text/UnipromCompanyNameText")]
    public sealed class UnipromCompanyNameText : MonoBehaviour
    {
        [SerializeField] UnipromText _text;
        
        string _startText;
        
        void Start()
        {
            _startText = _text.Text;
            if (UnipromManager.Instance.IsInitialized)
            {
                OnInitialized();
            }
            else
            {
                _text.Text = string.Empty;
                UnipromManager.OnInitialized += OnInitialized;
            }
        }

        void OnDestroy() => UnipromManager.OnInitialized -= OnInitialized;

        void OnInitialized()
        {
            var text = _startText.Replace("{COMPANY_NAME}", UnipromManager.Instance.GetCompanyName());
            _text.Text = text;
        }

        void Reset() => _text = GetComponent<UnipromText>();
    }
}
#endif
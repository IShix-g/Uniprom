#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using UnityEngine;
using UnityEngine.UI;

namespace Uniprom
{
    public sealed class UnipromPlatformSpecificDisplay : MonoBehaviour
    {
        [SerializeField] Image _image;
        [SerializeField] Sprite[] _logos;
        [SerializeField] UnipromText _text;
        
        void Awake()
        {
#if UNITY_IOS
            _text.Text = "App Store";
            _image.sprite = _logos[0];
#else
            _text.Text = "Google Play";
            _image.sprite = _logos[1];
#endif
        }
    }
}
#endif
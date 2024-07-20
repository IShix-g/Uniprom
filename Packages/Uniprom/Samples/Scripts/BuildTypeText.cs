#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using UnityEngine;
using UnityEngine.UI;

namespace Uniprom.Samples
{
    [RequireComponent(typeof(Text))]
    public sealed class BuildTypeText : MonoBehaviour
    {
        [SerializeField] Text _text;

        void Start() => _text.text = "Build type : " + UnipromSettings.Current.BuildType;
        
        void Reset() => _text = GetComponent<Text>();
    }
}
#endif
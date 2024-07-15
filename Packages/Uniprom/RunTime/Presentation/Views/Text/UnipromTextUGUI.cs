#if ENABLE_CMSUNIVORTEX
using UnityEngine;
using UnityEngine.UI;

namespace Uniprom
{
    [AddComponentMenu("Uniprom/Text/UnipromTextUGUI")]
    [RequireComponent(typeof(Text))]
    public sealed class UnipromTextUGUI : UnipromText
    {
        [SerializeField] Text _text;

        public override string Text
        {
            get => _text.text;
            set => _text.text = value;
        }
        
        public override Color Color
        {
            get => _text.color;
            set => _text.color = value;
        }
        
        void Reset() => _text = GetComponent<Text>();
    }
}
#endif
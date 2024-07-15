#if ENABLE_TEXTMESHPRO && ENABLE_CMSUNIVORTEX
using TMPro;
using UnityEngine;

namespace Uniprom
{
    [AddComponentMenu("Uniprom/Text/UnipromTextMeshPro")]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class UnipromTextMeshPro : UnipromText
    {
        [SerializeField] TextMeshProUGUI _text;

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

        void Reset() => _text = GetComponent<TextMeshProUGUI>();
    }
}
#endif
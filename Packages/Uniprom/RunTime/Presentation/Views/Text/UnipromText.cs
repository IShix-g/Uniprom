#if ENABLE_CMSUNIVORTEX
using UnityEngine;

namespace Uniprom
{
    public abstract class UnipromText : MonoBehaviour
    {
        public abstract string Text { get; set; }
        public abstract Color Color { get; set; }
    }
}
#endif
#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using UnityEngine;

namespace Uniprom
{
    public interface IUnipromView
    {
        GameObject RootObject { get; }
        UnipromViewType ViewType { get; }
        void Show();
        void Hide();
    }
}
#endif
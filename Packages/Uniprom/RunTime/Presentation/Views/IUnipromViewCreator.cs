#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System.Threading.Tasks;
using UnityEngine;

namespace Uniprom
{
    public interface IUnipromViewCreator<T> where T : IUnipromView
    {
        Task<T> CreatePrefabAsync(RectTransform parent);
        void Release(T obj);
    }
}
#endif
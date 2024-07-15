
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Uniprom
{
    public interface IUnipromReference
    {
        int ContentsLength { get; }
        bool IsInitialized { get; }
        bool IsLoading { get; }
        SystemLanguage Language { get; }
        IEnumerator Initialize(Action onLoaded = default);
        IEnumerator Initialize(SystemLanguage language, Action onLoaded = default);
        Task InitializeAsync();
        Task InitializeAsync(SystemLanguage language);
        IUnipromModel GetModelById(string id);
        bool TryGetModelById(string id, out IUnipromModel model);
        IUnipromModel GetModelByIndex(int index);
    }
}

using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Uniprom
{
    public interface IUnipromReference
    {
        int ContentsLength { get; }
        bool IsInitializedLocalize { get; }
        bool IsLoading { get; }
        SystemLanguage FindLanguage(SystemLanguage language);
        IEnumerator WaitForLoadLocalizationCo(Action onReady = default);
        Task WaitForLoadLocalizationAsync(CancellationToken token = default);
        IUnipromModel GetModelByKey(string id);
        bool TryGetModelByKey(string id, out IUnipromModel model);
        IUnipromModel GetModelByIndex(int index);
    }
}
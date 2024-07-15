#if DEBUG && ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using System.Threading;
using Uniprom;
using UnityEngine;

public sealed class SyncTest : MonoBehaviour
{
    CancellationTokenSource _cts;
    
    void Start()
    {
        UnipromManager.Initialize();
        var parent = FindObjectOfType<Canvas>().GetComponent<RectTransform>();
        UnipromManager.Instance.SetParent(parent);
        UnipromManager.Instance.ShowInterstitial();
     }
}
#endif
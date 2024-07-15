
using System;
using UnityEngine;

namespace Uniprom
{
    public interface IUnipromInterstitialModel : IUnipromViewModel, IDisposable
    {
        Sprite Sprite { get; }
    }
}

using System;
using UnityEngine;

namespace Uniprom
{
    public interface IUnipromIconModel : IUnipromViewModel, IDisposable
    {
        Sprite Sprite { get; }
    }
}
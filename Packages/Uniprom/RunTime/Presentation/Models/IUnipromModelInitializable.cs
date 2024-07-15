
using System;
using System.Threading.Tasks;

namespace Uniprom
{
    public interface IUnipromModelInitializable : IDisposable
    {
        bool IsLoaded { get; }
        IUnipromModel Model { get; }
        Task LoadAsync();
    }
}
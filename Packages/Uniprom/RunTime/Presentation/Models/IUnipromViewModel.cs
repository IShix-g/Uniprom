
namespace Uniprom
{
    public interface IUnipromViewModel
    {
        IUnipromModel Model { get; }
        string GetStoreUrl();
    }
}
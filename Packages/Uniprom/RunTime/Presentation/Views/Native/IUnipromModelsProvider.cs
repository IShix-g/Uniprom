#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
namespace Uniprom
{
    public interface IUnipromModelsProvider
    {
        bool CanShow { get; }
        bool CanReload { get; }
        bool HasModel { get; }
        int ModelLength { get; }
        void Reload();
        void StartAutoReload();
        void StopAutoReload();
    }
}
#endif
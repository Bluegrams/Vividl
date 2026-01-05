using System.Threading.Tasks;

namespace Vividl.Services.Update
{
    public interface ILibUpdateService
    {
        string Version { get; }
        bool IsUpdating { get; }
        Task<bool> CheckForUpdates();
        Task<string> Update();
    }
}

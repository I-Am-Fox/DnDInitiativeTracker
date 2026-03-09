using DnDInitiativeTracker.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDInitiativeTracker.Core.Interfaces.Services
{
    public interface IUpdateService
    {
        Task<UpdateInfo?> CheckForUpdatesAsync();
        Task<bool> DownloadAndInstallUpdateAsync(UpdateInfo updateInfo);
        string GetCurrentVersion();
    }
}

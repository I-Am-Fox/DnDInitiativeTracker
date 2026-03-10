using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDInitiativeTracker.Core.Config
{
    public static class AppInfo
    {
        public static string ApplicationName { get; set; } = "DnDInitiativeTracker";
        public static string ApplicationVersion { get; set; } = "1.0.1";
        public static string SettingsFolderName { get; set; } = "DnDInitiativeTracker";
        public static string GitHubRepositoryOwner { get; set; } = "I-Am-Fox";
        public static string GitHubRepositoryName { get; set; } = "DnDInitiativeTracker";

        public static string GetApplicationVersion()
        {
            var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
            var info = entryAssembly?.GetName().Version;
            var informational = entryAssembly is null
                ? null
                : System.Attribute.GetCustomAttribute(
                    entryAssembly,
                    typeof(System.Reflection.AssemblyInformationalVersionAttribute))
                    is System.Reflection.AssemblyInformationalVersionAttribute a
                        ? a.InformationalVersion
                        : null;

            if (!string.IsNullOrWhiteSpace(informational))
            {
                var plus = informational.IndexOf('+');
                return plus >= 0 ? informational[..plus] : informational;
            }

            return info?.ToString() ?? ApplicationVersion;
        }
    }
}

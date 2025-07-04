using System.Runtime.InteropServices;

namespace OpenCodeUpdater.Services;

public class PlatformDetectionService
{
    public string GetPlatformPattern()
    {
        if (OperatingSystem.IsWindows())
        {
            return Environment.Is64BitOperatingSystem ? "windows-x64" : "windows-x86";
        }
        else if (OperatingSystem.IsMacOS())
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "darwin-arm64" : "darwin-x64";
        }
        else if (OperatingSystem.IsLinux())
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "linux-arm64" : "linux-x64";
        }
        else
        {
            string rid = RuntimeInformation.RuntimeIdentifier;
            Console.WriteLine($"Unknown platform, using runtime identifier: {rid}");
            return rid.Contains("win") ? "windows-x64" :
                   rid.Contains("osx") || rid.Contains("darwin") ? "darwin-x64" :
                   "linux-x64";
        }
    }
}
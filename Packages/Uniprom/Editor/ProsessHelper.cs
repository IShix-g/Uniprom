
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Uniprom.Editor
{
    public static class ProcessHelper
    {
        public static void OpenFolder(string path)
        {
            if (Path.HasExtension(path))
            {
                path = Path.GetDirectoryName(path);
            }
            
            if (!Directory.Exists(path))
            {
                Debug.LogError("The folder does not exist: " + path);
                return;
            }

            if (Application.platform == RuntimePlatform.WindowsPlayer ||
                Application.platform == RuntimePlatform.WindowsEditor)
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = "explorer.exe",
                    Arguments = path
                });
            }
            else if (Application.platform == RuntimePlatform.OSXPlayer ||
                     Application.platform == RuntimePlatform.OSXEditor)
            {
                Process.Start("open", path);
            }
            else if (Application.platform == RuntimePlatform.LinuxPlayer ||
                     Application.platform == RuntimePlatform.LinuxEditor)
            {
                Process.Start("xdg-open", path);
            }
            else
            {
                Debug.LogError("Unsupported platform: " + Application.platform);
            }
        }
    }
}

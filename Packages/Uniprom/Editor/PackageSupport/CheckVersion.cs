
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Uniprom.Editor
{
    internal sealed class CheckVersion
    {
        internal static async Task<string> GetVersionOnServer(string gitUrl)
        {
            using var request = UnityWebRequest.Get(gitUrl);
            await request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                return GetVersionFromJson(request.downloadHandler.text);
            }
            
            Debug.LogError("GetVersion error: " + request.error);
            return string.Empty;
        }
        
        internal static string GetCurrent(string packagePath)
        {
            var path = Path.Combine(packagePath, "package.json");
            var json = File.ReadAllText(path);
            return GetVersionFromJson(json);
        }

        internal static string GetVersionFromUrl(string url)
        {
            var index = url.LastIndexOf('#');
            if (index == -1)
            {
                index = url.LastIndexOf('@');
            }
            if (index >= 0
                && index + 1 < url.Length)
            {
                return url.Substring(index + 1);
            }
            return string.Empty;
        }
        
        internal static void OpenPackageManager()
        {
            EditorApplication.ExecuteMenuItem("Window/Package Manager");
        }

        static string GetVersionFromJson(string json)
        {
            var obj = JsonUtility.FromJson<VersionString>(json);
            return obj?.version;
        }

        class VersionString
        {
            public string version;
        }
    }
}
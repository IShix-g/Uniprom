#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Uniprom.Editor
{
    public sealed class FtpUploader
    {
        readonly ServerInfo _info;

        [Serializable]
        public sealed class ServerInfo
        {
            public string ServerAddress;
            public string UserName;
            public string Password;
            public string ServerBaseDirectory;
            public bool EnableSSL;

            public bool IsValid()
                => !string.IsNullOrEmpty(ServerAddress)
                   && !string.IsNullOrEmpty(UserName)
                   && !string.IsNullOrEmpty(Password)
                   && !string.IsNullOrEmpty(ServerBaseDirectory);
        }
        
        public FtpUploader(string json) : this(JsonUtility.FromJson<ServerInfo>(json)){}
        
        public FtpUploader(ServerInfo info) => _info = info;
        
        public async Task UploadFilesAsync(string rootPath, IReadOnlyList<string> filePaths)
        {
            UnipromDebug.Log("Uploading of " + filePaths.Count + " files has started.");
            var tasks = filePaths.Select(filePath => UploadFileAsync(rootPath, filePath));
            await Task.WhenAll(tasks);
            UnipromDebug.Log(filePaths.Count + " files uploaded successfully");
        }
        
        async Task UploadFileAsync(string rootPath, string filePath)
        {
            await Task.Run(() => UploadFile(rootPath, filePath));
        }
        
        void UploadFile(string rootPath, string filePath)
        {
            var relativePath = GetRelativePath(rootPath, filePath);
            var ftpPath = relativePath.Replace(Path.DirectorySeparatorChar, '/');
            
            EnsureDirectoryExists(ftpPath);
            
            var url = $"ftp://{_info.ServerAddress}/{_info.ServerBaseDirectory}/{ftpPath}";
            var request = (FtpWebRequest) WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.UseBinary = true;
            request.UsePassive = true;
            request.KeepAlive = true;
            request.Credentials = new NetworkCredential(_info.UserName, _info.Password);
            request.EnableSsl = _info.EnableSSL;
            
            var fileContents = default(byte[]);
            using (var sourceStream = File.OpenRead(filePath))
            {
                fileContents = new byte[sourceStream.Length];
                _ = sourceStream.Read(fileContents, 0, fileContents.Length);
            }
            
            request.ContentLength = fileContents.Length;
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(fileContents, 0, fileContents.Length);
            }
            
            using (var response = (FtpWebResponse) request.GetResponse())
            {
                UnipromDebug.Log($"Upload File Complete, path:{url} status:{response.StatusDescription}");
            }
        }
        
        void EnsureDirectoryExists(string path)
        {
            if (Path.HasExtension(path))
            {
                path = Path.GetDirectoryName(path);
            }
            var paths = path.Split('/');
            var currPath = $"ftp://{_info.ServerAddress}/{_info.ServerBaseDirectory}";

            foreach (var subPath in paths)
            {
                if (string.IsNullOrEmpty(subPath))
                {
                    continue;
                }

                currPath += "/" + subPath;

                var request = (FtpWebRequest) WebRequest.Create(currPath);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = true;
                request.Credentials = new NetworkCredential(_info.UserName, _info.Password);
                request.EnableSsl = _info.EnableSSL;

                try
                {
                    using var response = (FtpWebResponse) request.GetResponse();
                    UnipromDebug.Log($"Create directory complete, status {response.StatusDescription}");
                }
                catch (WebException ex)
                {
                    var response = (FtpWebResponse)ex.Response;
                    if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        // Debug.Log($"Directory already exists, or another error occurred: {currPath}. Full exception: {ex}");
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
        
        string GetRelativePath(string rootPath, string filePath)
        {
            var uri = new Uri(rootPath);
            var fileUri = new Uri(filePath);
            var relativeUri = uri.MakeRelativeUri(fileUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());
            return relativePath;
        }
    }
}
#endif
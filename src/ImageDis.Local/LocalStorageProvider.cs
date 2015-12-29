using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageDis.Local
{
    public class LocalStorageProvider : IStorageProvider
    {
        private readonly string _filePath;
        private readonly string _redirectPath;

        public LocalStorageProvider(string filePath, string redirectPath)
        {
            _filePath = filePath;
            _redirectPath = redirectPath;
        }

        public Task<bool> CheckIfKeyExists(string key)
        {
            if (Directory.Exists(_filePath))
                return Task.FromResult(Directory.GetFiles(_filePath, $"{key}.*").Length > 0);
            else
                return Task.FromResult(false);
        }

        public async Task SaveFile(string key, string contentType, Stream stream, ImageDisOptions options)
        {
            if (!Directory.Exists(_filePath))
                Directory.CreateDirectory(_filePath);

            using (var f = File.OpenWrite($"{_filePath.TrimEnd('\\')}\\{key}.{ContentTypeToExtension(contentType, options)}"))
            {
                var buffer = new byte[8 * 1024];
                int len;
                while ((len = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    await f.WriteAsync(buffer, 0, len);
                }
            }
        }

        public Task<string> GetRedirectPath(string key)
        {
            var file = Path.GetExtension(Directory.GetFiles(_filePath, $"{key}.*").First());
            return Task.FromResult($"{_redirectPath}/{key}{file}");
        }

        public async Task<ImageDisFile> GetFile(string key, ImageDisOptions options)
        {
            var file = Directory.GetFiles(_filePath, $"{key}.*").FirstOrDefault();
            using (var f = File.OpenRead(file))
            {
                var ms = new MemoryStream();
                await f.CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);

                return new ImageDisFile
                {
                    ContentType = ExtensionToContentType(Path.GetExtension(file).TrimStart('.'), options),
                    Stream = ms
                };
            }
        }

        private string ExtensionToContentType(string extension, ImageDisOptions options)
        {
            if (options.AllowedImageTypes != null)
            {
                foreach (var allowedImageType in options.AllowedImageTypes)
                {
                    if (extension.Equals(allowedImageType.FileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        return allowedImageType.ContentType;
                    }
                }
            }

            throw new Exception("Unsupported extension.");
        }


        private string ContentTypeToExtension(string contentType, ImageDisOptions options)
        {
            if (options.AllowedImageTypes != null)
            {
                foreach (var allowedImageType in options.AllowedImageTypes)
                {
                    if (contentType.Equals(allowedImageType.ContentType, StringComparison.OrdinalIgnoreCase))
                    {
                        return allowedImageType.FileExtension;
                    }
                }
            }
            
            throw new Exception("Unsupported content type.");
        }
    }
}

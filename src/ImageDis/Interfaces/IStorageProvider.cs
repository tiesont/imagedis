using System.IO;
using System.Threading.Tasks;

namespace ImageDis
{
    public interface IStorageProvider
    {
        Task<bool> CheckIfKeyExists(string key);
        
        Task<string> GetRedirectPath(string key);

        Task SaveFile(string key, string contentType, Stream stream, ImageDisOptions options);

        Task<ImageDisFile> GetFile(string key, ImageDisOptions options);
    }
}

using System.IO;
using System.Threading.Tasks;

namespace ImageDis
{
    public interface IStorageProvider
    {
        Task<bool> CheckIfKeyExists(string key);

        Task SaveFile(string key, string contentType, Stream stream);

        Task<string> GetRedirectPath(string key);

        Task<ImageDisFile> GetFile(string key);
    }
}

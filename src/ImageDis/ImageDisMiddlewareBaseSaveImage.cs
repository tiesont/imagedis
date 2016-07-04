using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ImageDis
{
    public abstract partial class ImageDisMiddlewareBase
    {
        private bool IsImagePost(object context)
        {
            var request = GetRequest(context);
            return (request.Path.Equals("/") || request.Path.Equals(string.Empty))
                && request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase);
        }

        private async Task SaveImage(object context)
        {
            var request = GetRequest(context);
            var files = GetFiles(context);

            if (!files.Any())
            {
                RespondWithStatusCode(context, HttpStatusCode.BadRequest);
                return;
            }

            var file = files.First();

            // check acceptable mime types
            if (!IsAllowedImageType(file.ContentType))
            {
                RespondWithStatusCode(context, HttpStatusCode.UnsupportedMediaType);
                return;
            }

            // generate key
            var key = _options.KeyGenerator.GetKey();
            while (await _options.StorageProvider.CheckIfKeyExists(key))
                key = _options.KeyGenerator.GetKey();

            // save file
            var stream = file.Stream;
            var size = await ImageProcessorHelpers.GetSize(stream);
            await _options.StorageProvider.SaveFile(key, file.ContentType, stream, _options);

            // respond with image id
            await RespondWithJson(context, new
            {
                key = key,
                url = request.Scheme + "://" + request.Host + _options.Path + "/" + key,
                width = size.Width,
                height = size.Height
            });
        }
        
        private bool IsAllowedImageType(string contentType)
        {
            if (_options.AllowedImageTypes != null)
            {
                foreach (var allowedImageType in _options.AllowedImageTypes)
                {
                    if (contentType.Equals(allowedImageType.ContentType, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            return false;
        }
    }
}

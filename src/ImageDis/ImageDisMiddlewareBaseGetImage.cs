using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ImageDis
{
    public abstract partial class ImageDisMiddlewareBase
    {
        private bool IsImageGet(object context)
        {
            var request = GetRequest(context);
            return request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)
                && _getImageRegex.IsMatch(request.Path.ToString());
        }

        private async Task GetImage(object context)
        {
            // extract the key
            var request = GetRequest(context);
            var match = _getImageRegex.Match(request.Path);
            var key = match.Groups[1].Value;

            // check if it even exists
            if (!await _options.StorageProvider.CheckIfKeyExists(key))
            {
                RespondWithStatusCode(context, HttpStatusCode.NotFound);
                return;
            }

            // parse the params
            var param = ParseParameters(request.Query);
            var parts = ImageProcessorHelpers.GetParameterAsParts(param);

            // check if the new key exists (or if no param parts were added to the end, the original)
            var newKey = key;
            if (parts.Any())
                newKey = newKey + "_" + string.Join("_", parts);

            if (newKey == key || await _options.StorageProvider.CheckIfKeyExists(newKey))
            {
                RespondWithRedirect(context, await _options.StorageProvider.GetRedirectPath(newKey));
                return;
            }

            // apply transformations, save to storage and redirect user
            var original = await _options.StorageProvider.GetFile(key, _options);
            var newFile = await ImageProcessorHelpers.TransformImage(original.Stream, param);
            await _options.StorageProvider.SaveFile(newKey, original.ContentType, newFile, _options);
            RespondWithRedirect(context, await _options.StorageProvider.GetRedirectPath(newKey));
        }

        private ImageDisParameters ParseParameters(IDictionary<string, IEnumerable<string>> query)
        {
            var param = new ImageDisParameters();

            foreach (var q in query)
            {
                int integer;
                bool boolean;

                if (int.TryParse(q.Value.First(), out integer))
                    param.Add(q.Key, integer);
                else if (bool.TryParse(q.Value.First(), out boolean))
                    param.Add(q.Key, boolean);
                else
                    param.Add(q.Key, q.Value.First());
            }

            return param;
        }
    }
}

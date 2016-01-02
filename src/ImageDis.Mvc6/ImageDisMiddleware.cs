using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImageDis.Mvc6
{
    class ImageDisMiddleware
    {
        private readonly ImageDisOptions _options;
        private readonly Regex _getImageRegex;
        private readonly RequestDelegate _next;

        public ImageDisMiddleware(RequestDelegate next, ImageDisOptions options)
        {
            _next = next;
            _options = options;
            _getImageRegex = new Regex(@"/([A-Za-z0-9\.]+)", RegexOptions.IgnoreCase);
        }

        public async Task Invoke(HttpContext context)
        {
            if (IsImagePost(context))
                await SaveImage(context);
            else if (IsImageGet(context))
                await GetImage(context);
            else
                await _next.Invoke(context);
        }

        private bool IsImagePost(HttpContext context)
        {
            return (context.Request.Path.Equals("/") || context.Request.Path.Equals(string.Empty))
                && context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsImageGet(HttpContext context)
        {
            return context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)
                && _getImageRegex.IsMatch(context.Request.Path.ToString());
        }

        private async Task SaveImage(HttpContext context)
        {
            var files = context.Request.Form.Files.Any();

            if (!context.Request.Form.Files.Any())
            {
                context.Response.BadRequest();
                return;
            }

            var file = context.Request.Form.Files.First();

            // check acceptable mime types
            if (!IsAllowedImageType(file.ContentType))
            {
                context.Response.UnsupportedMediaType();
                return;
            }

            // generate key
            var key = _options.KeyGenerator.GetKey();
            while (await _options.StorageProvider.CheckIfKeyExists(key))
                key = _options.KeyGenerator.GetKey();

            // save file
            var stream = file.OpenReadStream();
            var size = await _options.ImageTransformProvider.GetSize(stream);
            await _options.StorageProvider.SaveFile(key, file.ContentType, stream, _options);

            // respond with image id
            await context.Response.Json(new
            {
                key = key,
                url = context.Request.Scheme + "://" + context.Request.Host + _options.Path + "/" + key,
                width = size.Width,
                height = size.Height
            });
        }

        private async Task GetImage(HttpContext context)
        {
            // extract the key
            var match = _getImageRegex.Match(context.Request.Path.ToString());
            var key = match.Groups[1].Value;

            // check if it even exists
            if (!await _options.StorageProvider.CheckIfKeyExists(key))
            {
                context.Response.NotFound();
                return;
            }

            // parse the params
            var param = new ImageDisParameters().AppendParameters(context.Request.Query);
            var parts = _options.ImageTransformProvider.GetParameterAsParts(param);

            // check if the new key exists (or if no param parts were added to the end, the original)
            var newKey = key;
            if (parts.Any())
                newKey = newKey + "_" + string.Join("_", parts);

            if (newKey == key || await _options.StorageProvider.CheckIfKeyExists(newKey))
            {
                context.Response.Redirect(await _options.StorageProvider.GetRedirectPath(newKey));
                return;
            }

            // apply transformations, save to storage and redirect user
            var original = await _options.StorageProvider.GetFile(key, _options);
            var newFile = await _options.ImageTransformProvider.TransformImage(original.Stream, param);
            await _options.StorageProvider.SaveFile(newKey, original.ContentType, newFile, _options);
            context.Response.Redirect(await _options.StorageProvider.GetRedirectPath(newKey));
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

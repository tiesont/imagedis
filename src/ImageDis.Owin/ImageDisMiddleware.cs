using ImageDis.Owin.Utility;
using Microsoft.Owin;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ImageDis.Owin
{
    class ImageDisMiddleware : OwinMiddleware
    {

        private readonly ImageDisOptions _options;
        private readonly Regex _getImageRegex;

        public ImageDisMiddleware(OwinMiddleware next, ImageDisOptions options)
            : base(next)
        {
            _options = options;
            _getImageRegex = new Regex(options.Path + @"/([A-Za-z0-9\.]+)", RegexOptions.IgnoreCase);
        }

        public async override Task Invoke(IOwinContext context)
        {
            if (IsImagePost(context))
                await SaveImage(context);
            else if (IsImageGet(context))
                await GetImage(context);
            else
                await Next.Invoke(context);
        }

        private bool IsImagePost(IOwinContext context)
        {
            return context.Request.Path.StartsWithSegments(new PathString(_options.Path))
                && context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsImageGet(IOwinContext context)
        {
            return context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase)
                && _getImageRegex.IsMatch(context.Request.Path.ToString());
        }

        private async Task SaveImage(IOwinContext context)
        {
            var formParser = new FormParser(context.Request);
            var form = await formParser.ReadFormAsync(CancellationToken.None);

            if (form == null || !form.Files.Any())
            {
                context.Response.BadRequest();
                return;
            }

            var file = form.Files.First();

            // check acceptable mime types
            if (_options.AllowedContentTypes != null && !_options.AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
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
            await _options.StorageProvider.SaveFile(key, file.ContentType, stream);

            // respond with image id
            await context.Response.Json(new
            {
                key = key,
                url = context.Request.Scheme + "://" + context.Request.Host + _options.Path + "/" + key
            });
        }

        private async Task GetImage(IOwinContext context)
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
            var original = await _options.StorageProvider.GetFile(key);
            var newFile = await _options.ImageTransformProvider.TransformImage(original.Stream, param);
            await _options.StorageProvider.SaveFile(newKey, original.ContentType, newFile);
            context.Response.Redirect(await _options.StorageProvider.GetRedirectPath(newKey));
        }
    }
}

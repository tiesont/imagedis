using ImageDis.Owin.Utility;
using Microsoft.Owin;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ImageDis.Owin
{
    class ImageDisMiddlewareImpl : ImageDisMiddlewareBase
    {
        private readonly ImageDisMiddleware _middleware;

        public ImageDisMiddlewareImpl(ImageDisMiddleware middleware, ImageDisOptions options)
            : base(options, options.Path)
        {
            _middleware = middleware;
        }

        #region Implementation

        protected override ImageDisRequest GetRequest(object context)
        {
            var ctx = (IOwinContext)context;
            var path = ctx.Request.Path.ToString();
            var isImageDis = path.StartsWith(_options.Path);
            return new ImageDisRequest
            {
                Scheme = ctx.Request.Scheme,
                Host = ctx.Request.Host.ToString(),
                Method = ctx.Request.Method,
                IsImageDis = isImageDis,
                Path = isImageDis ? path.Substring(path.Length) : path,
                Query = ctx.Request.Query.ToDictionary(
                    kv => kv.Key,
                    kv => (IEnumerable<string>)kv.Value.ToArray())
            };
        }

        protected async override Task<IEnumerable<ImageDisFile>> GetFiles(object context)
        {
            var ctx = (IOwinContext)context;
            var formParser = new FormParser(ctx.Request);
            var form = await formParser.ReadFormAsync(CancellationToken.None);

            return form.Files.Select(f => new ImageDisFile
            {
                ContentType = f.ContentType,
                Stream = f.OpenReadStream()
            }).ToArray();
        }

        protected override void RespondWithStatusCode(object context, HttpStatusCode statusCode)
        {
            var ctx = (IOwinContext)context;
            ctx.Response.StatusCode = (int)statusCode;
        }

        protected override void RespondWithRedirect(object context, string path)
        {
            var ctx = (IOwinContext)context;
            ctx.Response.StatusCode = (int)HttpStatusCode.TemporaryRedirect;
            ctx.Response.Headers.Add("Location", new[] { path });
        }

        protected async override Task RespondWithJson(object context, object obj)
        {
            var ctx = (IOwinContext)context;
            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonConvert.SerializeObject(obj));
        }

        #endregion
    }
}

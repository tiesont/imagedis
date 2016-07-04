using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ImageDis.AspNetCore
{
    class ImageDisMiddleware : ImageDisMiddlewareBase
    {
        private readonly RequestDelegate _next;

        public ImageDisMiddleware(RequestDelegate next, ImageDisOptions options)
            : base(options, string.Empty)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var processed = await Process(context);
            if (!processed)
                await _next.Invoke(context);
        }

        #region Implementation

        protected override ImageDisRequest GetRequest(object context)
        {
            var ctx = (HttpContext)context;
            return new ImageDisRequest
            {
                Scheme = ctx.Request.Scheme,
                Host = ctx.Request.Host.ToString(),
                Method = ctx.Request.Method,
                Path = ctx.Request.Path,
                Query = ctx.Request.Query.ToDictionary(
                    kv => kv.Key,
                    kv => (IEnumerable<string>)kv.Value.ToArray())
            };
        }

        protected override IEnumerable<ImageDisFile> GetFiles(object context)
        {
            var ctx = (HttpContext)context;
            return ctx.Request.Form.Files.Select(f => new ImageDisFile
            {
                ContentType = f.ContentType,
                Stream = f.OpenReadStream()
            }).ToArray();
        }

        protected override void RespondWithStatusCode(object context, HttpStatusCode statusCode)
        {
            var ctx = (HttpContext)context;
            ctx.Response.StatusCode = (int)statusCode;
        }

        protected override void RespondWithRedirect(object context, string path)
        {
            var ctx = (HttpContext)context;
            ctx.Response.StatusCode = (int)HttpStatusCode.TemporaryRedirect;
            ctx.Response.Headers.Add("Location", new[] { path });
        }

        protected async override Task RespondWithJson(object context, object obj)
        {
            var ctx = (HttpContext)context;
            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonConvert.SerializeObject(obj));
        }

        #endregion
    }
}

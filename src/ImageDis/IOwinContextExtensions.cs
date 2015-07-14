using Microsoft.Owin;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;

namespace ImageDis
{
    public static class IOwinResponseExtensions
    {
        public static async Task Json(this IOwinResponse response, object json)
        {
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "application/json";
            await response.WriteAsync(JsonConvert.SerializeObject(json));
        }

        public static void NotFound(this IOwinResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
        }

        public static void BadRequest(this IOwinResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
        }

        public static void UnsupportedMediaType(this IOwinResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
        }

        public static void Redirect(this IOwinResponse response, string url)
        {
            response.StatusCode = (int)HttpStatusCode.TemporaryRedirect;
            response.Headers.Add("Location", new[] { url });
        }
    }
}

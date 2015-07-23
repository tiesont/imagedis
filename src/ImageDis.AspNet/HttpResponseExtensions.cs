using Microsoft.AspNet.Http;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;

namespace ImageDis.AspNet
{
    public static class HttpResponseExtensions
    {
        public static async Task Json(this HttpResponse response, object json)
        {
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "application/json";
            await response.WriteAsync(JsonConvert.SerializeObject(json));
        }

        public static void NotFound(this HttpResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
        }

        public static void BadRequest(this HttpResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
        }

        public static void UnsupportedMediaType(this HttpResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
        }

        public static void Redirect(this HttpResponse response, string url)
        {
            response.StatusCode = (int)HttpStatusCode.TemporaryRedirect;
            response.Headers.Add("Location", new[] { url });
        }
    }
}

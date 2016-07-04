using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImageDis
{
    public abstract partial class ImageDisMiddlewareBase
    {
        private readonly ImageDisOptions _options;
        private readonly Regex _getImageRegex;

        public ImageDisMiddlewareBase(ImageDisOptions options, string pathPrefix)
        {
            _options = options;
            _getImageRegex = new Regex((pathPrefix ?? string.Empty) + @"/([A-Za-z0-9\.]+)", RegexOptions.IgnoreCase);
        }

        public async Task<bool> Process(object context)
        {
            var invoked = false;

            if (IsImagePost(context))
            {
                await SaveImage(context);
                invoked = true;
            }
            else if (IsImageGet(context))
            {
                await GetImage(context);
                invoked = true;
            }

            return invoked;
        }

        #region Interface
        
        protected abstract ImageDisRequest GetRequest(object context);

        protected abstract IEnumerable<ImageDisFile> GetFiles(object context);

        protected abstract void RespondWithStatusCode(object context, HttpStatusCode statusCode);

        protected abstract void RespondWithRedirect(object context, string path);

        protected abstract Task RespondWithJson(object context, object obj);

        #endregion
    }
}

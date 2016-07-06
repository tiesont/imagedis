using Microsoft.Owin;
using System.Threading.Tasks;

namespace ImageDis.Owin
{
    class ImageDisMiddleware : OwinMiddleware
    {
        private readonly ImageDisMiddlewareImpl _impl;

        public ImageDisMiddleware(OwinMiddleware next, ImageDisOptions options)
            : base(next)
        {
            _impl = new ImageDisMiddlewareImpl(this, options);
        }

        public async override Task Invoke(IOwinContext context)
        {
            var processed = await _impl.Process(context);
            if (!processed)
                await Next.Invoke(context);
        }
    }
}

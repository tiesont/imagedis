using Owin;

namespace ImageDis
{
    public static class ImageDisExtensions
    {
        public static void UseImageDis(this IAppBuilder app, ImageDisOptions options)
        {
            app.Use<ImageDisMiddleware>(options);
        }
    }
}

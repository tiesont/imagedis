using Owin;

namespace ImageDis.Owin
{
    public static class ImageDisBuilder
    {
        public static void UseImageDis(this IAppBuilder app, ImageDisOptions options)
        {
            app.Use<ImageDisMiddleware>(options);
        }
    }
}

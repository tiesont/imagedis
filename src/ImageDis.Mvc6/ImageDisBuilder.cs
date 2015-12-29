using Microsoft.AspNet.Builder;

namespace ImageDis.Mvc6
{
    public static class ImageDisBuilder
    {
        public static void UseImageDis(this IApplicationBuilder app, ImageDisOptions options)
        {
            app.Map(options.Path, subApp => subApp.RunImageDis(options));
        }

        public static void RunImageDis(this IApplicationBuilder builder, ImageDisOptions options)
        {
            builder.UseMiddleware<ImageDisMiddleware>(options);
        }
    }
}

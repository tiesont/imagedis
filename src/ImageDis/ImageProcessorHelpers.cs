using ImageProcessor;
using ImageProcessor.Imaging;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace ImageDis
{
    internal static class ImageProcessorHelpers
    {
        public static Task<ImageSize> GetSize(Stream image)
        {
            using (var imageFactory = new ImageFactory(preserveExifData: false))
            {
                var chain = imageFactory.Load(image);
                return Task.FromResult(
                    new ImageSize
                    {
                        Width = chain.Image.Size.Width,
                        Height = chain.Image.Size.Height
                    }
                );
            }
        }

        public static Task<Stream> TransformImage(Stream image, ImageDisParameters param)
        {
            var newImage = new MemoryStream();

            using (var imageFactory = new ImageFactory(preserveExifData: false))
            {
                var chain = imageFactory.Load(image);

                if (param.Has("w") || param.Has("h"))
                {
                    var resizeLayer = new ResizeLayer(new Size(param.Get<int>("w"), param.Get<int>("h")));
                    resizeLayer.ResizeMode = ResizeMode.Max;
                    resizeLayer.AnchorPosition = AnchorPosition.Center;

                    if (param.Has("pad") && param.Get<bool>("pad"))
                        resizeLayer.ResizeMode = ResizeMode.Pad;

                    chain.Resize(resizeLayer);
                }

                chain.Save(newImage);
            }

            return Task.FromResult<Stream>(newImage);
        }

        public static IEnumerable<string> GetParameterAsParts(ImageDisParameters param)
        {
            if (param.Has("w") && param.Has("h"))
                yield return param.Get<int>("w") + "x" + param.Get<int>("h");
            else if (param.Has("w"))
                yield return param.Get<int>("w") + "w";
            else if (param.Has("h"))
                yield return param.Get<int>("h") + "h";

            if (param.Has("pad") && param.Get<bool>("pad"))
                yield return "padded";
        }
    }
}

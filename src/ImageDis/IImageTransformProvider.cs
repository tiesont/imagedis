using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImageDis
{
    public interface IImageTransformProvider
    {
        Task<Stream> TransformImage(Stream image, ImageDisParameters param);

        IEnumerable<string> GetParameterAsParts(ImageDisParameters param);
    }
}

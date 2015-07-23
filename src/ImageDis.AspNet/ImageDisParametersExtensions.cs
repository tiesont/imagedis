using Microsoft.AspNet.Http;
using System.Linq;

namespace ImageDis.AspNet
{
    public static class ImageDisParametersExtensions
    {
        public static ImageDisParameters AppendParameters(this ImageDisParameters param, IReadableStringCollection query)
        {
            foreach (var q in query)
            {
                int integer;
                bool boolean;

                if (int.TryParse(q.Value.First(), out integer))
                    param.Add(q.Key, integer);
                else if (bool.TryParse(q.Value.First(), out boolean))
                    param.Add(q.Key, boolean);
                else
                    param.Add(q.Key, q.Value.First());
            }

            return param;
        }
    }
}

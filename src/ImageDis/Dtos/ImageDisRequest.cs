using System.Collections.Generic;

namespace ImageDis
{
    public class ImageDisRequest
    {
        public string Scheme { get; set; }
        public string Host { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public IDictionary<string, IEnumerable<string>> Query { get; set; }
    }
}

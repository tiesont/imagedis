using System;
using System.Collections.Generic;

namespace ImageDis
{
    public class ImageDisParameters
    {
        private readonly IDictionary<string, object> _dict;

        public ImageDisParameters()
        {
            _dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }
        
        public void Add<T>(string key, T value)
        {
            _dict.Add(key, value);
        }

        public bool Has(string key)
        {
            return _dict.ContainsKey(key) && _dict[key] != null;
        }

        public T Get<T>(string key)
        {
            if (!_dict.ContainsKey(key))
                return default(T);

            var value = _dict[key];

            if (value.GetType() != typeof(T))
                return default(T);

            return (T)value;
        }
    }
}

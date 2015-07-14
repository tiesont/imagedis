using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageDis
{
    public class ImageDisParameters
    {
        private readonly IDictionary<string, object> _dict;

        public ImageDisParameters(IReadableStringCollection param)
        {
            _dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in param)
            {
                int integer;
                bool boolean;

                if (int.TryParse(p.Value.First(), out integer))
                    _dict.Add(p.Key, integer);
                else if (bool.TryParse(p.Value.First(), out boolean))
                    _dict.Add(p.Key, boolean);
                else
                    _dict.Add(p.Key, p.Value.First());
            }
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

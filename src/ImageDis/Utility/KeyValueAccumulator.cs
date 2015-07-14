// The following code has been borrowed from the https://github.com/ASP-NET-MVC/aspnetwebstack repository.
//
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See https://github.com/ASP-NET-MVC/aspnetwebstack/blob/master/License.txt for license information.

using System.Collections.Generic;

namespace ImageDis.Utility
{
    public class KeyValueAccumulator<TKey, TValue>
    {
        private Dictionary<TKey, List<TValue>> _accumulator;
        IEqualityComparer<TKey> _comparer;

        public KeyValueAccumulator(IEqualityComparer<TKey> comparer)
        {
            _comparer = comparer;
            _accumulator = new Dictionary<TKey, List<TValue>>(comparer);
        }

        public void Append(TKey key, TValue value)
        {
            List<TValue> values;
            if (_accumulator.TryGetValue(key, out values))
            {
                values.Add(value);
            }
            else
            {
                _accumulator[key] = new List<TValue>(1) { value };
            }
        }

        public IDictionary<TKey, TValue[]> GetResults()
        {
            var results = new Dictionary<TKey, TValue[]>(_comparer);
            foreach (var kv in _accumulator)
            {
                results.Add(kv.Key, kv.Value.ToArray());
            }
            return results;
        }
    }
}

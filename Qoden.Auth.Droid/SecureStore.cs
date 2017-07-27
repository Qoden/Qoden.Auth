using System;
using System.Collections.Generic;
using Qoden.Util;

namespace Qoden.Auth
{
    public class SecureStore : ISecureStore
    {
        private Dictionary<string, object> _keys = new Dictionary<string, object>();

        public bool Delete(string key)
        {
            return _keys.Remove(key);
        }

        public T Get<T>(string key, T defaultValue)
        {
            return (T)_keys.GetValue(key, defaultValue);
        }

        public bool HasKey(string key)
        {
            return _keys.ContainsKey(key);
        }

        public void Set<T>(string key, T value)
        {
            _keys[key] = value;
        }
    }
}

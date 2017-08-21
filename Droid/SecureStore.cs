using System.IO;
using System.IO.IsolatedStorage;
using Java.Lang;
using Java.Security;
using Javax.Crypto;
using Exception = System.Exception;
using Qoden.Util;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Qoden.Auth
{

    //Based on https://github.com/XLabs/Xamarin-Forms-Labs/blob/master/src/Platform/XLabs.Platform.Droid/Services/KeyVaultStorage.cs
    public class SecureStore : ISecureStore
    {
        static IsolatedStorageFile File { get { return IsolatedStorageFile.GetUserStoreForApplication(); } }
        const string StorageFile = "Qoden.Auth.SecureStore";
        static readonly object SaveLock = new object();

        KeyStore _keyStore;
        KeyStore.PasswordProtection _protection;

        public SecureStore(char[] password)
        {
            _keyStore = KeyStore.GetInstance(KeyStore.DefaultType);
            _protection = new KeyStore.PasswordProtection(password);

            if (File.FileExists(StorageFile))
            {
                using (var stream = new IsolatedStorageFileStream(StorageFile, FileMode.Open, FileAccess.Read, File))
                {
                    _keyStore.Load(stream, password);
                }
            }
            else
            {
                _keyStore.Load(null, password);
            }
        }

        void Save()
        {
            lock (SaveLock)
            {
                using (var stream = new IsolatedStorageFileStream(StorageFile, FileMode.OpenOrCreate, FileAccess.Write, File))
                {
                    _keyStore.Store(stream, _protection.GetPassword());
                }
            }
        }

        public void Set<T>(string key, T value)
        {
            var jsonStr = JsonConvert.SerializeObject(value);
            var dataBytes = Encoding.UTF8.GetBytes(jsonStr);
            _keyStore.SetEntry(key, new KeyStore.SecretKeyEntry(new SecureData(dataBytes)), _protection);
            Save();
        }

        public bool Delete(string key)
        {
            lock (SaveLock)
            {
                var hasKey = HasKey(key);
                if (!hasKey) return false;
                _keyStore.DeleteEntry(key);
                Save();
            }
            return true;
        }

        public T Get<T>(string key, T defaultValue)
        {
            var entry = _keyStore.GetEntry(key, _protection) as KeyStore.SecretKeyEntry;
            if (entry == null)
            {
                return defaultValue;
            }
            var dataBytes = entry.SecretKey.GetEncoded();
            var str = Encoding.UTF8.GetString(dataBytes);
            return JsonConvert.DeserializeObject<T>(str);
        }

        public bool HasKey(string key)
        {
            return _keyStore.ContainsAlias(key);
        }

        class SecureData : Object, ISecretKey
        {
            const string Raw = "RAW";

            readonly byte[] data;

            public SecureData(byte[] dataBytes)
            {
                data = dataBytes;
            }

            #region IKey Members

            public string Algorithm
            {
                get { return Raw; }
            }

            public string Format
            {
                get { return Raw; }
            }

            public byte[] GetEncoded()
            {
                return data;
            }

            #endregion
        }
    }
}

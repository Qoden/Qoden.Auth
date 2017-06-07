using System;
using System.Collections.Generic;
using Qoden.Validation;

namespace Qoden.Auth
{
    /// <summary>
    /// Provide access to secure storage
    /// </summary>
    public interface ISecureStore
    {
        /// <summary>
        /// Read value from store
        /// </summary>
        /// <param name="key">Value key</param>
        /// <typeparam name="T">Value type</typeparam>
        T Get<T>(string key, T defaultValue);
        /// <summary>
        /// Write value to store
        /// </summary>
        /// <param name="key">Value key</param>
        /// <param name="value">Value to write</param>
        /// <typeparam name="T">Value type</typeparam>
        void Set<T>(string key, T value);

        /// <summary>
        /// Delete the specified key.
        /// </summary>
        /// <param name="key">Key.</param>
        bool Delete(string key);

        /// <summary>
        /// Check if store contains given key
        /// </summary>
        /// <param name="key">Key to check</param>
        bool HasKey(string key);
    }

    public static class SecureStorageExtensions
    {
        public static T Get<T>(this ISecureStore store, string key)
        {
            return store.Get(key, default(T));
        }
    }

    /// <summary>
    /// Indicate secure store error
    /// </summary>
    public class SecureStorageException : Exception
    {
        
    }
}

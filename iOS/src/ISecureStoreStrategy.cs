using Security;

namespace Qoden.Auth.iOS
{
    /// <summary>
    /// Strategy class defining how to work with iOS keychain.
    /// </summary>
    public interface ISecureStoreStrategy
    {
        /// <summary>
        /// Converts given key into keychain query record
        /// </summary>
        SecRecord KeyToQuery(string key);
        /// <summary>
        /// Reads data from keychain record
        /// </summary>
        T Read<T>(SecRecord record);
        /// <summary>
        /// Write data to keychain record
        /// </summary>
        /// <returns>The write.</returns>
        /// <param name="record">Record.</param>
        /// <param name="value">Value.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        void Write<T>(SecRecord record, T value);
    }

    public static class SecureStoreStrategyExtensions
    {
        /// <summary>
        /// Find the keychain item by given key.
        /// </summary>
        public static SecRecord Find(this ISecureStoreStrategy strategy, string key)
        {
            var query = strategy.KeyToQuery(key);
            SecStatusCode result;
            var record = SecKeyChain.QueryAsRecord(query, out result);
            AssertOk(result);
            return record;
        }

        /// <summary>
        /// Assert given status code does not represent failure condition (it is success or not found)
        /// </summary>
        public static void AssertOk(SecStatusCode code)
        {
            switch (code)
            {
                case SecStatusCode.Success:
                case SecStatusCode.ItemNotFound:
                    break;
                default:
                    throw new KeychainException(code);
            }
        }
    }
}

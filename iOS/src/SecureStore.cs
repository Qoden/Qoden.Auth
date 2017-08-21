using Qoden.Validation;
using Security;

namespace Qoden.Auth.iOS
{
    public class SecureStore : ISecureStore
    {
        private readonly ISecureStoreStrategy strategy;

        public SecureStore(ISecureStoreStrategy strategy)
        {
            Assert.Argument(strategy, nameof(strategy)).NotNull();
            this.strategy = strategy;
        }

        public SecureStore() : this(new DefaultSecureStoreStrategy())
        {
        }

        public T Get<T>(string key, T defaultValue)
        {
            var record = strategy.Find(key);
            if (record != null)
            {
                return strategy.Read<T>(record);
            }
            return defaultValue;
        }

        public bool HasKey(string key)
        {
            return strategy.Find(key) != null;
        }

        public void Set<T>(string key, T value)
        {
            SecStatusCode result;
            var query = strategy.KeyToQuery(key);
            var record = SecKeyChain.QueryAsRecord(query, out result);
            SecureStoreStrategyExtensions.AssertOk(result);
            if (record != null)
            {
                strategy.Write(record, value);
                result = SecKeyChain.Update(query, record);
                SecureStoreStrategyExtensions.AssertOk(result);
            }
            else
            {
                record = strategy.KeyToQuery(key);
                strategy.Write(record, value);
                result = SecKeyChain.Add(record);
                SecureStoreStrategyExtensions.AssertOk(result);
            }
        }

        public bool Delete(string key)
        {
            var query = strategy.KeyToQuery(key);
            var code = SecKeyChain.Remove(query);
            SecureStoreStrategyExtensions.AssertOk(code);
            return code != SecStatusCode.ItemNotFound;
        }
    }
}

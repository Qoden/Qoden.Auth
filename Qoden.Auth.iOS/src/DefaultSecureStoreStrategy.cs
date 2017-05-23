using Foundation;
using Newtonsoft.Json;
using Security;

namespace Qoden.Auth.iOS
{
    public class DefaultSecureStoreStrategy : ISecureStoreStrategy
    {
        public SecRecord KeyToQuery(string key)
        {
            var sr = new SecRecord(SecKind.GenericPassword);
            sr.Account = key;
            return sr;
        }

        public T Read<T>(SecRecord record)
        {
            return JsonConvert.DeserializeObject<T>(record.ValueData.ToString());
        }

        public void Write<T>(SecRecord record, T value)
        {
            record.ValueData = NSData.FromString(JsonConvert.SerializeObject(value));
        }
    }


}

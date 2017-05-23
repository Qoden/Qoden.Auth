using System;
using Security;

namespace Qoden.Auth.iOS
{
    public class KeychainException : SecureStorageException
    {
        public KeychainException(SecStatusCode code)
        {
            Code = code;
        }

        public SecStatusCode Code { get; private set; }
    }
}

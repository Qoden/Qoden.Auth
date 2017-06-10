using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Qoden.Auth
{
    using UserProfile = Dictionary<string, object>;

    /// <summary>
    /// User auth strategy which defines how to obtain user authorization and refresh it.
    /// </summary>
    public interface IAuthStrategy
    {
        /// <summary>
        /// Authorize user and return user information
        /// </summary>
        Task<UserProfile> Authorize();
        /// <summary>
        /// Authorize user authorization and return user information
        /// </summary>
        Task<UserProfile> Refresh(UserProfile savedProfile);
        /// <summary>
        /// Check if user authroization expired
        /// </summary>
        Task<bool> ProfileExpired(UserProfile savedProfile);
    }
}

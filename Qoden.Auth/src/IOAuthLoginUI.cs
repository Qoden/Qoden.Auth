using System;
using System.Threading.Tasks;
using Qoden.Util;

namespace Qoden.Auth
{
    /// <summary>
    /// OAuth login page.
    /// </summary>
    public interface IOAuthLoginUI
    {
        /// <summary>
        /// Display login page and return server response.
        /// </summary>
        /// <returns>Data returned by OAuth server.</returns>
        /// <param name="uri">Login page URI</param>
        Task<HttpValueCollection> Display(Uri uri);
    }
}

﻿using System;
using System.Threading.Tasks;
using Qoden.Util;

namespace Qoden.Auth
{
    /// <summary>
    /// Abstracts OAuth login pages used to get grant code.
    /// </summary>
    public interface IOAuthLoginUI
    {
        /// <summary>
        /// Display login page and get grant code
        /// </summary>
        /// <returns>OAuth grant code.</returns>
        /// <param name="uri">Login page URI</param>
        Task<HttpValueCollection> GetResult(Uri uri);
    }
}
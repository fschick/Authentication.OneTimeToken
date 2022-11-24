﻿using FS.Authentication.OneTimeToken.Abstractions.Models;
using System;

namespace FS.Authentication.OneTimeToken.Abstractions.Interfaces
{
    /// <summary>
    /// Interface for one time token authentication service.
    /// </summary>
    /// <autogeneratedoc />
    public interface IOneTimeTokenService
    {
        /// <summary>
        /// Creates a one-time token.
        /// </summary>
        /// <param name="roles">Comma delimited list of roles that are allowed to access the resource.</param>
        string CreateToken(params string[] roles);

        /// <summary>
        /// Creates a one-time token.
        /// </summary>
        /// <param name="expiresIn">The expires in.</param>
        /// <param name="roles">Comma delimited list of roles that are allowed to access the resource.</param>
        string CreateToken(TimeSpan? expiresIn = null, params string[] roles);

        /// <summary>
        /// Validates the token.
        /// </summary>
        /// <param name="token">The token.</param>
        TokenValidationResult ValidateToken(string token);
    }
}
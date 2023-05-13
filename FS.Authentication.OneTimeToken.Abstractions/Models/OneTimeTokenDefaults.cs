using System;
using System.Diagnostics.CodeAnalysis;

namespace FS.Authentication.OneTimeToken.Abstractions.Models
{
    /// <summary>
    /// Default values for one-time access token authentication.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class OneTimeTokenDefaults
    {
        /// <summary>
        /// Default ont-time token authentication scheme name
        /// </summary>
        public const string AuthenticationScheme = "OneTimeToken";

        /// <summary>
        /// Default value for 'OneTimeTokenOptions.AuthorizationHeaderName'.
        /// </summary>
        public const string AuthorizationHeaderName = "Authorization";

        /// <summary>
        /// Default value for 'OneTimeTokenOptions.AuthorizationHeaderName'.
        /// </summary>
        public const string NameIdentifier = "One time access token";

        /// <summary>
        /// Default value for 'OneTimeTokenOptions.AuthorizationHeaderPrefix'.
        /// </summary>
        public const string AuthorizationHeaderPrefix = "OneTime";

        /// <summary>
        /// Default value for 'OneTimeTokenOptions.AuthorizationQueryParamName'.
        /// </summary>
        public const string AuthorizationQueryParamName = "accessToken";

        /// <summary>
        /// Default value for 'OneTimeTokenOptions.DefaultExpireTime'.
        /// </summary>
        public static readonly TimeSpan DefaultExpireTime = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Default value for 'OneTimeTokenOptions.Now'.
        /// </summary>
        public static readonly Func<DateTime> Now = () => DateTime.UtcNow;
    }
}
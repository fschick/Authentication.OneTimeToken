using FS.Authentication.OneTimeToken.Abstractions.Models;
using Microsoft.AspNetCore.Authentication;
using System;

namespace FS.Authentication.OneTimeToken.Models;

/// <summary>
/// Options to configure the one-time access token authentication scheme.
/// </summary>
public class OneTimeTokenOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// The key of the authentication header to check for. Default is 'Authorization'.
    /// </summary>
    public string AuthorizationHeaderName { get; set; } = OneTimeTokenDefaults.AuthorizationHeaderName;

    /// <summary>
    /// The name identifier used to build the claims principal. Default is 'One time access token'.
    /// </summary>
    public string NameIdentifier { get; set; } = OneTimeTokenDefaults.NameIdentifier;

    /// <summary>
    /// The prefix used to identify a one-time access token in the authentication header value. Default is 'OneTimeToken'.
    /// </summary>
    public string AuthorizationHeaderPrefix { get; set; } = OneTimeTokenDefaults.AuthorizationHeaderPrefix;

    /// <summary>
    /// Name of the query parameter to retrieve the access token from (as an alternative to request header). Default is 'accessToken'.
    /// </summary>
    public string AuthorizationQueryParamName { get; set; } = OneTimeTokenDefaults.AuthorizationQueryParamName;

    /// <summary>
    /// Default time span until a access token expires. Default is 30 minutes.
    /// </summary>
    public TimeSpan DefaultExpireTime { get; set; } = OneTimeTokenDefaults.DefaultExpireTime;

    /// <summary>
    /// A delegate returning the current date/time. Default is '() => DateTime.UtcNow'.
    /// </summary>
    public Func<DateTime> Now { get; set; } = OneTimeTokenDefaults.Now;
}
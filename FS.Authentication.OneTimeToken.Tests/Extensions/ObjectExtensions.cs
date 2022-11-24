using Newtonsoft.Json;

namespace FS.Authentication.OneTimeToken.Tests.Extensions;

internal static class ObjectExtensions
{
    public static T Clone<T>(this T obj)
        => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
}
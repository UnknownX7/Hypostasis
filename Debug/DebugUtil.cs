namespace Hypostasis.Debug;

public static class DebugUtil
{
    public static T LogDebug<T>(this T o, string format = null)
    {
        DalamudApi.LogDebug(GetString(o, format));
        return o;
    }

    public static T LogInfo<T>(this T o, string format = null)
    {
        DalamudApi.LogInfo(GetString(o, format));
        return o;
    }

    public static T LogError<T>(this T o, string format = null)
    {
        DalamudApi.LogError(GetString(o, format));
        return o;
    }

    private static string GetString(object o, string format) => string.IsNullOrEmpty(format)
        ? o.ToString()
        : (string)(o.GetType().GetMethod(nameof(ToString), new[] { typeof(string) })?.Invoke(o, new object[] { format }) ?? o.ToString());
}
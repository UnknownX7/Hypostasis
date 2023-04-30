namespace Hypostasis.Debug;

public static unsafe class DebugUtil
{
    public static T LogDebug<T>(this T o, string format = null)
    {
        DalamudApi.LogDebug(GetString(o, format));
        return o;
    }

    public static T* LogDebug<T>(T* o) where T : unmanaged
    {
        DalamudApi.LogDebug($"{(nint)o:X}");
        return o;
    }

    public static T LogInfo<T>(this T o, string format = null)
    {
        DalamudApi.LogInfo(GetString(o, format));
        return o;
    }

    public static T* LogInfo<T>(T* o) where T : unmanaged
    {
        DalamudApi.LogInfo($"{(nint)o:X}");
        return o;
    }

    public static T LogError<T>(this T o, string format = null)
    {
        DalamudApi.LogError(GetString(o, format));
        return o;
    }

    public static T* LogError<T>(T* o) where T : unmanaged
    {
        DalamudApi.LogError($"{(nint)o:X}");
        return o;
    }

    private static string GetString(object o, string format) => string.IsNullOrEmpty(format)
        ? o.ToString()
        : (string)(o.GetType().GetMethod(nameof(ToString), new[] { typeof(string) })?.Invoke(o, new object[] { format }) ?? o.ToString());
}
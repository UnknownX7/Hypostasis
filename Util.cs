using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Logging;

namespace Hypostasis;

public static partial class Util
{
    public class AssignableInfo
    {
        public object Object { get; init; }
        public MemberInfo MemberInfo { get; init; }
        private readonly FieldInfo fieldInfo;
        private readonly PropertyInfo propertyInfo;

        public string Name => fieldInfo?.Name ?? propertyInfo?.Name ?? string.Empty;
        public Type Type => fieldInfo?.FieldType ?? propertyInfo?.PropertyType;

        public AssignableInfo(object o, MemberInfo info)
        {
            Object = o;
            MemberInfo = info;
            fieldInfo = info as FieldInfo;
            propertyInfo = info as PropertyInfo;
        }

        public object GetValue() => fieldInfo?.GetValue(Object) ?? propertyInfo?.GetValue(Object);

        public void SetValue(object v)
        {
            fieldInfo?.SetValue(Object, v);
            propertyInfo?.SetValue(Object, v);
        }
    }

    [LibraryImport("user32.dll")]
    private static partial nint GetForegroundWindow();

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial int GetWindowThreadProcessId(nint handle, out int processId);

    public static bool IsWindowFocused
    {
        get
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == nint.Zero)
                return false;

            var procId = Environment.ProcessId;
            _ = GetWindowThreadProcessId(activatedHandle, out var activeProcId);

            return activeProcId == procId;
        }
    }

    public static Assembly Assembly => Assembly.GetExecutingAssembly();

    public static Type[] AssemblyTypes => Assembly.GetTypes();

    public static AssemblyName AssemblyName => Assembly.GetName();

    public static IEnumerable<Type> GetTypes<T>(this Assembly assembly) => typeof(T).IsInterface
        ? assembly.GetTypes().Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(T)))
        : assembly.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(T)));

    public static IEnumerable<(Type, T)> GetTypesWithAttribute<T>(this Assembly assembly) where T : Attribute =>
        from t in assembly.GetTypes() let attribute = t.GetCustomAttribute<T>() where attribute != null select (t, attribute);

    public static bool StartProcess(ProcessStartInfo startInfo)
    {
        try
        {
            Process.Start(startInfo);
            return true;
        }
        catch (Exception e)
        {
            PluginLog.Error(e, "Failed to start process!");
            return false;
        }
    }

    public static bool StartProcess(string process, bool admin = false)
    {
        return StartProcess(new ProcessStartInfo
        {
            FileName = process,
            UseShellExecute = true,
            Verb = admin ? "runas" : string.Empty
        });
    }

    public static string CompressString(string s, string prefix = "")
    {
        var bytes = Encoding.UTF8.GetBytes(s);
        using var ms = new MemoryStream();
        using (var gs = new GZipStream(ms, CompressionMode.Compress))
            gs.Write(bytes, 0, bytes.Length);
        return prefix + Convert.ToBase64String(ms.ToArray());
    }

    public static string DecompressString(string s, string prefix = "")
    {
        if (!s.StartsWith(prefix))
            throw new ApplicationException("This export is for a different plugin.");
        var data = Convert.FromBase64String(s[prefix.Length..]);
        using var ms = new MemoryStream(data);
        using var gs = new GZipStream(ms, CompressionMode.Decompress);
        using var r = new StreamReader(gs);
        return r.ReadToEnd();
    }

    public static unsafe nint ConvertObjectToIntPtr(object o) => o switch
    {
        Pointer p => (nint)Pointer.Unbox(p),
        nint p => p,
        nuint p => (nint)p,
        { } when o.IsNumeric() => (nint)Convert.ToInt64(o),
        _ => nint.Zero
    };

    public static bool IsNumeric(this object o) => o switch
    {
        //Int128 => true, UInt128 => true,
        //nint => true, nuint => true,
        long => true, ulong => true,
        int => true, uint => true,
        short => true, ushort => true,
        sbyte => true, byte => true,
        double => true, float => true, decimal => true,
        _ => false
    };

    public static bool IsValidHookAddress(this nint address) => DalamudApi.SigScanner.IsValidHookAddress(address);

    public static object Cast(this Type type, object data)
    {
        var dataParam = Expression.Parameter(typeof(object), "data");
        var body = Expression.Block(Expression.Convert(Expression.Convert(dataParam, data.GetType()), type));
        return Expression.Lambda(body, dataParam).Compile().DynamicInvoke(data);
    }

    public static Vector2 Rotate(this Vector2 v, float a)
    {
        var aCos = (float)Math.Cos(a);
        var aSin = (float)Math.Sin(a);
        return v.Rotate(aCos, aSin);
    }

    public static Vector2 Rotate(this Vector2 v, float aCos, float aSin) => new(v.X * aCos - v.Y * aSin, v.X * aSin + v.Y * aCos);
}
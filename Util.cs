using System.Linq.Expressions;
using System;
using System.Reflection;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Dalamud.Logging;

namespace Hypostasis;

public static class Util
{
    public class AssignableInfo
    {
        public readonly object obj;
        public readonly MemberInfo memberInfo;
        private readonly FieldInfo fieldInfo;
        private readonly PropertyInfo propertyInfo;

        public string Name => fieldInfo?.Name ?? propertyInfo?.Name ?? string.Empty;
        public Type Type => fieldInfo?.FieldType ?? propertyInfo?.PropertyType;

        public AssignableInfo(object o, MemberInfo info)
        {
            obj = o;
            memberInfo = info;
            fieldInfo = info as FieldInfo;
            propertyInfo = info as PropertyInfo;
        }

        public object GetValue() => fieldInfo?.GetValue(obj) ?? propertyInfo?.GetValue(obj);

        public void SetValue(object v)
        {
            fieldInfo?.SetValue(obj, v);
            propertyInfo?.SetValue(obj, v);
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)] private static extern nint GetForegroundWindow();
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)] private static extern int GetWindowThreadProcessId(nint handle, out int processId);
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
        var data = Convert.FromBase64String(s);
        using var ms = new MemoryStream(data);
        using var gs = new GZipStream(ms, CompressionMode.Decompress);
        using var r = new StreamReader(gs);
        return r.ReadToEnd();
    }

    public static object Cast(this Type type, object data)
    {
        var dataParam = Expression.Parameter(typeof(object), "data");
        var body = Expression.Block(Expression.Convert(Expression.Convert(dataParam, data.GetType()), type));
        return Expression.Lambda(body, dataParam).Compile().DynamicInvoke(data);
    }
}
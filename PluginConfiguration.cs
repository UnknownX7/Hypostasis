using System;
using System.IO;
using Dalamud.Configuration;
using Dalamud.Logging;

namespace Hypostasis;

public abstract class PluginConfiguration<T> where T : PluginConfiguration<T>, IPluginConfiguration, new()
{
    public static DirectoryInfo ConfigFolder => DalamudApi.PluginInterface.ConfigDirectory;
    public static FileInfo ConfigFile => DalamudApi.PluginInterface.ConfigFile;

    public abstract int Version { get; set; }

    public abstract void Initialize();

    public void Save() => DalamudApi.PluginInterface.SavePluginConfig(this as T);

    private static T ResetConfig()
    {
        ConfigFile.MoveTo(ConfigFile.FullName + ".CORRUPT", true);
        return new T();
    }

    public static T LoadConfig()
    {
        try
        {
            return DalamudApi.PluginInterface.GetPluginConfig() as T ?? new T();
        }
        catch (Exception e)
        {
            PluginLog.Error(e, "Error loading config! Renaming old file and resetting...");
            return ResetConfig();
        }
    }
}
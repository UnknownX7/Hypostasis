using System;
using System.IO;
using Dalamud.Configuration;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Logging;

namespace Hypostasis.Dalamud;

public abstract class PluginConfiguration<T> where T : PluginConfiguration<T>, IPluginConfiguration, new()
{
    public static DirectoryInfo ConfigFolder => DalamudApi.PluginInterface.ConfigDirectory;
    public static FileInfo ConfigFile => DalamudApi.PluginInterface.ConfigFile;

    public virtual int Version { get; set; }
    public Version PluginVersion;

    public virtual void Initialize() { }

    public static T LoadConfig()
    {
        T config;

        try
        {
            config = DalamudApi.PluginInterface.GetPluginConfig() as T ?? new T();
        }
        catch (Exception e)
        {
            const string message = "Error loading config! Renaming old file and resetting...";
            DalamudApi.PluginInterface.UiBuilder.AddNotification(message, Hypostasis.PluginName, NotificationType.Error, 10_000);
            PluginLog.Error(e, message);
            config = ResetConfig();
        }

        config.Initialize();
        config.UpdateVersion();

        return config;
    }

    public void Save()
    {
        PluginModuleManager.CheckModules();
        DalamudApi.PluginInterface.SavePluginConfig(this as T);
    }

    private static T ResetConfig()
    {
        ConfigFile.MoveTo(ConfigFile.FullName + ".CORRUPT", true);
        return new T();
    }

    private void UpdateVersion()
    {
        var pluginVersion = Util.AssemblyName.Version;
        if (PluginVersion == pluginVersion) return;

        var prevVersion = PluginVersion;
        PluginVersion = pluginVersion;

        if (prevVersion < pluginVersion)
            OnUpdate(prevVersion);
    }

    protected virtual void OnUpdate(Version previousVersion) { }
}
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.GuruAudioEnhancer.Configuration;

public sealed class PluginConfiguration : BasePluginConfiguration
{
    public string FfmpegPath { get; set; } = "/usr/lib/jellyfin-ffmpeg/ffmpeg";
    public bool ProcessMovies { get; set; } = true;
    public bool ProcessEpisodes { get; set; } = true;
    public bool DialogueBoostEnabled { get; set; } = true;
    public bool NightModeEnabled { get; set; } = true;
    public bool StrongDialogueEnabled { get; set; } = false;
    public int AacBitrateKbps { get; set; } = 192;
    public bool OverwriteExisting { get; set; } = false;
    public bool SkipNonLocalMedia { get; set; } = true;

    // Safety feature for tuning: when enabled, process only one explicitly selected item.
    public bool TestMode { get; set; } = true;
    public string TestItemId { get; set; } = string.Empty;
    public string TestItemName { get; set; } = string.Empty;

    // Retained for migration from v0.1.1. If TestItemId is empty the service can still
    // fall back to the old title-match behaviour so an existing config remains safe.
    public string TestTitle { get; set; } = string.Empty;
}

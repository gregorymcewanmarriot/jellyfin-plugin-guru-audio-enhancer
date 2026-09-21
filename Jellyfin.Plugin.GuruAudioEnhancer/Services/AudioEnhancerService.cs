using System.Diagnostics;
using Jellyfin.Plugin.GuruAudioEnhancer.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Jellyfin.Data.Enums;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.GuruAudioEnhancer.Services;

public sealed class AudioEnhancerService
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<AudioEnhancerService> _logger;

    public AudioEnhancerService(ILibraryManager libraryManager, ILogger<AudioEnhancerService> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
    }

    public async Task GenerateAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();
        ValidateFfmpeg(config.FfmpegPath);

        var kinds = new List<BaseItemKind>();
        if (config.ProcessMovies) kinds.Add(BaseItemKind.Movie);
        if (config.ProcessEpisodes) kinds.Add(BaseItemKind.Episode);

        if (kinds.Count == 0)
        {
            _logger.LogInformation("Guru Audio Enhancer: no media types are enabled.");
            progress.Report(100);
            return;
        }

        var items = _libraryManager.GetItemList(new InternalItemsQuery
        {
            IncludeItemTypes = kinds.ToArray(),
            Recursive = true,
            IsVirtualItem = false
        })
        .Where(i => !string.IsNullOrWhiteSpace(i.Path))
        .ToList();

        if (config.TestMode)
        {
            items = FilterToTestItem(items, config, "generation");
            if (items.Count == 0)
            {
                progress.Report(100);
                return;
            }

            _logger.LogInformation("Guru Audio Enhancer TEST MODE: processing only '{Name}' at {Path}", items[0].Name, items[0].Path);
        }

        var presets = GetEnabledPresets(config).ToList();
        if (presets.Count == 0)
        {
            _logger.LogInformation("Guru Audio Enhancer: no presets are enabled.");
            progress.Report(100);
            return;
        }

        var totalJobs = Math.Max(1, items.Count * presets.Count);
        var completedJobs = 0;

        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var inputPath = item.Path!;

            if (!File.Exists(inputPath))
            {
                _logger.LogDebug("Skipping unavailable path {Path}", inputPath);
                completedJobs += presets.Count;
                continue;
            }

            if (config.SkipNonLocalMedia && Uri.TryCreate(inputPath, UriKind.Absolute, out var uri) && !uri.IsFile)
            {
                completedJobs += presets.Count;
                continue;
            }

            foreach (var preset in presets)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var outputPath = BuildOutputPath(inputPath, preset.DisplayName);

                if (File.Exists(outputPath) && !config.OverwriteExisting)
                {
                    _logger.LogDebug("Enhanced track exists, skipping: {Path}", outputPath);
                }
                else
                {
                    await GenerateTrackAsync(config, inputPath, outputPath, preset, cancellationToken).ConfigureAwait(false);
                }

                completedJobs++;
                progress.Report(completedJobs * 100d / totalJobs);
            }
        }

        progress.Report(100);
    }

    public Task CleanupAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();
        var kinds = new List<BaseItemKind>();
        if (config.ProcessMovies) kinds.Add(BaseItemKind.Movie);
        if (config.ProcessEpisodes) kinds.Add(BaseItemKind.Episode);

        if (kinds.Count == 0)
        {
            progress.Report(100);
            return Task.CompletedTask;
        }

        var items = _libraryManager.GetItemList(new InternalItemsQuery
        {
            IncludeItemTypes = kinds.ToArray(),
            Recursive = true,
            IsVirtualItem = false
        })
        .Where(i => !string.IsNullOrWhiteSpace(i.Path))
        .ToList();

        if (config.TestMode)
        {
            items = FilterToTestItem(items, config, "cleanup");
            if (items.Count == 0)
            {
                progress.Report(100);
                return Task.CompletedTask;
            }
        }

        var suffixes = new[] { "Dialogue Boost", "Night Mode", "Strong Dialogue" };
        var done = 0;
        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var suffix in suffixes)
            {
                var path = BuildOutputPath(item.Path!, suffix);
                try
                {
                    if (File.Exists(path)) File.Delete(path);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not delete enhanced track {Path}", path);
                }
            }

            done++;
            progress.Report(items.Count == 0 ? 100 : done * 100d / items.Count);
        }

        progress.Report(100);
        return Task.CompletedTask;
    }


    private List<BaseItem> FilterToTestItem(List<BaseItem> items, PluginConfiguration config, string operation)
    {
        if (!string.IsNullOrWhiteSpace(config.TestItemId) && Guid.TryParse(config.TestItemId, out var selectedId))
        {
            var exact = items.Where(i => i.Id == selectedId).Take(1).ToList();
            if (exact.Count == 0)
            {
                _logger.LogWarning(
                    "Guru Audio Enhancer: Test Mode {Operation} could not find selected Jellyfin item id {ItemId}. Nothing will be changed.",
                    operation,
                    config.TestItemId);
            }

            return exact;
        }

        // Backwards-compatible v0.1.1 fallback. This is intentionally only used when
        // no exact item id has been selected in the v0.1.2 configuration page.
        if (!string.IsNullOrWhiteSpace(config.TestTitle))
        {
            var legacy = items
                .Where(i => !string.IsNullOrWhiteSpace(i.Name) && i.Name.Contains(config.TestTitle, StringComparison.OrdinalIgnoreCase))
                .Take(1)
                .ToList();

            if (legacy.Count == 0)
            {
                _logger.LogWarning(
                    "Guru Audio Enhancer: Test Mode {Operation} could not find a legacy title containing '{TestTitle}'. Nothing will be changed.",
                    operation,
                    config.TestTitle);
            }

            return legacy;
        }

        _logger.LogWarning(
            "Guru Audio Enhancer: Test Mode {Operation} was requested without selecting a Jellyfin item. Nothing will be changed.",
            operation);
        return new List<BaseItem>();
    }

    private async Task GenerateTrackAsync(
        PluginConfiguration config,
        string inputPath,
        string outputPath,
        AudioPreset preset,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        var tempPath = outputPath + ".tmp.aac";
        if (File.Exists(tempPath)) File.Delete(tempPath);

        // -map 0:a:0 intentionally uses the first audio stream for v1.
        // The pan stage folds multichannel content to stereo while strongly preserving the center/dialogue channel.
        var channels = await ProbeChannelCountAsync(config.FfmpegPath, inputPath, cancellationToken).ConfigureAwait(false);
        var filterGraph = BuildFilterGraph(preset.Key, channels);

        var args = string.Join(' ', new[]
        {
            "-hide_banner", "-nostdin", "-y",
            "-i", Quote(inputPath),
            "-map", "0:a:0",
            "-vn", "-sn", "-dn",
            "-af", Quote(filterGraph),
            "-c:a", "aac",
            "-b:a", $"{Math.Clamp(config.AacBitrateKbps, 96, 320)}k",
            "-metadata", Quote($"title={preset.DisplayName}"),
            "-f", "adts",
            Quote(tempPath)
        });

        _logger.LogInformation("Generating {Preset}: {Input}", preset.DisplayName, inputPath);

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = config.FfmpegPath,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            }
        };

        process.Start();
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        var stderr = await stderrTask.ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            TryDelete(tempPath);
            throw new InvalidOperationException($"FFmpeg failed for '{inputPath}' ({preset.DisplayName}), exit {process.ExitCode}: {Tail(stderr, 2000)}");
        }

        File.Move(tempPath, outputPath, true);
        _logger.LogInformation("Created enhanced track {Output}", outputPath);
    }

    private static IEnumerable<AudioPreset> GetEnabledPresets(PluginConfiguration config)
    {
        if (config.DialogueBoostEnabled)
            yield return new AudioPreset("dialogue", "Dialogue Boost", string.Empty);
        if (config.NightModeEnabled)
            yield return new AudioPreset("night", "Night Mode", string.Empty);
        if (config.StrongDialogueEnabled)
            yield return new AudioPreset("strong", "Strong Dialogue", string.Empty);
    }

    private static string BuildFilterGraph(string presetKey, int channels)
    {
        string downmix;
        if (channels >= 8)
        {
            downmix = presetKey switch
            {
                "strong" => "pan=stereo|FL=0.58*FL+0.92*FC+0.18*LFE+0.24*SL+0.18*BL|FR=0.58*FR+0.92*FC+0.18*LFE+0.24*SR+0.18*BR",
                "night" => "pan=stereo|FL=0.68*FL+0.82*FC+0.16*LFE+0.28*SL+0.20*BL|FR=0.68*FR+0.82*FC+0.16*LFE+0.28*SR+0.20*BR",
                _ => "pan=stereo|FL=0.78*FL+0.74*FC+0.18*LFE+0.32*SL+0.22*BL|FR=0.78*FR+0.74*FC+0.18*LFE+0.32*SR+0.22*BR"
            };
        }
        else if (channels >= 6)
        {
            downmix = presetKey switch
            {
                "strong" => "pan=stereo|FL=0.58*FL+0.92*FC+0.18*LFE+0.28*SL|FR=0.58*FR+0.92*FC+0.18*LFE+0.28*SR",
                "night" => "pan=stereo|FL=0.68*FL+0.82*FC+0.16*LFE+0.32*SL|FR=0.68*FR+0.82*FC+0.16*LFE+0.32*SR",
                _ => "pan=stereo|FL=0.78*FL+0.74*FC+0.18*LFE+0.36*SL|FR=0.78*FR+0.74*FC+0.18*LFE+0.36*SR"
            };
        }
        else
        {
            // Stereo and unusual layouts use FFmpeg's normal layout conversion first.
            downmix = "aformat=channel_layouts=stereo";
        }

        var processing = presetKey switch
        {
            "strong" => "highpass=f=120,equalizer=f=1800:t=q:w=1.0:g=3,equalizer=f=3200:t=q:w=1.0:g=2.5,acompressor=threshold=0.065:ratio=5:attack=5:release=300:makeup=1.8,alimiter=limit=0.90",
            "night" => "highpass=f=100,equalizer=f=2200:t=q:w=1.1:g=3,acompressor=threshold=0.08:ratio=4:attack=8:release=250:makeup=1.6,alimiter=limit=0.92",
            _ => "highpass=f=90,equalizer=f=2200:t=q:w=1.2:g=2.5,acompressor=threshold=0.12:ratio=2.2:attack=15:release=180:makeup=1.35,alimiter=limit=0.95"
        };

        return downmix + "," + processing;
    }

    private async Task<int> ProbeChannelCountAsync(string ffmpegPath, string inputPath, CancellationToken cancellationToken)
    {
        var ffprobePath = Path.Combine(Path.GetDirectoryName(ffmpegPath) ?? string.Empty, "ffprobe");
        if (!File.Exists(ffprobePath))
        {
            _logger.LogWarning("ffprobe not found beside FFmpeg; using safe stereo conversion for {Path}", inputPath);
            return 2;
        }

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ffprobePath,
                Arguments = $"-v error -select_streams a:0 -show_entries stream=channels -of csv=p=0 {Quote(inputPath)}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.Start();
        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        var output = (await outputTask.ConfigureAwait(false)).Trim();

        return process.ExitCode == 0 && int.TryParse(output, out var channels) && channels > 0 ? channels : 2;
    }

    private static string BuildOutputPath(string inputPath, string displayName)
    {
        var dir = Path.GetDirectoryName(inputPath) ?? string.Empty;
        var baseName = Path.GetFileNameWithoutExtension(inputPath);
        return Path.Combine(dir, $"{baseName}.{displayName}.eng.aac");
    }

    private static void ValidateFfmpeg(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            throw new FileNotFoundException("FFmpeg was not found. Set the correct Jellyfin FFmpeg path in Guru Audio Enhancer settings.", path);
    }

    private static string Quote(string value) => '"' + value.Replace("\"", "\\\"") + '"';

    private static string Tail(string value, int max) => value.Length <= max ? value : value[^max..];

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }
}

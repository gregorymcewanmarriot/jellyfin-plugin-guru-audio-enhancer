using Jellyfin.Plugin.GuruAudioEnhancer.Services;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.GuruAudioEnhancer.ScheduledTasks;

public sealed class GenerateEnhancedAudioTask : IScheduledTask, IConfigurableScheduledTask
{
    private readonly AudioEnhancerService _service;

    public GenerateEnhancedAudioTask(AudioEnhancerService service) => _service = service;

    public string Name => "Generate enhanced dialogue audio";
    public string Key => "GuruAudioEnhancerGenerate";
    public string Description => "Creates external Dialogue Boost / Night Mode audio tracks for Movies and Episodes.";
    public string Category => "Guru Audio Enhancer";
    public bool IsHidden => false;
    public bool IsEnabled => true;
    public bool IsLogged => true;

    public Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        => _service.GenerateAsync(progress, cancellationToken);

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() => Array.Empty<TaskTriggerInfo>();
}

using Jellyfin.Plugin.GuruAudioEnhancer.Services;
using Jellyfin.Plugin.GuruAudioEnhancer.ScheduledTasks;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.GuruAudioEnhancer;

public sealed class PluginServiceRegistrator : IPluginServiceRegistrator
{
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<AudioEnhancerService>();
        serviceCollection.AddSingleton<IScheduledTask, GenerateEnhancedAudioTask>();
        serviceCollection.AddSingleton<IScheduledTask, CleanupEnhancedAudioTask>();
    }
}

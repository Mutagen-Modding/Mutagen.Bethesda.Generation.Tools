using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins;

namespace Mutagen.Bethesda.Generation.Tools.Common;

public class EnvironmentProvider : IDisposable
{
    private readonly IGameReleaseContext _gameReleaseContext;
    private readonly ISourceFileProvider _sourceFileProvider;
    private Lazy<IGameEnvironment> _environment;
    public IGameEnvironment Environment => _environment.Value;
    
    public EnvironmentProvider(
        IGameReleaseContext gameReleaseContext,
        ISourceFileProvider sourceFileProvider)
    {
        _gameReleaseContext = gameReleaseContext;
        _sourceFileProvider = sourceFileProvider;
        _environment = new Lazy<IGameEnvironment>(GetEnvironment);
    }

    private IGameEnvironment GetEnvironment()
    {
        if (_sourceFileProvider.SourceFile == ModPath.Empty)
        {
            return GameEnvironment.Typical.Builder(_gameReleaseContext.Release)
                .TransformModListings(x => x.OnlyEnabledAndExisting())
                .Build();
        }
        else
        {
            return GameEnvironment.Typical.Builder(_gameReleaseContext.Release)
                .WithLoadOrder(_sourceFileProvider.SourceFile.ModKey)
                .WithTargetDataFolder(_sourceFileProvider.SourceFile.Path.Directory!.Value)
                .Build();
        }
    }

    public void Dispose()
    {
        if (_environment.IsValueCreated)
        {
            _environment.Value.Dispose();
        }
    }
}
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins;

namespace Mutagen.Bethesda.Generation.Tools;

public static class Utility
{
    public static IGameEnvironment GetGameEnvironmentState(GameRelease release, ModPath? singleModSourcePath)
    {
        if (singleModSourcePath == null || singleModSourcePath == ModPath.Empty)
        {
            return GameEnvironment.Typical.Builder(release)
                .TransformModListings(x => x.OnlyEnabledAndExisting())
                .Build();
        }
        else
        {
            return GameEnvironment.Typical.Builder(release)
                .WithLoadOrder(singleModSourcePath.ModKey)
                .WithTargetDataFolder(singleModSourcePath.Path.Directory!.Value)
                .Build();
        }
    }
}
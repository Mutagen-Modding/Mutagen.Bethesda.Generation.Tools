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
    
    public static IGameEnvironment GetGameEnvironmentStateWithSeparateSupport(GameRelease release, ModPath? singleModSourcePath)
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
                .WithTargetDataFolder(singleModSourcePath.Path.Directory!.Value)
                .Build();
        }
    }
    
    public static IReadOnlyList<ModPath> GetModsToCheck(IGameEnvironment env, ModPath sourceFile)
    {
        if (sourceFile != ModPath.Empty)
        {
            return new[] { sourceFile };
        }
        return env.LoadOrder.ListedOrder
            .OnlyEnabledAndExisting()
            .Select(x => new ModPath(Path.Combine(env.DataFolderPath, x.ModKey.FileName)))
            .ToList();
    }

    public static ReadOnlySpan<char> SkipPast(ReadOnlySpan<char> str, string target)
    {
        var index = str.IndexOf(target);
        if (index == -1)
        {
            throw new ArgumentException();
        }

        return str.Slice(index + target.Length);
    }
}
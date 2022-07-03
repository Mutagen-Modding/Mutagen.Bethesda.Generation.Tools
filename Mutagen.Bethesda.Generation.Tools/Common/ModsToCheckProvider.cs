using Mutagen.Bethesda.Plugins.Records;
using Noggog;

namespace Mutagen.Bethesda.Generation.Tools.Common;

public class ModsToCheckProvider
{
    private Lazy<IReadOnlyList<IModGetter>> _mods;
    public IReadOnlyList<IModGetter> Mods => _mods.Value;
    
    public ModsToCheckProvider(
        EnvironmentProvider environmentProvider)
    {
        _mods = new Lazy<IReadOnlyList<IModGetter>>(() =>
        {
            return environmentProvider.Environment.LoadOrder.ListedOrder
                .OnlyEnabledAndExisting()
                .Select(x => x.Mod)
                .NotNull()
                .ToList();
        });
    }
}
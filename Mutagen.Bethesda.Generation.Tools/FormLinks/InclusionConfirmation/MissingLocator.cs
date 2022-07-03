using Mutagen.Bethesda.Generation.Tools.Common;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;

namespace Mutagen.Bethesda.Generation.Tools.FormLinks.InclusionConfirmation;

public record ModSourceToMissingLinks
{
    public Dictionary<IFormLinkGetter, HashSet<FormKey>> SourceToMissingLinks { get; } = new();
}

public record ModsSourceToMissingLinks
{
    public Dictionary<ModKey, ModSourceToMissingLinks> Mods { get; } = new();
}

public record MissingLinkToModsMapping
{
    public Dictionary<Type, ModsSourceToMissingLinks> MissingLinkToMods { get; } = new();
}

public record SourceRecordsToMissingLinks
{
    public Dictionary<Type, MissingLinkToModsMapping> SourceToMissingLinks { get; } = new();
}

public class MissingLocator
{
    private readonly ModsToCheckProvider _modsToCheckProvider;
    private readonly TargetTypeProvider _targetTypeProvider;
    private readonly EnvironmentProvider _environmentProvider;
    private readonly ExclusionsProvider _exclusionsProvider;
    
    public MissingLocator(
        ModsToCheckProvider modsToCheckProvider,
        TargetTypeProvider targetTypeProvider,
        ExclusionsProvider exclusionsProvider, 
        EnvironmentProvider environmentProvider)
    {
        _modsToCheckProvider = modsToCheckProvider;
        _targetTypeProvider = targetTypeProvider;
        _exclusionsProvider = exclusionsProvider;
        _environmentProvider = environmentProvider;
    }

    public SourceRecordsToMissingLinks GetMissing()
    {
        SourceRecordsToMissingLinks missing = new();

        foreach (var mod in _modsToCheckProvider.Mods)
        {
            foreach (var context in mod.EnumerateMajorRecordSimpleContexts(_targetTypeProvider.GetTargetType()))
            {
                if (_exclusionsProvider.Exclude(context.Record.FormKey)) continue;
                foreach (var link in context.Record.EnumerateFormLinks())
                {
                    // If null, skip
                    if (link.IsNull) continue;
                    // If found in typed constraints, skip
                    if (_environmentProvider.Environment.LinkCache.TryResolve(link.FormKey, link.Type, out _)) continue;
                    // If not found at all, skip
                    if (!_environmentProvider.Environment.LinkCache.TryResolve(link.FormKey, typeof(IMajorRecordGetter), out var linked)) continue;

                    lock (missing)
                    {
                        missing.SourceToMissingLinks.GetOrAdd(context.Record.GetType())
                            .MissingLinkToMods.GetOrAdd(linked.GetType())
                            .Mods.GetOrAdd(context.ModKey)
                            .SourceToMissingLinks.GetOrAdd(context.Record.ToLinkFromRuntimeType())
                            .Add(link.FormKey);
                    }
                }
            }
        }

        return missing;
    }
}
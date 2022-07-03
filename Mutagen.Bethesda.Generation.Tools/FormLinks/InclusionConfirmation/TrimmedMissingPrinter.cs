using Mutagen.Bethesda.Generation.Tools.Common;

namespace Mutagen.Bethesda.Generation.Tools.FormLinks.InclusionConfirmation;

public class TrimmedMissingPrinter
{
    private readonly LocationRetriever _locationRetriever;
    private readonly EnvironmentProvider _environmentProvider;
    
    public TrimmedMissingPrinter(
        LocationRetriever locationRetriever,
        EnvironmentProvider environmentProvider)
    {
        _locationRetriever = locationRetriever;
        _environmentProvider = environmentProvider;
    }
    
    public void Print(SourceRecordsToMissingLinks missing)
    {
        Console.WriteLine($"Printing trimmed missing link detail:");
        foreach (var sourceRecord in missing.SourceToMissingLinks)
        {
            Console.WriteLine($"   {sourceRecord.Key.Name}:");
            foreach (var missingType in sourceRecord.Value.MissingLinkToMods)
            {
                var modKey = missingType.Value.Mods.First();
                var mod = _environmentProvider.Environment.LoadOrder[modKey.Key];
                var modCache = mod.Mod!.ToUntypedImmutableLinkCache();
                var formLink = modKey.Value.SourceToMissingLinks.First();
                var rec = modCache.Resolve(formLink.Key);
                var targetKey = formLink.Value.First();
                var loc = new Stack<string>();
                _locationRetriever.RetrieveLocationOf(rec, targetKey.FormKey, loc);
                Console.WriteLine($"        {formLink.Key.FormKey} missing {targetKey} [{missingType.Key.Name}]: {string.Join(" -> ", loc.Reverse())}");
            }
        }
    }
}
using CommandLine;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;

namespace Mutagen.Bethesda.Generation.Tools.ContentAnalyzers;

/// <summary>
/// Lists references to a particular record
/// </summary>
[Verb("find-references-to")]
public class FindReferencesTo
{
    [Option('f', "FormKey", HelpText = "FormKey to find references to")]
    public string FormKey { get; set; } = string.Empty;
    
    [Option('r', "Release", Required = true, HelpText = "GameRelease targeted")]
    public GameRelease Release { get; set; }

    public void Execute()
    {
        var fk = Plugins.FormKey.Factory(this.FormKey);
        
        using var env = Utility.GetGameEnvironmentState(Release, null);

        Console.WriteLine($"{FormKey} referenced by:");
        foreach (var rec in env.LoadOrder.PriorityOrder.WinningOverrides(typeof(IMajorRecordGetter)))
        {
            try
            {
                if (!rec.EnumerateFormLinks().Select(x => x.FormKeyNullable)
                        .Contains(fk))
                {
                    continue;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error considering {rec}");
            }
            
            
            Console.WriteLine($"  {rec}");
        }
    }
    
    public IReadOnlyDictionary<FormKey, HashSet<FormKey>> GetReferencedByLookups(IModGetter mod)
    {
        var lookup = new Dictionary<FormKey, HashSet<FormKey>>();
        foreach (var rec in mod.EnumerateMajorRecords())
        {
            foreach (var link in rec.EnumerateFormLinks())
            {
                lookup.GetOrAdd(link.FormKey).Add(rec.FormKey);
            }
        }
        return lookup;
    }
}
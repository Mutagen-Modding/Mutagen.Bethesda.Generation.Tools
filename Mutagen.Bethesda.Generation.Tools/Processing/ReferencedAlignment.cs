using CommandLine;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Analysis;
using Mutagen.Bethesda.Plugins.Meta;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;

namespace Mutagen.Bethesda.Generation.Tools.Processing;

[Verb("referenced-alignment", HelpText = "Aligns records and groups of two mods containing equivalent content")]
public class ReferencedAlignment
{
    [Option('i', "InputPath", 
        HelpText = "Path to the Bethesda plugin",
        Required = true)]
    public ModPath InputPath { get; set; } = string.Empty;
    
    [Option('r', "ReferencePath", 
        HelpText = "Path to the Bethesda plugin to reference to for alignment",
        Required = true)]
    public ModPath ReferencePath { get; set; } = string.Empty;
    
    [Option('o', "OutputPath", 
        HelpText = "Path to output the standardized Bethesda plugin",
        Required = true)]
    public ModPath OutputPath { get; set; } = string.Empty;
    
    [Option('g', "GameRelease",
        HelpText = "Game release that the plugin is related to",
        Required = false)]
    public GameRelease GameRelease { get; set; }

    public void Execute()
    {
        using var env = GameEnvironment.Typical.Construct(GameRelease);
        ILoadOrderGetter<IModFlagsGetter> lo = new LoadOrder<IModFlagsGetter>(
            env.LoadOrder.ListedOrder.ResolveAllModsExist());
        var loc = RecordLocator.GetLocations(ReferencePath, GameRelease, lo);
        var formKeyOrder = loc.ListedRecords.Values
            .Select(x => x.FormKey)
            .ToList();
    }
}
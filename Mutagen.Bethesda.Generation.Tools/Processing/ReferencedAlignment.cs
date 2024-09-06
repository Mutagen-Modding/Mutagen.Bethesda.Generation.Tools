using CommandLine;
using Mutagen.Bethesda.Plugins.Analysis;
using Mutagen.Bethesda.Plugins.Meta;
using Noggog;

namespace Mutagen.Bethesda.Generation.Tools.Processing;

[Verb("referenced-alignment", HelpText = "Aligns records and groups of two mods containing equivalent content")]
public class ReferencedAlignment
{
    [Option('i', "InputPath", 
        HelpText = "Path to the Bethesda plugin",
        Required = true)]
    public FilePath InputPath { get; set; } = string.Empty;
    
    [Option('r', "ReferencePath", 
        HelpText = "Path to the Bethesda plugin to reference to for alignment",
        Required = true)]
    public FilePath ReferencePath { get; set; } = string.Empty;
    
    [Option('o', "OutputPath", 
        HelpText = "Path to output the standardized Bethesda plugin",
        Required = true)]
    public FilePath OutputPath { get; set; } = string.Empty;
    
    [Option('g', "GameRelease",
        HelpText = "Game release that the plugin is related to",
        Required = false)]
    public GameRelease GameRelease { get; set; }

    public void Execute()
    {
        var loc = RecordLocator.GetLocations(ReferencePath, GameConstants.Get(GameRelease));
        var formKeyOrder = loc.ListedRecords.Values
            .Select(x => x.FormKey)
            .ToList();
    }
}
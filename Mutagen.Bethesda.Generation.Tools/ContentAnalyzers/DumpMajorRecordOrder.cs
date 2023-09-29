using CommandLine;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Analysis;

namespace Mutagen.Bethesda.Generation.Tools.ContentAnalyzers;

/// <summary>
/// Dumps the contained major record ordering
/// </summary>
[Verb("dump-major-record-order")]
public class DumpMajorRecordOrder
{
    [Option('f', "File", HelpText = "Path to a source file to analyze.  Will analyze environment load order if missing")]
    public ModPath SourceFile { get; set; } = ModPath.Empty;
    
    [Option('r', "Release", Required = true, HelpText = "GameRelease targeted")]
    public GameRelease Release { get; set; }

    public void Execute()
    {
        Console.WriteLine($"Checking {SourceFile}");
        Console.WriteLine($"Finding all record locations");
        var locs = RecordLocator.GetLocations(
            SourceFile,
            Release);

        var set = new HashSet<RecordType>();
        var list = new List<RecordType>();
        foreach (var item in locs.GrupLocations)
        {
            var type = item.Value.ContainedRecordType;
            if (set.Add(type))
            {
                if (type.Type.All(c => char.IsLetter(c)))
                {
                    list.Add(type);
                }
            }
        }
        
        Console.WriteLine($"Dumping data");
        foreach (var i in list)
        {
            Console.WriteLine($"   {i}");
        }
    }
}
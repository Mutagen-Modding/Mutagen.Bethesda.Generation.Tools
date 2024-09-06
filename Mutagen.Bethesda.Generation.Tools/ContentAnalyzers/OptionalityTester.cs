using System.Text;
using CommandLine;
using Mutagen.Bethesda.Generation.Tools.FormLinks;
using Mutagen.Bethesda.Generation.Tools.Strings;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Analysis;
using Mutagen.Bethesda.Plugins.Binary.Headers;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Plugins.Binary.Streams;
using Mutagen.Bethesda.Plugins.Binary.Translations;
using Mutagen.Bethesda.Plugins.Masters;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Strings;
using Noggog;

namespace Mutagen.Bethesda.Generation.Tools.ContentAnalyzers;

/// <summary>
/// Tests whether a subrecord is optional
/// </summary>
[Verb("optionality")]
public class OptionalityTester
{
    [Option('f', "File", HelpText = "Path to a source file to analyze.  Will analyze environment load order if missing")]
    public ModPath SourceFile { get; set; } = ModPath.Empty;
    
    [Option('r', "Release", Required = true, HelpText = "GameRelease targeted")]
    public GameRelease Release { get; set; }

    [Option('m', "Major", Required = true, HelpText = "MajorRecord RecordType to search under")]
    public string MajorRecordType { get; set; } = RecordType.Null.Type;

    [Option('s', "Sub", Required = true,  HelpText = "SubRecord RecordType to analyze")]
    public string SubRecordType { get; set; } = RecordType.Null.Type;

    public void Execute()
    {
        using var env = Utility.GetGameEnvironmentState(Release, SourceFile);
        var modsToCheck = Utility.GetModsToCheck(env, SourceFile);
        ILoadOrderGetter<IModFlagsGetter> lo = new LoadOrder<IModFlagsGetter>(
            env.LoadOrder.ListedOrder.ResolveAllModsExist());
        
        foreach (var modPath in modsToCheck)
        {
            Console.WriteLine($"Checking {modPath}");
            Console.WriteLine($"Finding all record locations");
            var locs = RecordLocator.GetLocations(
                modPath,
                Release,
                lo);
            using var stream = new MutagenBinaryReadStream(
                modPath, 
                ParsingMeta.Factory(BinaryReadParameters.Default, Release, modPath));
        
            foreach (var recordLocationMarker in locs.ListedRecords)
            {
                if (recordLocationMarker.Value.Record != MajorRecordType) continue;
                stream.Position = recordLocationMarker.Key;
                var majorFrame = stream.ReadMajorRecord();
                if (majorFrame.IsCompressed)
                {
                    majorFrame = majorFrame.Decompress(out _);
                }

                if (!majorFrame.TryFindSubrecord(SubRecordType, out _))
                {
                    Console.WriteLine($"{SubRecordType} optional");
                    Console.WriteLine("Done");
                    return;
                }
            }
        }
        Console.WriteLine($"{SubRecordType} NOT optional");
        Console.WriteLine("Done");
    }
}
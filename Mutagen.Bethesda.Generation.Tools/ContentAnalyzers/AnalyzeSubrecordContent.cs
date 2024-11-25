using System.IO.Abstractions;
using CommandLine;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Analysis;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Plugins.Binary.Streams;
using Mutagen.Bethesda.Plugins.Binary.Translations;
using Mutagen.Bethesda.Plugins.Masters;
using Mutagen.Bethesda.Plugins.Masters.DI;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Strings.DI;
using Noggog;

namespace Mutagen.Bethesda.Generation.Tools.ContentAnalyzers;

/// <summary>
/// Analyzes a specific subrecord for content and possibilities
/// </summary>
[Verb("analyze-subrecord")]
public class AnalyzeSubrecordContent
{
    [Option('f', "File", HelpText = "Path to a source file to analyze.  Will analyze environment load order if missing")]
    public ModPath SourceFile { get; set; } = ModPath.Empty;
    
    [Option('r', "Release", Required = true, HelpText = "GameRelease targeted")]
    public GameRelease Release { get; set; }

    [Option('m', "Major", HelpText = "MajorRecord RecordType to search under")]
    public string? MajorRecordType { get; set; }

    [Option('s', "Sub", Required = true,  HelpText = "SubRecord RecordType to analyze")]
    public string SubRecordType { get; set; } = RecordType.Null.Type;

    [Option('x', "FromIndex", Required = false,  HelpText = "Subsection start index")]
    public int? FromIndex { get; set; }

    [Option('y', "ToIndex", Required = false,  HelpText = "Subsection end index")]
    public int? EndIndex { get; set; }

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

            var masterFlagsCompiler = new MasterFlagsLookupCompiler(
                new FileSystem(),
                new GameReleaseInjection(Release),
                new DataDirectoryInjection(env.DataFolderPath));
            
            using var stream = new MutagenBinaryReadStream(
                modPath, 
                ParsingMeta.Factory(BinaryReadParameters.Default with
                {
                    MasterFlagsLookup = masterFlagsCompiler.ConstructFor(modPath)
                }, Release, modPath));
            Console.WriteLine($"Dumping data");
            List<ReadOnlyMemorySlice<byte>> recs = new();
            foreach (var recordLocationMarker in locs.ListedRecords)
            {
                if (MajorRecordType != null && recordLocationMarker.Value.Record != MajorRecordType) continue;
                stream.Position = recordLocationMarker.Key;
                var majorFrame = stream.ReadMajorRecord();
                if (majorFrame.IsCompressed)
                {
                    majorFrame = majorFrame.Decompress(out _);
                }

                foreach (var subRec in majorFrame.FindEnumerateSubrecords(SubRecordType))
                {
                    var content = subRec.Content;
                    if (FromIndex != null && EndIndex != null)
                    {
                        content = content.Slice(FromIndex.Value, EndIndex.Value - FromIndex.Value);
                    }
                    else if (FromIndex != null)
                    {
                        content = content.Slice(FromIndex.Value);
                    }
                    else if (EndIndex != null)
                    {
                        content = content.Slice(0, EndIndex.Value);
                    }
                    recs.Add(content);
                }
            }
            
            Console.WriteLine("Raw byte content:");
            foreach (var hexStr in recs
                         .Select(x => $"0x{x.ToHexString()}")
                         .Distinct()
                         .OrderBy(x => x.Length))
            {
                Console.WriteLine($"  {hexStr} ({hexStr.Length - 2})");
            }

            Console.WriteLine("String content:");
            foreach (var str in recs
                         .Select(x => BinaryStringUtility.ProcessWholeToZString(x, MutagenEncoding.GetEncoding(Release, Language.English)))
                         .Distinct()
                         .OrderBy(x => x))
            {
                Console.WriteLine($"  {str}");
            }
        }
    }
}
using System.Text;
using CommandLine;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Analysis;
using Mutagen.Bethesda.Plugins.Binary.Streams;
using Noggog;

namespace Mutagen.Bethesda.Generation.Tools.ContentAnalyzers;

/// <summary>
/// Dumps the contained subrecord data of the target Major Record
/// </summary>
[Verb("dump-subrecords")]
public class DumpSubrecords
{
    [Option('f', "File", HelpText = "Path to a source file to analyze.  Will analyze environment load order if missing")]
    public ModPath SourceFile { get; set; } = ModPath.Empty;
    
    [Option('r', "Release", Required = true, HelpText = "GameRelease targeted")]
    public GameRelease Release { get; set; }

    [Option('m', "Major", Required = true, HelpText = "MajorRecord RecordType to search under")]
    public string MajorRecordType { get; set; } = RecordType.Null.Type;

    private class Counter
    {
        public int RecordCount;
        public Dictionary<int, int> LengthCount = new();
    }

    public void Execute()
    {
        using var env = Utility.GetGameEnvironmentState(Release, SourceFile);
        var modsToCheck = env.LoadOrder.ListedOrder
            .OnlyEnabledAndExisting()
            .Select(x => new ModPath(Path.Combine(env.DataFolderPath, x.ModKey.FileName)))
            .ToList();
        var subrecordCounter = new Dictionary<RecordType, Counter>();
        var printedStrings = new HashSet<string>();
        foreach (var modPath in modsToCheck)
        {
            Console.WriteLine($"Checking {modPath}");
            Console.WriteLine($"Finding all record locations");
            var locs = RecordLocator.GetLocations(
                modPath,
                Release);
            using var stream = new MutagenBinaryReadStream(modPath, Release);
            Console.WriteLine($"Dumping data");
            foreach (var recordLocationMarker in locs.ListedRecords)
            {
                if (recordLocationMarker.Value.Record != MajorRecordType) continue;
                stream.Position = recordLocationMarker.Key;
                var majorFrame = stream.ReadMajorRecord();
                if (majorFrame.IsCompressed)
                {
                    majorFrame = majorFrame.Decompress(out _);
                }

                StringBuilder sb = new();
                sb.Append($"{majorFrame.RecordType} ->");
                
                foreach (var subRec in majorFrame.EnumerateSubrecords())
                {
                    var subRecItem = subrecordCounter.GetOrAdd(subRec.RecordType);
                    subRecItem.RecordCount++;
                    
                    if (!subRecItem.LengthCount.TryGetValue(subRec.ContentLength, out var lengthCount))
                    {
                        lengthCount = 0;
                    }
                    subRecItem.LengthCount[subRec.ContentLength] = lengthCount + 1;
                    
                    sb.Append($" {subRec.RecordType}");
                }

                var str = sb.ToString();
                if (printedStrings.Add(str))
                {
                    Console.WriteLine(str);
                }
            }
        }

        Console.WriteLine($"Writing summary counts for {MajorRecordType}:");
        foreach (var entry in subrecordCounter.OrderBy(x => x.Key.Type))
        {
            Console.WriteLine($"  {entry.Key}: {entry.Value.RecordCount}");
        }
        
        Console.WriteLine($"Writing detailed summary counts for {MajorRecordType}:");
        foreach (var entry in subrecordCounter.OrderBy(x => x.Key.Type))
        {
            Console.WriteLine($"  {entry.Key}: {entry.Value.RecordCount}");
            foreach (var lenEntry in entry.Value.LengthCount)
            {
                Console.WriteLine($"    Len {lenEntry.Key}: {lenEntry.Value}");
            }
        }
        Console.WriteLine("Done");
    }
}
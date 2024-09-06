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

    private class LengthCounter
    {
        public int RecordCount;
        public Dictionary<int, int> LengthCount = new();
        public List<ReadOnlyMemorySlice<byte>> Datas = new();
    }
    
    private class OffsetCounter
    {
        public Dictionary<RecordType, (int Times, HashSet<FormKey> Targets)> LinkCount = new();
        public int Count;
    }
    
    private class LinkCounter
    {
        public RecordType Type;
        public int Count;
    }

    public void Execute()
    {
        using var env = Utility.GetGameEnvironmentState(Release, SourceFile);
        ILoadOrderGetter<IModFlagsGetter> lo = new LoadOrder<IModFlagsGetter>(
            env.LoadOrder.ListedOrder.ResolveAllModsExist());
        var modsToCheck = Utility.GetModsToCheck(env, SourceFile);
        var subrecordCounter = new Dictionary<RecordType, LengthCounter>();
        var printedStrings = new HashSet<string>();
        var formLinkFishing = new Dictionary<RecordType, Dictionary<int, OffsetCounter>>();
        var stringResults = new Dictionary<StringMappingFisher.TargetSubrecord, StringMappingFisher.StringsSourceDictionary>();
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
            Console.WriteLine($"Dumping data");
        
            var stringsOverlay = StringsFolderLookupOverlay.TypicalFactory(
                Release,
                modPath.ModKey,
                env.DataFolderPath,
                null);
            
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
                    subRecItem.Datas.Add(subRec.Content);
                    
                    sb.Append($" {subRec.RecordType}");

                    FishForFormLinks(locs, stream.MetaData.MasterReferences, subRec, formLinkFishing);

                    StringMappingFisher.CheckSubrecordIsString(
                        majorFrame.RecordType,
                        stringResults,
                        subRec,
                        stringsOverlay,
                        index: null);
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
            if (FormLinkTypeFisher.IsLikelyNullTerminatedString(entry.Value.Datas))
            {
                Console.WriteLine($"    Likely string");
            }
            else
            {
                foreach (var lenEntry in entry.Value.LengthCount)
                {
                    Console.WriteLine($"    Len {lenEntry.Key}: {lenEntry.Value}");
                }
            }
        }
        
        Console.WriteLine($"Writing potential formlink counts for {MajorRecordType}:");
        foreach (var entry in formLinkFishing
                     .OrderBy(x => x.Key.Type))
        {
            Console.WriteLine($"  {entry.Key} ({subrecordCounter[entry.Key].RecordCount}):");
            foreach (var offsetEntry in entry.Value.OrderBy(x => x.Key))
            {
                Console.WriteLine($"    Offset {offsetEntry.Key}:");
                foreach (var linkEntry in offsetEntry.Value.LinkCount)
                {
                    Console.WriteLine($"      {linkEntry.Key}: {linkEntry.Value.Times} times.  ({linkEntry.Value.Targets.Count} unique)");
                }
            }
        }
        
        Console.WriteLine($"Writing potential string counts for {MajorRecordType}:");
        StringMappingFisher.PrintResults(stringResults);
        
        Console.WriteLine("Done");
    }

    private void FishForFormLinks(
        RecordLocatorResults locs,
        IReadOnlySeparatedMasterPackage masters,
        SubrecordPinFrame subRec,
        Dictionary<RecordType, Dictionary<int, OffsetCounter>> tracker)
    {
        var content = subRec.Content;
        if (content.Length < 4 || content.Length > 200) return;
        if (FormLinkTypeFisher.IsLikelyNullTerminatedString(content))
        {
            return;
        }
        var times = (content.Length - 4) / 2 + 1;
        for (int i = 0; i < times; i++)
        {
            var link = FormKeyBinaryTranslation.Instance.Parse(
                content,
                masters);
            if (!link.IsNull && locs.TryGetRecord(link, out var otherRec))
            {
                var offsetTracker = tracker.GetOrAdd(subRec.RecordType).GetOrAdd(i * 2);
                offsetTracker.Count++;
                var linkTracker = offsetTracker.LinkCount.GetOrAdd(otherRec.Record, () =>
                {
                    return (Times: 0, Targets: new());
                });
                linkTracker.Targets.Add(link);
                offsetTracker.LinkCount[otherRec.Record] = (linkTracker.Times + 1, linkTracker.Targets);
            }

            content = content.Slice(2);
        }
    }
}
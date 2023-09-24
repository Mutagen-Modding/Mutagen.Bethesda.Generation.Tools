using CommandLine;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Analysis;
using Mutagen.Bethesda.Plugins.Binary.Streams;
using Mutagen.Bethesda.Plugins.Binary.Translations;
using Noggog;

namespace Mutagen.Bethesda.Generation.Tools.FormLinks;

/// <summary>
/// Helps analyze one specific FormID within a subrecord and see what MajorRecord types it refers to.
/// Can help locate some of the types it's allowed to point to, but will not find all, as there might be
/// not existing record that makes use of a particular link type, even if it's allowed. 
/// </summary>
[Verb("formlink-type-fisher")]
public class FormLinkTypeFisher
{
    [Option('f', "File", HelpText = "Path to a source file to analyze.  Will analyze environment load order if missing")]
    public ModPath SourceFile { get; set; } = ModPath.Empty;
    
    [Option('r', "Release", Required = true, HelpText = "GameRelease targeted")]
    public GameRelease Release { get; set; }

    [Option('m', "Major", Required = true, HelpText = "MajorRecord RecordType to search under")]
    public string MajorRecordType { get; set; } = RecordType.Null.Type;
    
    [Option('s', "Sub", Required = true,  HelpText = "SubRecord RecordType to analyze")]
    public string SubRecordType { get; set; } = RecordType.Null.Type;
    
    [Option('o', "Offset", Required = false, HelpText = "Binary length offset from the start of the subrecord to where the FormLink is")]
    public ushort Offset { get; set; }
    
    [Option('p', "RepeatEvery", Required = false, HelpText = "How often to repeat the check based on length")]
    public ushort? RepeatEvery { get; set; }

    public class FormLinkFisherDictionary
    {
        public readonly Dictionary<RecordType, Results> ByLinkedRecordType = new();
    }
    
    public class Results
    {
        public int Count;
    }
    
    public void Execute()
    {
        using var env = Utility.GetGameEnvironmentState(Release, SourceFile);
        var modsToCheck = env.LoadOrder.ListedOrder
            .OnlyEnabledAndExisting()
            .Select(x => new ModPath(Path.Combine(env.DataFolderPath, x.ModKey.FileName)))
            .ToList();
        var targetedTypes = new FormLinkFisherDictionary();
        bool isLikelyString = true;
        foreach (var modPath in modsToCheck)
        {
            Console.WriteLine($"Checking {modPath}");
            Console.WriteLine($"Finding all record locations");
            var locs = RecordLocator.GetLocations(
                modPath,
                Release);
            using var stream = new MutagenBinaryReadStream(modPath, Release);
            Console.WriteLine($"Analyzing target links");
            foreach (var recordLocationMarker in locs.ListedRecords)
            {
                if (recordLocationMarker.Value.Record != MajorRecordType) continue;
                stream.Position = recordLocationMarker.Key;
                var majorFrame = stream.ReadMajorRecord();
                if (majorFrame.IsCompressed)
                {
                    majorFrame = majorFrame.Decompress(out _);
                }
                foreach (var subRec in majorFrame.FindEnumerateSubrecords(SubRecordType))
                {
                    var content = subRec.Content;
                    var times = RepeatEvery == null ? 1 : subRec.ContentLength / RepeatEvery;
                    for (int i = 0; i < times; i++)
                    {
                        var link = FormKeyBinaryTranslation.Instance.Parse(
                            content.Slice(Offset),
                            stream.MetaData.MasterReferences);
                        if (!link.IsNull && locs.TryGetRecord(link, out var otherRec))
                        {
                            var cur = targetedTypes.ByLinkedRecordType.GetOrAdd(otherRec.Record);
                            cur.Count++;
                            targetedTypes.ByLinkedRecordType[otherRec.Record] = cur;
                        }

                        if (RepeatEvery != null)
                        {
                            content.Slice(RepeatEvery.Value);
                        }
                    }

                    if (isLikelyString && !IsLikelyNullTerminatedString(subRec.Content))
                    {
                        isLikelyString = false;
                    }
                }
            }
        }

        Console.WriteLine($"{MajorRecordType} -> {SubRecordType} {(Offset == 0 ? null : $"at offset {Offset} ")}targeted:");
        PrintResults(targetedTypes);
        Console.WriteLine("Done");
    }

    public static void PrintResults(FormLinkFisherDictionary targetedTypes)
    {
        foreach (var type in targetedTypes.ByLinkedRecordType.OrderBy(x => x.Key.Type))
        {
            Console.WriteLine($"  {type.Key}: {type.Value.Count}");
        }
    }

    public static bool IsLikelyNullTerminatedString(ReadOnlyMemorySlice<byte> bytes)
    {
        if (bytes.Length == 0) return false;
        if (bytes[^1] != 0) return false;
        return bytes.SliceUpTo(bytes.Length - 1).All(x => char.IsAscii((char)x));
    }

    public static bool IsLikelyNullTerminatedString(IReadOnlyCollection<ReadOnlyMemorySlice<byte>> bytes)
    {
        if (!bytes.Select(x => x.Length).Distinct().CountGreaterThan(4)) return false;
        return bytes.All(IsLikelyNullTerminatedString);
    }
}
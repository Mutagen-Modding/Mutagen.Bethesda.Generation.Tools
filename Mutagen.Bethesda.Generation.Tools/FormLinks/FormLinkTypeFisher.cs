using System.IO.Abstractions;
using CommandLine;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Analysis;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Plugins.Binary.Streams;
using Mutagen.Bethesda.Plugins.Binary.Translations;
using Mutagen.Bethesda.Plugins.Masters.DI;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
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

    [Option('m', "Major", HelpText = "MajorRecord RecordType to search under")]
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
        using var env = Utility.GetGameEnvironmentStateWithSeparateSupport(Release, SourceFile);
        var modsToCheck = Utility.GetModsToCheck(env, SourceFile);
        ILoadOrderGetter<IModFlagsGetter> lo = new LoadOrder<IModFlagsGetter>(
            env.LoadOrder.ListedOrder.ResolveAllModsExist());
        var targetedTypes = new FormLinkFisherDictionary();
        var lookup = new Dictionary<FormKey, RecordLocationMarker>();
        var locsLookup = new Dictionary<ModKey, RecordLocatorResults>();

        foreach (var modPath in modsToCheck)
        {
            Console.WriteLine($"Finding all record locations in {modPath}");
            var locs = RecordLocator.GetLocations(
                modPath,
                Release,
                lo);
            locsLookup[modPath.ModKey] = locs;
        }

        lookup = locsLookup.Values
            .SelectMany(x => x.ListedRecords.Values)
            .DistinctBy(x => x.FormKey)
            .ToDictionary(x => x.FormKey, x => x);

        bool isLikelyString = true;
        foreach (var modPath in modsToCheck)
        {
            Console.WriteLine($"Checking {modPath}");

            var masterFlagsCompiler = new MasterFlagsLookupCompiler(
                new FileSystem(),
                new GameReleaseInjection(Release),
                new DataDirectoryInjection(env.DataFolderPath));
            
            var locs = locsLookup[modPath.ModKey];
            using var stream = new MutagenBinaryReadStream(
                modPath, 
                ParsingMeta.Factory(BinaryReadParameters.Default with
                {
                    MasterFlagsLookup = masterFlagsCompiler.ConstructFor(modPath)
                }, Release, modPath));
            Console.WriteLine($"Analyzing target links");
            foreach (var recordLocationMarker in locs.ListedRecords)
            {
                if (MajorRecordType != RecordType.Null.Type && recordLocationMarker.Value.Record != MajorRecordType) continue;
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
                        var formKey = FormKeyBinaryTranslation.Instance.Parse(
                            content.Slice(Offset),
                            stream.MetaData.MasterReferences);
                        if (!formKey.IsNull && lookup.TryGetValue(formKey, out var otherRec))
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
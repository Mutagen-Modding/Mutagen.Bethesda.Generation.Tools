using CommandLine;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Analysis;
using Mutagen.Bethesda.Plugins.Binary.Headers;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Plugins.Binary.Streams;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Utility;
using Mutagen.Bethesda.Strings;
using Noggog;

namespace Mutagen.Bethesda.Generation.Tools.Strings;

[Verb("string-mapping-fisher")]
public class StringMappingFisher
{
    [Option('f', "File", HelpText = "Path to a source file to analyze.  Will analyze environment load order if missing")]
    public ModPath SourceFile { get; set; } = ModPath.Empty;

    [Option('r', "Release", Required = true, HelpText = "GameRelease targeted")]
    public GameRelease Release { get; set; }

    [Option('m', "Major", Required = false, HelpText = "MajorRecord RecordType to search under")]
    public string? MajorRecordType { get; set; }

    [Option('i', "Index", Required = false, HelpText = "Specific index to look for")]
    public uint? Index { get; set; }

    public record TargetSubrecord(RecordType Major, RecordType SubRecord);

    public class StringsSourceDictionary
    {
        public readonly Dictionary<StringsSource, StringIdCounterDictionary> ByStringsSource = new();
    }

    public class StringIdCounterDictionary
    {
        public readonly Dictionary<uint, int> ByStringIdCounter = new();
    }

    public void Execute()
    {
        if (!Release.ToCategory().HasLocalization())
        {
            Console.WriteLine("Game does not support localization.  No work to be done.");
            return;
        }

        var results = new Dictionary<TargetSubrecord, StringsSourceDictionary>();

        if (SourceFile == ModPath.Empty)
        {
            using var env = GameEnvironment.Typical.Construct(Release);
            foreach (var mod in env.LoadOrder.ListedOrder)
            {
                if (!mod.Enabled) continue;
                var path = ModPath.FromPath(Path.Combine(env.DataFolderPath, mod.ModKey.FileName));
                if (!CheckModIsLocalized(path, Release)) continue;
                FillResults(
                    Release,
                    MajorRecordType,
                    path,
                    env.DataFolderPath,
                    results,
                    Index);
            }
        }
        else
        {
            if (!CheckModIsLocalized(SourceFile, Release)) return;
            FillResults(
                Release,
                MajorRecordType,
                SourceFile,
                Path.GetDirectoryName(SourceFile)!,
                results,
                Index);
        }

        PrintResults(results);
        Console.WriteLine("Done");
    }

    private bool CheckModIsLocalized(ModPath path, GameRelease release)
    {
        using var stream = new MutagenBinaryReadStream(
            path, 
            ParsingMeta.Factory(BinaryReadParameters.Default, release, path));
        var modHeader = stream.ReadModHeader();

        var localizedIndex = Release.ToCategory().GetLocalizedFlagIndex();
        if (!Enums.HasFlag(modHeader.Flags, localizedIndex!.Value))
        {
            Console.WriteLine($"File is not localized, and so checker will not work as intended: {path}");
            return false;
        }

        return true;
    }

    public static void FillResults(
        GameRelease release,
        string? majorRecordType,
        ModPath modPath,
        DirectoryPath dataPath,
        Dictionary<TargetSubrecord, StringsSourceDictionary> results,
        uint? index)
    {        
        using var env = Utility.GetGameEnvironmentState(release, modPath);
        ILoadOrderGetter<IModFlagsGetter> lo = new LoadOrder<IModFlagsGetter>(
            env.LoadOrder.ListedOrder.ResolveAllModsExist());
        
        using var stream = new MutagenBinaryReadStream(
            modPath, 
            ParsingMeta.Factory(BinaryReadParameters.Default, release, modPath));
        
        var stringsOverlay = StringsFolderLookupOverlay.TypicalFactory(
            release,
            modPath.ModKey,
            dataPath,
            null);

        Console.WriteLine($"Finding all record locations in {modPath}...");
        var locs = RecordLocator.GetLocations(
            modPath,
            release,
            lo,
            majorRecordType == null ? null : new RecordInterest(majorRecordType));

        Console.WriteLine($"Analyzing subrecords for strings in {modPath}...");
        foreach (var recordLocationMarker in locs.ListedRecords)
        {
            stream.Position = recordLocationMarker.Key;
            var origMajorFrame = stream.ReadMajorRecord();
            var majorFrame = origMajorFrame;
            if (majorFrame.IsCompressed)
            {
                majorFrame = majorFrame.Decompress(out _);
            }
            foreach (var subRec in majorFrame.EnumerateSubrecords())
            {
                CheckSubrecordIsString(majorFrame.RecordType, results, subRec, stringsOverlay, index);
            }
        }
    }

    public static void CheckSubrecordIsString(
        RecordType majorRecType,
        Dictionary<TargetSubrecord, StringsSourceDictionary> results, 
        SubrecordPinFrame subRec,
        StringsFolderLookupOverlay stringsOverlay,
        uint? index)
    {
        if (subRec.ContentLength != 4) return;
        var key = subRec.AsUInt32();
        if (key == 0) return;

        if (index.HasValue && index.Value != key) return;

        foreach (var source in Enums<StringsSource>.Values)
        {
            if (stringsOverlay.TryLookup(source, Language.English, key, out _))
            {
                results.GetOrAdd(new(majorRecType, subRec.RecordType))
                    .ByStringsSource.GetOrAdd(source)
                    .ByStringIdCounter.UpdateOrAdd(key, (existing) => existing + 1);
            }
        }
    }

    public static void PrintResults(Dictionary<TargetSubrecord, StringsSourceDictionary> results)
    {
        foreach (var rec in results.GroupBy(x => x.Key.Major))
        {
            Console.WriteLine($"/=== {rec.Key}:");
            var highPotentials = rec.Where(IsHighPotential).ToArray();

            if (highPotentials.Length > 0)
            {
                Console.WriteLine(
                    $"| ---- High Potentials: ---");
            }
            
            foreach (var subRec in highPotentials)
            {
                Console.WriteLine(
                    $"| {subRec.Key.SubRecord} => {string.Join(", ", subRec.Value.ByStringsSource.Select(x => $"{x.Key}:{x.Value.ByStringIdCounter.Count}"))}");
            }

            if (highPotentials.Length > 0)
            {
                Console.WriteLine("|");
            }

            var lowLikelihood = rec.Where(x => !IsHighPotential(x)).ToArray();
            if (lowLikelihood.Length > 0)
            {
                Console.WriteLine(
                    $"| ---- Lower Potentials: ---");
                foreach (var subRec in lowLikelihood)
                {
                    Console.WriteLine(
                        $"| {subRec.Key.SubRecord} => {string.Join(", ", subRec.Value.ByStringsSource.Select(x => $"{x.Key}:{x.Value.ByStringIdCounter.Count}"))}");
                }
            }
            
            Console.WriteLine("===================/");
            Console.WriteLine();
        }
    }

    private static bool IsHighPotential(KeyValuePair<TargetSubrecord, StringsSourceDictionary> x)
    {
        return x.Value.ByStringsSource.Count == 1 
               && x.Value.ByStringsSource.Values.All(x =>
                   x.ByStringIdCounter.Values.All(x => x == 1));
    }
}

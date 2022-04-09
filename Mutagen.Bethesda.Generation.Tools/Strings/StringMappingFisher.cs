using System.Diagnostics;
using CommandLine;
using Loqui;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Installs;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Analysis;
using Mutagen.Bethesda.Plugins.Binary.Streams;
using Mutagen.Bethesda.Plugins.Cache.Internals.Implementations;
using Mutagen.Bethesda.Plugins.Implicit.DI;
using Mutagen.Bethesda.Plugins.Order.DI;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Records.DI;
using Mutagen.Bethesda.Plugins.Records.Mapping;
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

    record TargetSubrecord(RecordType Major, RecordType SubRecord);

    public void Execute()
    {
        if (!Release.ToCategory().HasLocalization())
        {
            Console.WriteLine("Game does not support localization.  No work to be done.");
            return;
        }

        var results = new Dictionary<TargetSubrecord, Dictionary<StringsSource, Dictionary<uint, int>>>();

        if (SourceFile == ModPath.Empty)
        {
            using var env = GameEnvironment.Typical.Construct(Release);
            foreach (var mod in env.LoadOrder.ListedOrder)
            {
                if (!mod.Enabled) continue;
                var path = ModPath.FromPath(Path.Combine(env.DataFolderPath, mod.ModKey.FileName));
                if (!CheckModIsLocalized(path)) continue;
                FillResults(
                    Release,
                    MajorRecordType,
                    path,
                    env.DataFolderPath,
                    results);
            }
        }
        else
        {
            if (!CheckModIsLocalized(SourceFile)) return;
            FillResults(
                Release,
                MajorRecordType,
                SourceFile,
                Path.GetDirectoryName(SourceFile)!,
                results);
        }

        PrintResults(results);
        Console.WriteLine("Done");
    }

    private bool CheckModIsLocalized(ModPath path)
    {
        using var stream = new MutagenBinaryReadStream(path, Release);
        var modHeader = stream.ReadModHeader();

        if (!EnumExt.HasFlag(modHeader.Flags, (int)ModHeaderCommonFlag.Localized))
        {
            Console.WriteLine($"File is not localized, and so checker will not work as intended: {path}");
            return false;
        }

        return true;
    }

    private static void FillResults(
        GameRelease release,
        string? majorRecordType,
        ModPath modPath,
        DirectoryPath dataPath,
        Dictionary<TargetSubrecord, Dictionary<StringsSource, Dictionary<uint, int>>> results)
    {        
        using var stream = new MutagenBinaryReadStream(modPath, release);
        
        var stringsOverlay = StringsFolderLookupOverlay.TypicalFactory(
            release,
            modPath.ModKey,
            dataPath,
            null);

        Console.WriteLine($"Finding all record locations in {modPath}...");
        var locs = RecordLocator.GetLocations(
            modPath,
            release,
            majorRecordType == null ? null : new RecordInterest(majorRecordType));

        Console.WriteLine($"Analyzing subrecords for strings in {modPath}...");
        foreach (var recordLocationMarker in locs.ListedRecords)
        {
            stream.Position = recordLocationMarker.Key;
            var origMajorFrame = stream.ReadMajorRecordFrame();
            var majorFrame = origMajorFrame;
            if (majorFrame.IsCompressed)
            {
                majorFrame = majorFrame.Decompress(out _);
            }
            foreach (var subRec in majorFrame.EnumerateSubrecords())
            {
                if (subRec.ContentLength != 4) continue;
                var key = subRec.AsUInt32();
                if (key == 0) continue;

                foreach (var source in EnumExt.GetValues<StringsSource>())
                {
                    if (stringsOverlay.TryLookup(source, Language.English, key, out _))
                    {
                        results.GetOrAdd(new(majorFrame.RecordType, subRec.RecordType))
                            .GetOrAdd(source)
                            .UpdateOrAdd(key, (existing) => existing + 1);
                    }
                }
            }
        }
    }

    private static void PrintResults(Dictionary<TargetSubrecord, Dictionary<StringsSource, Dictionary<uint, int>>> results)
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
                    $"| {subRec.Key.SubRecord} => {string.Join(", ", subRec.Value.Select(x => $"{x.Key}:{x.Value.Count}"))}");
            }

            if (highPotentials.Length > 0)
            {
                Console.WriteLine("|");
            }

            var lowLiklihood = rec.Where(x => !IsHighPotential(x)).ToArray();
            if (lowLiklihood.Length > 0)
            {
                Console.WriteLine(
                    $"| ---- Lower Potentials: ---");
                foreach (var subRec in lowLiklihood)
                {
                    Console.WriteLine(
                        $"| {subRec.Key.SubRecord} => {string.Join(", ", subRec.Value.Select(x => $"{x.Key}:{x.Value.Count}"))}");
                }
            }
            
            Console.WriteLine("===================/");
            Console.WriteLine();
        }
    }

    private static bool IsHighPotential(KeyValuePair<TargetSubrecord, Dictionary<StringsSource, Dictionary<uint, int>>> x)
    {
        return x.Value.Count == 1;
    }
}

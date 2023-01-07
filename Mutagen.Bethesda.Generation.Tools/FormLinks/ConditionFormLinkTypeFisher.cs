﻿using System.Buffers.Binary;
using CommandLine;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Analysis;
using Mutagen.Bethesda.Plugins.Binary.Streams;
using Mutagen.Bethesda.Plugins.Binary.Translations;
using Mutagen.Bethesda.Plugins.Records.Internals;
using Noggog;

namespace Mutagen.Bethesda.Generation.Tools.FormLinks;

[Verb("condition-formlink-type-fisher")]
public class ConditionFormLinkTypeFisher
{
    [Option('f', "File", HelpText = "Path to a source file to analyze.  Will analyze environment load order if missing")]
    public ModPath SourceFile { get; set; } = ModPath.Empty;
    
    [Option('r', "Release", Required = true, HelpText = "GameRelease targeted")]
    public GameRelease Release { get; set; }

    private class ParametersTypes
    {
        public readonly HashSet<RecordType> First = new();
        public readonly HashSet<RecordType> Second = new();
    }
    
    public void Execute()
    {
        using var env = Utility.GetGameEnvironmentState(Release, SourceFile);
        var modsToCheck = env.LoadOrder.ListedOrder
            .OnlyEnabledAndExisting()
            .Select(x => new ModPath(Path.Combine(env.DataFolderPath, x.ModKey.FileName)))
            .ToList();
        var targetedTypes = new Dictionary<int, ParametersTypes>();
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
                stream.Position = recordLocationMarker.Key;
                var majorFrame = stream.ReadMajorRecord();
                if (majorFrame.IsCompressed)
                {
                    majorFrame = majorFrame.Decompress(out _);
                }
                foreach (var subRec in majorFrame.FindEnumerateSubrecords(RecordTypes.CTDA))
                {
                    var content = subRec.Content;
                    var function = BinaryPrimitives.ReadInt16LittleEndian(content.Slice(8));
                    var link = FormKeyBinaryTranslation.Instance.Parse(
                        content.Slice(12),
                        stream.MetaData.MasterReferences);
                    var link2 = FormKeyBinaryTranslation.Instance.Parse(
                        content.Slice(16),
                        stream.MetaData.MasterReferences);
                    if (!link.IsNull && locs.TryGetRecord(link, out var otherRec))
                    {
                        targetedTypes.GetOrAdd(function).First.Add(otherRec.Record);
                    }
                    if (!link2.IsNull && locs.TryGetRecord(link2, out otherRec))
                    {
                        targetedTypes.GetOrAdd(function).Second.Add(otherRec.Record);
                    }
                }
            }
        }

        foreach (var function in targetedTypes)
        {
            if (function.Value.First.Count == 0 && function.Value.Second.Count == 0) continue;
            Console.WriteLine($"{function.Key} targeted:");
            if (function.Value.First.Count != 0)
            {
                Console.WriteLine($"   First:");
                foreach (var type in function.Value.First.OrderBy(x => x.Type))
                {
                    Console.WriteLine($"      {type}");
                }
            }
            if (function.Value.Second.Count != 0)
            {
                Console.WriteLine($"   Second:");
                foreach (var type in function.Value.Second.OrderBy(x => x.Type))
                {
                    Console.WriteLine($"      {type}");
                }
            }
        }
        Console.WriteLine("Done");
    }
}
using System.Buffers.Binary;
using CommandLine;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Analysis;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Plugins.Binary.Streams;
using Mutagen.Bethesda.Plugins.Binary.Translations;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
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
        public readonly Dictionary<RecordType, int> First = new();
        public readonly Dictionary<RecordType, int> Second = new();
    }
    
    public void Execute()
    {
        using var env = Utility.GetGameEnvironmentState(Release, SourceFile);
        var modsToCheck = Utility.GetModsToCheck(env, SourceFile);
        ILoadOrderGetter<IModFlagsGetter> lo = new LoadOrder<IModFlagsGetter>(
            env.LoadOrder.ListedOrder.ResolveAllModsExist());
        var targetedTypes = new Dictionary<int, ParametersTypes>();
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
                        targetedTypes.GetOrAdd(function).First.UpdateOrAdd(otherRec.Record, i => i + 1);
                    }
                    if (!link2.IsNull && locs.TryGetRecord(link2, out otherRec))
                    {
                        targetedTypes.GetOrAdd(function).Second.UpdateOrAdd(otherRec.Record, i => i + 1);
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
                foreach (var kv in function.Value.First.OrderBy(x => x.Key.Type))
                {
                    Console.WriteLine($"      {kv.Key}: {kv.Value}");
                }
            }
            if (function.Value.Second.Count != 0)
            {
                Console.WriteLine($"   Second:");
                foreach (var kv in function.Value.Second.OrderBy(x => x.Key.Type))
                {
                    Console.WriteLine($"      {kv.Key}: {kv.Value}");
                }
            }
        }
        Console.WriteLine("Done");
    }
}
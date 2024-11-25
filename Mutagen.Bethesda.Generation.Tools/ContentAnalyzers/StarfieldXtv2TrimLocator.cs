using System.Buffers.Binary;
using System.IO.Abstractions;
using CommandLine;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Analysis;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Plugins.Binary.Streams;
using Mutagen.Bethesda.Plugins.Masters.DI;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Utility;
using Mutagen.Bethesda.Starfield.Internals;
using Noggog;

namespace Mutagen.Bethesda.Generation.Tools.ContentAnalyzers;

[Verb("xtv2-trim-locator")]
public class StarfieldXtv2TrimLocator
{
    [Option('f', "File", HelpText = "Path to a source file to analyze.  Will analyze environment load order if missing")]
    public ModPath SourceFile { get; set; } = ModPath.Empty;
    
    [Option('r', "Release", Required = true, HelpText = "GameRelease targeted")]
    public GameRelease Release { get; set; }

    public void Execute()
    {
        Console.WriteLine($"Checking {SourceFile}");
        Console.WriteLine($"Finding all record locations");
        using var env = Utility.GetGameEnvironmentState(Release, SourceFile);
        ILoadOrderGetter<IModFlagsGetter> lo = new LoadOrder<IModFlagsGetter>(
            env.LoadOrder.ListedOrder.ResolveAllModsExist());
        var locs = RecordLocator.GetLocations(
            SourceFile,
            Release,
            lo,
            new RecordInterest(interestingTypes: new []
            {
                RecordTypes.REFR,
                RecordTypes.CELL
            }));

        var masterFlagsCompiler = new MasterFlagsLookupCompiler(
            new FileSystem(),
            new GameReleaseInjection(Release),
            new DataDirectoryInjection(env.DataFolderPath));

        using var stream = new MutagenBinaryReadStream(
            SourceFile, 
            ParsingMeta.Factory(BinaryReadParameters.Default with
            {
                MasterFlagsLookup = masterFlagsCompiler.ConstructFor(SourceFile)
            }, Release, SourceFile));

        var xtv2RecType = new RecordType("XTV2");
        
        Console.WriteLine($"Dumping data");
        List<KeyValue<uint, int>> list = new();
        foreach (var item in locs.ListedRecords)
        {
            stream.Position = item.Key;
            var majorFrame = stream.ReadMajorRecord();
            if (majorFrame.IsCompressed)
            {
                majorFrame = majorFrame.Decompress(out _);
            }

            int loc = 0;
            if (majorFrame.TryFindSubrecord(xtv2RecType, out var xtv2))
            {
                while (xtv2.ContentLength > loc)
                {
                    if (CheckIfIsFluff(xtv2.Content.Slice(loc), out var hasFormLink))
                    {
                        int cut = xtv2.ContentLength - loc;
                        list.Add(new KeyValue<uint, int>(majorFrame.FormID.FullId, cut));
                        break;
                    }

                    loc += hasFormLink ? 0x38 : 0x34;
                }
            }
        }
        
        Console.WriteLine("Key value pairs");
        foreach (var item in list)
        {
            Console.WriteLine($"  {{ 0x{item.Key:X}, 0x{item.Value:X} }},");
        }
    }

    private bool CheckIfIsFluff(ReadOnlyMemorySlice<byte> mem, out bool hasFormLink)
    {
        try
        {
            var starter = mem.Slice(0, 0x28);
            var flags = BinaryPrimitives.ReadInt32LittleEndian(mem.Slice(0x28, 4));
            hasFormLink = !Enums.HasFlag(flags, 4);
            if (!hasFormLink)
            {
                return false;
            }

            var ender = mem.Slice(0x2C, 12);
            if (starter.Any(b => b != 0)) return false;
            if (ender.Any(b => b != 0)) return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            hasFormLink = false;
            return true;
        }
        
        return true;
    }
}
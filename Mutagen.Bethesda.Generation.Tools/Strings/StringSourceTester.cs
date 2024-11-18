using CommandLine;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Analysis;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Plugins.Binary.Streams;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Records.Mapping;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using Noggog;
using RecordTypes = Mutagen.Bethesda.Skyrim.Internals.RecordTypes;

namespace Mutagen.Bethesda.Generation.Tools.Strings;

[Verb("string-source-tester")]
public class StringSourceTester
{
    [Option('f', "File", HelpText = "Path to a source file to analyze.  Will analyze environment load order if missing")]
    public ModPath SourceFile { get; set; } = ModPath.Empty;

    [Option('r', "Release", Required = true, HelpText = "GameRelease targeted")]
    public GameRelease Release { get; set; }

    [Option('k', "FormKey", Required = true, HelpText = "FormKey of record to analyze")]
    public required string FormKeyString { get; set; }

    [Option('t', "Type", Required = true, HelpText = "Type of record to analyze")]
    public required string TypeName { get; set; }
    
    [Option("HaltAfterRunning", Required = false, HelpText = "Waits for user input before closing")]
    public required bool HaltAfterRunning { get; set; }

    private FormKey FormKey => FormKey.Factory(FormKeyString);

    public void Execute()
    {
        if (!Release.ToCategory().HasLocalization())
        {
            Console.WriteLine("Game does not support localization.  No work to be done.");
            return;
        }

        if (!GetterTypeMapping.Instance.TryGetGetterType($"Mutagen.Bethesda.{Release.ToCategory()}.{TypeName}", out var targetType))
        {
            Console.WriteLine($"Could not find target type for: {TypeName}");
            return;
        }
        
        if (SourceFile == ModPath.Empty)
        {
            using var env = GameEnvironment.Typical.Construct(Release);
            var context = env.LinkCache.ResolveSimpleContext(FormKey, targetType);
            Test(Path.Combine(env.DataFolderPath, context.ModKey.FileName), env.DataFolderPath, targetType);
        }
        else
        {
            if (!CheckModIsLocalized(SourceFile, Release)) return;
            Test(SourceFile, Path.GetDirectoryName(SourceFile)!, targetType);
        }
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

    public void Test(
        ModPath modPath,
        DirectoryPath dataPath,
        Type targetType)
    {
        var locs = RecordLocator.GetLocations(modPath, Release, null);
        var loc = locs.GetRecord(FormKey);
        
        using var mod = ModInstantiator.ImportGetter(modPath, Release);
        var linkCache = mod.ToUntypedImmutableLinkCache();
        var rec = linkCache.Resolve(FormKey, targetType);
        if (rec is not INamedGetter named)
        {
            throw new ArgumentException("Command was given a record that was not Named");
        }
        
        Console.WriteLine($"Analyzing from winning override path: {modPath}");
        Console.WriteLine($"Mutagen Name field returned: {named.Name}");

        if (!mod.UsingLocalization)
        {
            Console.WriteLine("Mod was not localized.  String was embedded.");
            return;
        }
        
        using var stream = new MutagenBinaryReadStream(modPath, Release, null);
        stream.Position = loc.Location.Min;

        var majorFrame = stream.ReadMajorRecord();
        if (majorFrame.IsCompressed)
        {
            majorFrame = majorFrame.Decompress(out var _);
        }
        var full = majorFrame.FindSubrecord(RecordTypes.FULL);
        Console.WriteLine($"FULL record index: {full.AsUInt32()}");

        var stringsOverlay = StringsFolderLookupOverlay.TypicalFactory(
            Release,
            modPath.ModKey,
            dataPath,
            null);

        if (!stringsOverlay.TryLookup(StringsSource.Normal, Language.English, full.AsUInt32(), 
                out var str,
                out var sourcePath))
        {
            throw new ArgumentException($"Couldnt look up index {full.AsUInt32()}");
        }

        Console.WriteLine($"StringsOverlay lookup found:");
        Console.WriteLine($"  {str}");
        Console.WriteLine($"  {sourcePath}");

        if (HaltAfterRunning)
        {
            Console.WriteLine("DONE!  Press any key to exit");
            Console.ReadKey();
        }
    }
}
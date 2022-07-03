using System.Diagnostics;
using System.Reflection;
using CommandLine;
using Loqui;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;

namespace Mutagen.Bethesda.Generation.Tools.FormLinks;

/// <summary>
/// Confirms that there is no missing types included in FormLink interfaces.  It compares all formlinks and ensures that
/// if they point to an existing MajorRecord, that the typing defined in Mutagen allows them to connect.
/// </summary>
[Verb("formlink-inclusion-confirmation")]
public class FormLinkInclusionConfirmation
{
    [Option('f', "File", HelpText = "Path to a source file to analyze.  Will analyze environment load order if missing")]
    public ModPath SourceFile { get; set; } = ModPath.Empty;
    
    [Option('r', "Release", Required = true, HelpText = "GameRelease targeted")]
    public GameRelease Release { get; set; }
    
    [Option('e', "Exclusions", Required = false, HelpText = "File of major record formkeys to not analyze")]
    public string? ExclusionsPath { get; set; }

    public void Execute()
    {
        Stopwatch sw = new();
        sw.Start();
        using var env = Utility.GetGameEnvironmentState(Release, SourceFile);
        var modsToCheck = env.LoadOrder.ListedOrder
            .OnlyEnabledAndExisting()
            .Select(x => x.Mod)
            .NotNull()
            .ToList();

        HashSet<FormKey>? exclusions = null;
        if (ExclusionsPath != null && File.Exists(ExclusionsPath))
        {
            exclusions = File.ReadAllLines(ExclusionsPath).Select(l => FormKey.Factory(l)).ToHashSet();
        }

        Dictionary<Type, Dictionary<Type, Dictionary<ModKey, Dictionary<IFormLinkGetter, List<FormKey>>>>> missing = new();

        foreach (var mod in modsToCheck)
        {
            Console.WriteLine($"Checking {mod.ModKey}");
        }

        foreach (var mod in modsToCheck)
        {
            foreach (var context in mod.EnumerateMajorRecordSimpleContexts())
            {
                if (exclusions?.Contains(context.Record.FormKey) ?? false) continue;
                foreach (var link in context.Record.EnumerateFormLinks())
                {
                    // If null, skip
                    if (link.IsNull) continue;
                    // If found in typed constraints, skip
                    if (env.LinkCache.TryResolve(link.FormKey, link.Type, out _)) continue;
                    // If not found at all, skip
                    if (!env.LinkCache.TryResolve(link.FormKey, typeof(IMajorRecordGetter), out var linked)) continue;

                    lock (missing)
                    {
                        missing
                            .GetOrAdd(context.Record.GetType())
                            .GetOrAdd(linked.GetType())
                            .GetOrAdd(context.ModKey)
                            .GetOrAdd(context.Record.ToLinkFromRuntimeType())
                            .Add(link.FormKey);
                    }
                }
            }
        }

        Console.WriteLine($"Printing missing link summary:");
        foreach (var m in missing)
        {
            Console.WriteLine($"{LoquiRegistration.GetRegister(m.Key).ClassType.Name} had missing links:");
            foreach (var type in m.Value)
            {
                Console.WriteLine($"  {LoquiRegistration.GetRegister(type.Key).ClassType.Name}:");
                foreach (var rec in type.Value.SelectMany(x => x.Value).Take(5))
                {
                    Console.WriteLine($"    {rec.Key}");
                }
            }
        }

        Console.WriteLine($"Printing trimmed missing link detail:");
        foreach (var m in missing)
        {
            foreach (var type in m.Value)
            {
                var modKey = type.Value.First();
                var mod = env.LoadOrder[modKey.Key];
                var modCache = mod.Mod!.ToUntypedImmutableLinkCache();
                var formLink = modKey.Value.First();
                var rec = modCache.Resolve(formLink.Key);
                var targetKey = formLink.Value.First();
                PrintLocationOf(rec, targetKey, new Stack<string>());
            }
        }
        Console.WriteLine($"Done {sw.ElapsedMilliseconds}ms");
    }

    private bool PrintLocationOf(object rec, FormKey formKey, Stack<string> access)
    {
        foreach (var prop in rec.GetType().GetProperties(BindingFlags.Public))
        {
            if (prop.PropertyType.InheritsFrom(typeof(IFormLinkGetter)))
            {
                var val = prop.GetValue(rec) as IFormLinkGetter;
                if (val?.FormKeyNullable == formKey)
                {
                    access.Push(prop.Name);
                    return true;
                }
            }
            else if (prop.PropertyType.InheritsFrom(typeof(IEnumerable<IFormLinkGetter>)))
            {
                var val = prop.GetValue(rec) as IEnumerable<IFormLinkGetter>;
                foreach (var fk in val.EmptyIfNull())
                {
                    if (fk?.FormKeyNullable == formKey)
                    {
                        access.Push(prop.Name);
                        return true;
                    }
                }
            }
            else if (prop.PropertyType.InheritsFrom(typeof(ILoquiObject)))
            {
                access.Push(prop.Name);
                if (PrintLocationOf(rec, formKey, access)) return true;
            }
        }

        return false;
    }
}
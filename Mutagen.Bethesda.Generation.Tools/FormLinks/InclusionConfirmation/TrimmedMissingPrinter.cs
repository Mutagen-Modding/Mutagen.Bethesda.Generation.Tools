using System.Reflection;
using Loqui;
using Mutagen.Bethesda.Generation.Tools.Common;
using Mutagen.Bethesda.Plugins;
using Noggog;

namespace Mutagen.Bethesda.Generation.Tools.FormLinks.InclusionConfirmation;

public class TrimmedMissingPrinter
{
    private readonly EnvironmentProvider _environmentProvider;
    
    public TrimmedMissingPrinter(EnvironmentProvider environmentProvider)
    {
        _environmentProvider = environmentProvider;
    }
    
    public void Print(SourceRecordsToMissingLinks missing)
    {
        Console.WriteLine($"Printing trimmed missing link detail:");
        foreach (var m in missing.SourceToMissingLinks)
        {
            foreach (var type in m.Value.MissingLinkToMods)
            {
                var modKey = type.Value.Mods.First();
                var mod = _environmentProvider.Environment.LoadOrder[modKey.Key];
                var modCache = mod.Mod!.ToUntypedImmutableLinkCache();
                var formLink = modKey.Value.SourceToMissingLinks.First();
                var rec = modCache.Resolve(formLink.Key);
                var targetKey = formLink.Value.First();
                PrintLocationOf(rec, targetKey, new Stack<string>());
            }
        }
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
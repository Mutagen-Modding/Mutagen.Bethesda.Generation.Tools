using Mutagen.Bethesda.Json;
using Mutagen.Bethesda.Plugins;
using Newtonsoft.Json;
using Noggog;

namespace Mutagen.Bethesda.Generation.Tools.FormLinks.InclusionConfirmation;

public record Exclusion(IFormLinkGetter SourceRecord, FormKey? MissingLink);

public record Exclusions(List<Exclusion> Items);

public class ExclusionsProvider
{
    private readonly Dictionary<ModKey, Dictionary<IFormLinkIdentifier, HashSet<FormKey>?>> _exclusions = new();

    public ExclusionsProvider(FormLinkInclusionConfirmation settings)
    {
        var jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented
        };
        jsonSettings.AddMutagenConverters();
        if (settings.ExclusionsPath == null) return;
        if (File.Exists(settings.ExclusionsPath))
        {
            var exclusions = JsonConvert.DeserializeObject<Exclusions>(File.ReadAllText(settings.ExclusionsPath), jsonSettings)!;
            Set(ModKey.Null, exclusions);
        }
        else if (Directory.Exists(settings.ExclusionsPath))
        {
            foreach (var f in Directory.EnumerateFiles(settings.ExclusionsPath))
            {
                var exclusions = JsonConvert.DeserializeObject<Exclusions>(File.ReadAllText(f), jsonSettings)!;
                var fileName = Path.GetFileNameWithoutExtension(f);
                Set(ModKey.FromNameAndExtension(fileName), exclusions);
            }
        }
    }

    private void Set(ModKey modKey, Exclusions exclusions)
    {
        var dict = _exclusions.GetOrAdd(modKey);
        foreach (var exl in exclusions.Items)
        {
            if (!dict.TryGetValue(exl.SourceRecord, out var set))
            {
                set = exl.MissingLink == null ? null : new HashSet<FormKey>();
                dict[exl.SourceRecord] = set;
            }

            if (set != null && exl.MissingLink != null)
            {
                set.Add(exl.MissingLink.Value);
            }
        }
    }
    
    public bool Exclude(ModKey modKey, IFormLinkIdentifier sourceLink)
    {
        if (_exclusions.Count == 0) return false;
        if (!_exclusions.TryGetValue(modKey, out var exclusions)
            && !_exclusions.TryGetValue(ModKey.Null, out exclusions))
        {
            return false;
        }

        if (!exclusions.TryGetValue(sourceLink, out var missingLinks)) return false;
        return missingLinks == null;
    }
    
    public bool Exclude(ModKey modKey, IFormLinkIdentifier sourceLink, FormKey missingKey)
    {
        if (_exclusions.Count == 0) return false;
        if (!_exclusions.TryGetValue(modKey, out var exclusions)
            && !_exclusions.TryGetValue(ModKey.Null, out exclusions))
        {
            return false;
        }

        if (!exclusions.TryGetValue(sourceLink, out var missingLinks)) return false;

        return missingLinks?.Contains(missingKey) ?? true;
    }
}
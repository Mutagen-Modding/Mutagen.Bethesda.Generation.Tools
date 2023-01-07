using Mutagen.Bethesda.Json;
using Mutagen.Bethesda.Plugins;
using Newtonsoft.Json;
using Noggog;

namespace Mutagen.Bethesda.Generation.Tools.FormLinks.InclusionConfirmation;

public record Exclusion(IFormLinkGetter SourceRecord, FormKey MissingLink);

public record Exclusions(List<Exclusion> Items);

public class ExclusionsProvider
{
    private readonly Dictionary<ModKey, Dictionary<IFormLinkIdentifier, HashSet<FormKey>>> _exclusions = new();

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
            dict.GetOrAdd(exl.SourceRecord).Add(exl.MissingLink);
        }
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

        return missingLinks.Contains(missingKey);
    }
}
using Mutagen.Bethesda.Plugins;

namespace Mutagen.Bethesda.Generation.Tools.FormLinks.InclusionConfirmation;

public class ExclusionsProvider
{
    private readonly HashSet<FormKey>? _exclusions;
    
    public ExclusionsProvider(FormLinkInclusionConfirmation settings)
    {
        if (settings.ExclusionsPath != null && File.Exists(settings.ExclusionsPath))
        {
            _exclusions = File.ReadAllLines(settings.ExclusionsPath)
                .Select(l => FormKey.Factory(l))
                .ToHashSet();
        }
    }
    
    public bool Exclude(FormKey formKey)
    {
        return _exclusions?.Contains(formKey) ?? false;
    }
}
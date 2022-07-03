using Mutagen.Bethesda.Generation.Tools.Common;

namespace Mutagen.Bethesda.Generation.Tools.FormLinks.InclusionConfirmation;

public class Executor
{
    private readonly ModsToCheckProvider _modsToCheckProvider;
    private readonly MissingLocator _missingLocator;
    private readonly MissingPrinter _missingPrinter;
    private readonly TrimmedMissingPrinter _trimmedMissingPrinter;

    public Executor(
        ModsToCheckProvider modsToCheckProvider,
        MissingLocator missingLocator,
        MissingPrinter missingPrinter,
        TrimmedMissingPrinter trimmedMissingPrinter)
    {
        _modsToCheckProvider = modsToCheckProvider;
        _missingLocator = missingLocator;
        _missingPrinter = missingPrinter;
        _trimmedMissingPrinter = trimmedMissingPrinter;
    }

    public void Execute()
    {
        foreach (var mod in _modsToCheckProvider.Mods)
        {
            Console.WriteLine($"Checking {mod.ModKey}");
        }

        var missing = _missingLocator.GetMissing();

        _missingPrinter.Print(missing);
        _trimmedMissingPrinter.Print(missing);
    }
}
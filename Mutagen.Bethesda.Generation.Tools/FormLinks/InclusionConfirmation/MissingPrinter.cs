using Loqui;

namespace Mutagen.Bethesda.Generation.Tools.FormLinks.InclusionConfirmation;

public class MissingPrinter
{
    public void Print(SourceRecordsToMissingLinks missing)
    {
        Console.WriteLine($"Printing missing link summary:");
        foreach (var m in missing.SourceToMissingLinks)
        {
            Console.WriteLine($"{LoquiRegistration.GetRegister(m.Key).ClassType.Name} had missing links:");
            foreach (var type in m.Value.MissingLinkToMods)
            {
                Console.WriteLine($"  {LoquiRegistration.GetRegister(type.Key).ClassType.Name}:");
                foreach (var rec in type.Value.Mods.Values.SelectMany(x => x.SourceToMissingLinks).Take(5))
                {
                    Console.WriteLine($"    {rec.Key}");
                }
            }
        }
    }
}
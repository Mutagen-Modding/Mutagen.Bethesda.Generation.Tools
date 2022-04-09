using CommandLine;
using Mutagen.Bethesda.Generation.Tools.FormLinks;
using Mutagen.Bethesda.Generation.Tools.Strings;
using Mutagen.Bethesda.Generation.Tools.XEdit.Enum;

try
{
    var parser = new Parser();
    return await parser.ParseArguments(
            args,
            typeof(RunXEditEnumConverter),
            typeof(StringMappingFisher),
            typeof(FormLinkTypeFisher))
        .MapResult(
            async (RunXEditEnumConverter xEditEnum) =>
            {
                await xEditEnum.Execute();
                return 0;
            },
            async (StringMappingFisher fisher) =>
            {
                fisher.Execute();
                return 0;
            },
            async (FormLinkTypeFisher fisher) =>
            {
                fisher.Execute();
                return 0;
            },
            async _ =>
            {
                Console.Error.WriteLine(
                    $"Could not parse arguments into an executable command: {string.Join(' ', args)}");
                return -1;
            });
}
catch (Exception ex)
{
    System.Console.Error.WriteLine(ex);
    return -1;
}
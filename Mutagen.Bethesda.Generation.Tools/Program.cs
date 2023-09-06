using CommandLine;
using Mutagen.Bethesda.Generation.Tools.ContentAnalyzers;
using Mutagen.Bethesda.Generation.Tools.FormLinks;
using Mutagen.Bethesda.Generation.Tools.FormLinks.InclusionConfirmation;
using Mutagen.Bethesda.Generation.Tools.Strings;
using Mutagen.Bethesda.Generation.Tools.XEdit;
using Mutagen.Bethesda.Generation.Tools.XEdit.Enum;

try
{
    var parser = new Parser();
    return await parser.ParseArguments(
            args,
            typeof(RunXEditEnumConverter),
            typeof(RunXEditConditionFunctionGenerator),
            typeof(StringMappingFisher),
            typeof(ConditionFormLinkTypeFisher),
            typeof(FormLinkTypeFisher),
            typeof(FormLinkInclusionConfirmation),
            typeof(DumpSubrecords),
            typeof(AnalyzeSubrecordContent))
        .MapResult(
            async (RunXEditEnumConverter x) =>
            {
                await x.Execute();
                return 0;
            },
            async (RunXEditConditionFunctionGenerator x) =>
            {
                await x.Execute();
                return 0;
            },
            async (StringMappingFisher x) =>
            {
                x.Execute();
                return 0;
            },
            async (FormLinkTypeFisher x) =>
            {
                x.Execute();
                return 0;
            },
            async (FormLinkInclusionConfirmation x) =>
            {
                x.Execute();
                return 0;
            },
            async (ConditionFormLinkTypeFisher x) =>
            {
                x.Execute();
                return 0;
            },
            async (DumpSubrecords x) =>
            {
                x.Execute();
                return 0;
            },
            async (AnalyzeSubrecordContent x) =>
            {
                x.Execute();
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
using CommandLine;
using Noggog;

namespace Mutagen.Bethesda.Generation.Tools.XEdit.Enum;

[Verb("xedit-enum-convert", HelpText = "Converts an enum for Functions defined in xEdit to a C# enum")]
public class RunXEditEnumConverter
{
    [Option('s', "SourceFile", Required = true, HelpText = "Path to a source file with the xEdit copied into it")]
    public FilePath SourceFile { get; set; }
        
    [Option('o', "OutputFile", Required = true, HelpText = "Where to write out the results")]
    public FilePath OutputFile { get; set; }

    [Option('t', "Type", Required = false)]
    public EnumType Type { get; set; } = EnumType.Normal;

    public async Task Execute()
    {
        switch (Type)
        {
            case EnumType.Normal:
                EnumConverter.Convert(SourceFile, OutputFile);
                break;
            case EnumType.Curly:
                CurlyEnumConverter.Convert(SourceFile, OutputFile);
                break;
            case EnumType.Function:
                FunctionEnumGenerator.Convert(SourceFile, OutputFile);
                break;
            case EnumType.RecordType:
                RecordTypeEnumConverter.Convert(SourceFile, OutputFile);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public enum EnumType
{
    Normal,
    Curly,
    Function,
    RecordType,
}
using Noggog;
using Noggog.StructuredStrings;
using Noggog.StructuredStrings.CSharp;

namespace Mutagen.Bethesda.Generation.Tools.XEdit.Enum;

/// <summary>
/// Takes a hand-extracted xEdit snippet of the condition function enum and translates to a C# enum output
/// </summary>
public static class FunctionEnumGenerator
{
    public const string IndexStr = "Index:";
    public const string NameStr = "Name: '";
    
    /// <summary>
    /// Expects files with lines like
    /// (Index:   0; Name: 'GetWantBlocking'),    //   0
    /// </summary>
    public static void Convert(FilePath source, FilePath output)
    {
        StructuredStringBuilder sb = new();
        sb.AppendLine("public enum Function");
        using (sb.CurlyBrace())
        {
            foreach (var line in new ConditionFunctionParser().ReadFunctions(source))
            {
                sb.AppendLine($"{line.Name} = {line.Index},");
            }
        }
        sb.AppendLine();

        if (File.Exists(output))
        {
            File.Delete(output);
        }

        using var outputStream = new StreamWriter(File.OpenWrite(output));
        outputStream.Write(sb.ToString());
    }
}
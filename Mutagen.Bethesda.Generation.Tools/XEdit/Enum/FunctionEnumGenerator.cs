using Noggog;
using Noggog.StructuredStrings;
using Noggog.StructuredStrings.CSharp;

namespace Mutagen.Bethesda.Generation.Tools.XEdit.Enum;

/// <summary>
/// Takes a hand-extracted xEdit snippet of the condition function enum and translates to a C# enum output
/// </summary>
public static class FunctionEnumGenerator
{
    private const string IndexStr = "Index:";
    private const string NameStr = "Name: '";
    
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
            foreach (var line in File.ReadLines(source))
            {
                var span = line.AsSpan();
                span = EnumConverter.SkipPast(span, IndexStr);
            
                var semiColonIndex = span.IndexOf(";");
                if (semiColonIndex == -1)
                {
                    throw new ArgumentException();
                }

                if (!int.TryParse(span.Slice(0, semiColonIndex), out var i))
                {
                    throw new ArgumentException();
                }

                span = EnumConverter.SkipPast(span, NameStr);

                var name = span.Slice(0, span.IndexOf('\'')).ToString();

                name = EnumConverter.CleanName(name);
                
                sb.AppendLine($"{name} = {i},");
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
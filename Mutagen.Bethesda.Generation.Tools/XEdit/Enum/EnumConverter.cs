using System.Globalization;
using Noggog;
using Noggog.StructuredStrings;
using Noggog.StructuredStrings.CSharp;

namespace Mutagen.Bethesda.Generation.Tools.XEdit.Enum;

public static class EnumConverter
{
    public static void Convert(FilePath source, FilePath output)
    {
        StructuredStringBuilder sb = new();
        sb.AppendLine("public enum EnumName");
        using (sb.CurlyBrace())
        {
            foreach (var line in File.ReadLines(source))
            {
                var span = line.AsSpan();

                var numberEndIndex = span.IndexOf(",");
                if (numberEndIndex == -1)
                {
                    throw new ArgumentException();
                }

                int i;

                var numberSpan = span.Slice(0, numberEndIndex).TrimStart().TrimEnd();
                bool hex = false;
                if (numberSpan.StartsWith("0x"))
                {
                    hex = true;
                    numberSpan = numberSpan.Slice(2);
                    if (!int.TryParse(numberSpan, NumberStyles.HexNumber, null, out i))
                    {
                        throw new ArgumentException();
                    }
                }
                else if (!int.TryParse(numberSpan, out i))
                {
                    throw new ArgumentException();
                }

                span = SkipPast(span, ", '");

                var name = span.Slice(0, span.IndexOf("\',")).ToString();

                if (name.Contains("Unknown")) continue;

                name = CleanName(name);

                sb.AppendLine($"{name} = {(hex ? $"0x{i:x}" : i)},");
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

    public static ReadOnlySpan<char> SkipPast(ReadOnlySpan<char> str, string target)
    {
        var index = str.IndexOf(target);
        if (index == -1)
        {
            throw new ArgumentException();
        }

        return str.Slice(index + target.Length);
    }

    public static string CleanName(string name)
    {
        return name
            .Replace(" ", string.Empty)
            .Replace("/", "Or")
            .Replace("-", string.Empty)
            .Replace("'", string.Empty)
            .Replace("\\", "Or");
    }
}

using System.Globalization;
using Noggog;
using Noggog.StructuredStrings;
using Noggog.StructuredStrings.CSharp;

namespace Mutagen.Bethesda.Generation.Tools.XEdit.Enum;

public static class CurlyEnumConverter
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
                span = Utility.SkipPast(span, "{");

                var numberEndIndex = span.IndexOf("}");
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

                span = Utility.SkipPast(span, "} '");

                var name = span.Slice(0, span.IndexOf('\'')).ToString();

                if (name.Contains("Unknown")) continue;

                name = EnumConverter.CleanName(name);

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
}

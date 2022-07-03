using Mutagen.Bethesda.Plugins;
using Noggog;
using Noggog.StructuredStrings;
using Noggog.StructuredStrings.CSharp;

namespace Mutagen.Bethesda.Generation.Tools.XEdit.Enum;

public class RecordTypeEnumConverter
{
    public static void Convert(FilePath source, FilePath output)
    {
        HashSet<RecordType> types = new();
        StructuredStringBuilder sb = new();
        sb.AppendLine("public enum EnumName");
        using (sb.CurlyBrace())
        {
            foreach (var line in File.ReadLines(source))
            {
                var span = line.AsSpan();
                span = EnumConverter.SkipPast(span, "Sig2Int(");

                var recordTypeIndex = span.IndexOf(")");
                if (recordTypeIndex == -1)
                {
                    throw new ArgumentException();
                }

                var recordTypeSpan = span.Slice(0, recordTypeIndex).TrimStart().TrimEnd();
                if (recordTypeSpan.Length != 4)
                {
                    throw new ArgumentException();
                }

                var recordType = new RecordType(recordTypeSpan);

                span = EnumConverter.SkipPast(span, ", '");

                var name = span.Slice(0, span.IndexOf('\'')).ToString();

                if (name.Contains("Unknown")) continue;

                name = EnumConverter.CleanName(name);

                sb.AppendLine($"{name} = RecordTypeInts.{recordType.Type},");
                types.Add(recordType);
            }
        }
        sb.AppendLine();
        sb.AppendLine();

        foreach (var t in types.OrderBy(x => x.Type))
        {
            sb.AppendLine($"<AdditionalContainedRecordType>{t}</AdditionalContainedRecordType>");
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
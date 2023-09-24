using System.Diagnostics.CodeAnalysis;
using DynamicData;
using Mutagen.Bethesda.Generation.Tools.XEdit.Enum;
using Noggog;

namespace Mutagen.Bethesda.Generation.Tools.XEdit;

public record ConditionFunction(string Name, int Index, string? Param1, string? Param2);

public class ConditionFunctionParser
{
    public const string IndexStr = "Index:";
    public const string NameStr = "Name: '";
    public const string Param1Str = "ParamType1:";
    public const string Param2Str = "ParamType2:";

    public IEnumerable<ConditionFunction> ReadFunctions(FilePath source)
    {
        foreach (var line in File.ReadLines(source))
        {
            var span = RemoveComments(line).AsSpan();
            span = Utility.SkipPast(span, IndexStr);
            
            var semiColonIndex = span.IndexOf(";");
            if (semiColonIndex == -1)
            {
                throw new ArgumentException();
            }

            if (!int.TryParse(span.Slice(0, semiColonIndex), out var i))
            {
                throw new ArgumentException();
            }

            span = Utility.SkipPast(span, NameStr);

            var name = span.Slice(0, span.IndexOf('\'')).ToString();

            name = EnumConverter.CleanName(name);

            TryGetParamTypeStr(span, Param1Str, out var param1Str);
            TryGetParamTypeStr(span, Param2Str, out var param2Str);

            yield return new ConditionFunction(name, i, param1Str, param2Str);
        }
    }

    public string RemoveComments(string str)
    {
        int index = str.IndexOf("{");
        while (index != -1)
        {
            int endIndex = str.IndexOf("}", index);
            str = str.Substring(0, index) + str.Substring(endIndex + 1);
            index = str.IndexOf("{");
        }

        return str;
    }

    public bool TryGetParamTypeStr(ReadOnlySpan<char> span, string paramName, [MaybeNullWhen(false)] out string paramType)
    {
        var index = span.IndexOf(paramName);
        if (index == -1)
        {
            paramType = default;
            return false;
        }

        span = span.Slice(index + paramName.Length);

        var endIndex = span.IndexOf(';');
        if (endIndex == -1)
        {
            endIndex = span.IndexOf(')');
        }
        if (endIndex != -1)
        {
            span = span.Slice(0, endIndex);
        }

        span = span.Trim();
        paramType = span.ToString();
        return true;
    }
}
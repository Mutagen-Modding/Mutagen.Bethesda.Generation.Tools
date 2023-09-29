using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Linq;
using CommandLine;
using Mutagen.Bethesda.Generation.Tools.XEdit.Enum;
using Noggog;
using Noggog.StructuredStrings;
using Noggog.StructuredStrings.CSharp;

namespace Mutagen.Bethesda.Generation.Tools.XEdit;

[Verb("xedit-condition-function-processor", HelpText = "Processes condition function definitions and generates various code")]
public class RunXEditConditionFunctionGenerator
{
    public const string LoquiNs = "http://tempuri.org/LoquiSource.xsd";
    
    [Option('s', "SourceFile", Required = true, HelpText = "Path to a source file with the xEdit copied into it")]
    public FilePath SourceFile { get; set; }
        
    [Option('c', "GameCategory", Required = true, HelpText = "What game category is being targeted")]
    public GameCategory GameCategory { get; set; }
        
    [Option('o', "OutputFolder", Required = true, HelpText = "Where to write out the results")]
    public DirectoryPath OutputFolder { get; set; }
    
    [Option('r', "ReferenceOutput", Required = false, HelpText = "Old output to reference")]
    public FilePath ReferencePath { get; set; }
    
    public async Task Execute()
    {
        Directory.CreateDirectory(OutputFolder);
        
        var enumGen = new RunXEditEnumConverter()
        {
            Type = EnumType.Function,
            SourceFile = SourceFile,
            OutputFile = Path.Combine(OutputFolder, "Function.cs")
        };

        await enumGen.Execute();
        ConditionFunction[] functions = new ConditionFunctionParser().ReadFunctions(SourceFile).ToArray();
        GenerateParameterType(functions, Path.Combine(OutputFolder, "ParameterType.cs"));
        GenerateGetParameterTypes(functions, Path.Combine(OutputFolder, "GetParameterTypes.cs"));
        GenerateConditionDefinitions(functions, Path.Combine(OutputFolder, "ConditionFunctionDatas.xml"));
        GenerateConditionSwitch(functions, Path.Combine(OutputFolder, "ConditionSwitch.cs"));
        GenerateConditionCustomCode(functions, Path.Combine(OutputFolder, "ConditionClasses"));
    }

    public static void GenerateParameterType(IEnumerable<ConditionFunction> source, FilePath output)
    {
        StructuredStringBuilder sb = new();
        var set = new HashSet<string>();
        sb.AppendLine("public enum ParameterType");
        using (sb.CurlyBrace())
        {
            sb.AppendLine("None,");
            var outputItems = new HashSet<string>();
            foreach (var line in source)
            {
                var toDo = (string? s) =>
                {
                    if (s == null) return;
                    if (set.Add(s))
                    {
                        outputItems.Add(GetParamTypeString(s));
                    }
                };

                toDo(line.Param1);
                toDo(line.Param2);
            }

            foreach (var item in outputItems.OrderBy(x => x))
            {
                sb.AppendLine($"{item},");
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

    private static string GetParamTypeString(string? str)
    {
        if (str == null) return "None";

        return str.TrimStart("pt");
    }

    public string GetClassName(ConditionFunction function)
    {
        return $"{function.Name}ConditionData";
    }

    public void GenerateGetParameterTypes(IEnumerable<ConditionFunction> source, FilePath output)
    {
        StructuredStringBuilder sb = new();
        sb.AppendLine("public static (ParameterType First, ParameterType Second) GetParameterTypes(Function function)");
        using (sb.CurlyBrace())
        {
            sb.AppendLine("return ((ushort)function) switch");
            using (sb.CurlyBrace())
            {
                foreach (var line in source)
                {
                    if (line.Param1 == null && line.Param2 == null) continue;
                    sb.AppendLine($"{line.Index} => (ParameterType.{GetParamTypeString(line.Param1)}, ParameterType.{GetParamTypeString(line.Param2)}),");
                }
                sb.AppendLine("_ => (ParameterType.None, ParameterType.None),");
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

    private bool ShouldSkip(ConditionFunction function)
    {
        return function.Name == "GetEventData";
    }
    
    public void GenerateConditionDefinitions(IEnumerable<ConditionFunction> source, FilePath output)
    {
        var objs = new List<object>();
        foreach (var function in source)
        {
            if (ShouldSkip(function)) continue;
            
            var paramElements = new List<XElement>();
            if (ConvertParamToXElement(function.Param1, "FirstParameter", GameCategory, out var elem))
            {
                if (IsStringParam(function.Param1))
                {
                    paramElements.Add(
                        new XElement(
                            XName.Get("Int32", LoquiNs),
                            new XAttribute("name", "FirstUnusedIntParameter")));
                    paramElements.Add(elem);
                }
                else
                {
                    paramElements.Add(elem);
                    paramElements.Add(
                        new XElement(
                            XName.Get("String", LoquiNs),
                            new XAttribute("name", "FirstUnusedStringParameter"),
                            new XAttribute("nullable", "true"),
                            new XAttribute("binary", "NoGeneration")));
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            if (ConvertParamToXElement(function.Param2, "SecondParameter", GameCategory, out elem))
            {
                if (IsStringParam(function.Param2))
                {
                    paramElements.Add(
                        new XElement(
                            XName.Get("Int32", LoquiNs),
                            new XAttribute("name", "SecondUnusedIntParameter")));
                    paramElements.Add(elem);
                }
                else
                {
                    paramElements.Add(elem);
                    paramElements.Add(
                        new XElement(
                            XName.Get("String", LoquiNs),
                            new XAttribute("name", "SecondUnusedStringParameter"),
                            new XAttribute("nullable", "true"),
                            new XAttribute("binary", "NoGeneration")));
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            
            var className = GetClassName(function);
            
            objs.Add(
                new XElement(XName.Get("Object", LoquiNs),
                    new XAttribute("name", className),
                    new XAttribute("objType", "Subrecord"),
                    new XAttribute("baseClass", "ConditionData"),
                    new XAttribute("binaryOverlay", "NoGeneration"),
                    new XElement(XName.Get("Fields", LoquiNs),
                        paramElements)));
        }
        
        var xml = new XElement(
            XName.Get("Loqui", LoquiNs),
            objs.ToArray());

        using var f = File.Create(output);
        using var writer = XmlTextWriter.Create(f, new XmlWriterSettings()
        {
            Indent = true
        });
        xml.WriteTo(writer);
    }

    private bool IsStringParam(string? paramStr)
    {
        return paramStr is "ptVariableName" or "ptString";
    }

    private bool ConvertParamToXElement(
        string? paramLine,
        string fieldName,
        GameCategory category,
        [MaybeNullWhen(false)] out XElement fieldLine)
    {
        switch (paramLine)
        {
            case "ptActor":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "PlacedNpc"));
                break;
            case "ptActorBase":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "Npc"));
                break;
            case "ptObjectReference":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XElement(XName.Get("Interface", LoquiNs), "IPlacedSimple"));
                break;
            case "ptAxis":
                fieldLine = new XElement(
                    XName.Get("Enum", LoquiNs),
                    new XAttribute("enumName", "Axis"));
                break;
            case "ptPronoun":
                fieldLine = new XElement(
                    XName.Get("Enum", LoquiNs),
                    new XAttribute("enumName", "Pronoun"));
                break;
            case "ptActorValue" when category is GameCategory.Fallout4:
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "ActorValueRecord"));
                break;
            case "ptActorValue":
                fieldLine = new XElement(
                    XName.Get("Enum", LoquiNs),
                    new XAttribute("enumName", "ActorValue"));
                break;
            case "ptInventoryObject":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XElement(XName.Get("Interface", LoquiNs), "IItem"));
                break;
            case "ptQuest":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "Quest"));
                break;
            case "ptFaction":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "Faction"));
                break;
            case "ptCell":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "Cell"));
                break;
            case "ptClass":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "Class"));
                break;
            case "ptRace":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "Race"));
                break;
            case "ptSex":
                fieldLine = new XElement(
                    XName.Get("Enum", LoquiNs),
                    new XAttribute("enumName", "MaleFemaleGender"));
                break;
            case "ptReferencableObject":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XElement(XName.Get("Interface", LoquiNs), "IPlaceableObject"));
                break;
            case "ptGlobal":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "Global"));
                break;
            case "ptRegion":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "Region"));
                break;
            case "ptWeather":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "Weather"));
                break;
            case "ptPackage":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "Package"));
                break;
            case "ptFurniture":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "Furniture"));
                break;
            case "ptAcousticSpace":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "AcousticSpace"));
                break;
            case "ptMagicEffect":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "MagicEffect"));
                break;
            case "ptMagicItem":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "Spell"));
                break;
            case "ptScene":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "Scene"));
                break;
            case "ptLocation":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "Location"));
                break;
            case "ptFormList":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "FormList"));
                break;
            case "ptOwner":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XElement(XName.Get("Interface", LoquiNs), "IOwner"));
                break;
            case "ptSnapTemplateNode":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "SnapTemplateNode"));
                break;
            case "ptForm":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", $"{category}MajorRecord"));
                break;
            case "ptWorldSpace":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "Worldspace"));
                break;
            case "ptKeyword":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "Keyword"));
                break;
            case "ptShout":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "Shout"));
                break;
            case "ptVoiceType":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "VoiceType"));
                break;
            case "ptEncounterZone":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", category == GameCategory.Starfield ? "StarfieldMajorRecord" : "EncounterZone"));
                break;
            case "ptSpeechChallenge":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "SpeechChallenge"));
                break;
            case "ptPlanet":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "Planet"));
                break;
            case "ptDamageType":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "DamageType"));
                break;
            case "ptVariableName":
            case "ptString":
                fieldLine = new XElement(
                    XName.Get("String", LoquiNs),
                    new XAttribute("nullable", "true"),
                    new XAttribute("binary", "NoGeneration"));
                break;
            case "ptPerk":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "Perk"));
                break;
            case "ptIdleForm":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "IdleAnimation"));
                break;
            case "ptEquipType":
                fieldLine = new XElement(
                    XName.Get("Enum", LoquiNs),
                    new XAttribute("enumName", "EquipTypeFlag"));
                break;
            case "ptKnowable":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XElement(XName.Get("Interface", LoquiNs), "IKnowable"));
                break;
            case "ptAssociationType":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", category == GameCategory.Starfield ? "StarfieldMajorRecord": "AssociationType"));
                break;
            case "ptResource":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "Resource"));
                break;
            case "ptRefType":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "LocationReferenceType"));
                break;
            case "ptConditionForm":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "ConditionRecord"));
                break;
            case "ptResearchProject":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XAttribute("refName", "ResearchProject"));
                break;
            case "ptEventData":
                fieldLine = new XElement(
                    XName.Get("FormLinkOrIndex", LoquiNs),
                    new XElement(XName.Get("Interface", LoquiNs), "IEventDataTarget"));
                break;
            case "ptInteger":
            case "ptInteger{Alt?}":
            case "ptQuestStage":
            case null:
                // ToDo
                // Unknown fields
                // Fill out with proper field types when known
            case "ptAlias":
            case "ptFormType":
            case "ptMiscStat":
            case "ptPackdata":
            case "ptVATSValueFunction":
            case "ptVATSValueParam":
            case "ptAlignment":
            case "ptCriticalStage":
            case "ptCastingSource":
            case "ptEvent":
            case "ptWardState":
            case "ptFurnitureAnim":
            case "ptFurnitureEntry":
            case "ptAdvanceAction":
            case "ptCrimeType":
            case "ptDamageCauseType":
            case "ptBiomeMask":
            case "ptPerkCategory":
            case "ptPerkSkillGroupComparison":
            case "ptPerkSkillGroup":
            case "ptReactionType":
            case "ptLimbCategory":
                fieldLine = new XElement(XName.Get("Int32", LoquiNs));
                break;
            case "ptFloat":
                fieldLine = new XElement(XName.Get("Float", LoquiNs));
                break;
            default:
                throw new NotImplementedException();
        }
        
        fieldLine.Add(new XAttribute("name", fieldName));
        return true;
    }

    public void GenerateConditionSwitch(IEnumerable<ConditionFunction> source, FilePath output)
    {
        StructuredStringBuilder sb = new();
        sb.AppendLine("public ConditionData CreateFromBinary(MutagenFrame frame, ushort functionIndex)");
        using (sb.CurlyBrace())
        {
            sb.AppendLine("switch (functionIndex)");
            using (sb.CurlyBrace())
            {
                foreach (var function in source)
                {
                    sb.AppendLine($"case {function.Index}:");
                    using (sb.IncreaseDepth())
                    {
                        sb.AppendLine($"return {GetClassName(function)}.CreateFromBinary(frame);");
                    }
                }
                sb.AppendLine($"default:");
                using (sb.IncreaseDepth())
                {
                    sb.AppendLine($"return UnknownConditionData.CreateFromBinary(frame);");
                }
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

    public void GenerateConditionCustomCode(IEnumerable<ConditionFunction> source, DirectoryPath output)
    {
        if (output.Exists)
        {
            output.DeleteEntireFolder();
        }
        Directory.CreateDirectory(output);

        foreach (var item in source)
        {
            if (ShouldSkip(item)) continue;
            
            StructuredStringBuilder sb = new();
            var firstStr = IsStringParam(item.Param1) ? "FirstParameter" : "FirstUnusedStringParameter";
            var secondStr = IsStringParam(item.Param2) ? "SecondParameter" : "SecondUnusedStringParameter";

            var name = $"{item.Name}ConditionData";
            
            using (sb.Namespace($"Mutagen.Bethesda.{GameCategory}"))
            {
                using (var c = sb.Class(name))
                {
                    c.Partial = true;
                    c.Interfaces.Add("IConditionStringParameter");
                }
                using (sb.CurlyBrace())
                {
                    sb.AppendLine($"string? IConditionStringParameterGetter.FirstStringParameter => {firstStr};");
                    sb.AppendLine();

                    sb.AppendLine($"string? IConditionStringParameterGetter.SecondStringParameter => {secondStr};");
                    sb.AppendLine();
                    
                    sb.AppendLine("string? IConditionStringParameter.FirstStringParameter");
                    using (sb.CurlyBrace())
                    {
                        sb.AppendLine($"get => {firstStr};");
                        sb.AppendLine($"set => {firstStr} = value;");
                    }
                    sb.AppendLine();
                    
                    sb.AppendLine("string? IConditionStringParameter.SecondStringParameter");
                    using (sb.CurlyBrace())
                    {
                        sb.AppendLine($"get => {secondStr};");
                        sb.AppendLine($"set => {secondStr} = value;");
                    }
                    sb.AppendLine();
                
                    sb.AppendLine($"Condition.Function IConditionDataGetter.Function => Condition.Function.{item.Name};");
                    sb.AppendLine();
                }
                sb.AppendLine();
            }

            File.WriteAllText(Path.Combine(output, $"{name}.cs"), sb.ToString());
        }
    }
}
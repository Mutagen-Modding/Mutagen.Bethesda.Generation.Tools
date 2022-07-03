using System.Collections;
using System.Reflection;
using Loqui;
using Mutagen.Bethesda.Plugins;
using Noggog;

namespace Mutagen.Bethesda.Generation.Tools.FormLinks.InclusionConfirmation;

public class LocationRetriever
{
    public bool RetrieveLocationOf(object rec, FormKey formKey, Stack<string> access)
    {
        var getter = LoquiRegistration.GetRegister(rec.GetType()).GetterType;
        var props = getter.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in props)
        {
            if (prop.PropertyType.InheritsFrom(typeof(IFormLinkGetter)))
            {
                var val = prop.GetValue(rec) as IFormLinkGetter;
                if (val?.FormKeyNullable == formKey)
                {
                    access.Push(prop.Name);
                    return true;
                }
            }
            else if (prop.PropertyType.InheritsFrom(typeof(IEnumerable<IFormLinkGetter>)))
            {
                var val = prop.GetValue(rec) as IEnumerable<IFormLinkGetter>;
                if (val == null) continue;
                var list = val.ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    var fk = list[i];
                    if (fk?.FormKeyNullable == formKey)
                    {
                        access.Push($"{prop.Name}[{i}]");
                        return true;
                    }
                }
                
                foreach (var fk in val.EmptyIfNull().ToList())
                {
                    if (fk?.FormKeyNullable == formKey)
                    {
                        access.Push(prop.Name);
                        return true;
                    }
                }
            }
            else if (prop.PropertyType.InheritsFrom(typeof(ILoquiObject)))
            {
                access.Push(prop.Name);
                var val = prop.GetValue(rec);
                if (val == null)
                {
                    access.Pop();
                    continue;
                }
                if (RetrieveLocationOf(val, formKey, access)) return true;
                access.Pop();
            }
            else if (prop.PropertyType.IsGenericType
                     && prop.PropertyType.InheritsFrom(typeof(IEnumerable))
                     && prop.PropertyType.GenericTypeArguments.Length == 1
                     && prop.PropertyType.GenericTypeArguments[0].InheritsFrom(typeof(ILoquiObject)))
            {
                var val = prop.GetValue(rec) as IEnumerable<object>;
                if (val == null) continue;
                var list = val.ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    access.Push($"{prop.Name}[{i}]");
                    if (RetrieveLocationOf(item, formKey, access)) return true;
                    access.Pop();
                }
            }
        }

        return false;
    }
}
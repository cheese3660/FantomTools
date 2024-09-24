using System.Text;
using FantomTools.Fantom.Attributes;
using FantomTools.Fantom.Code;
using FantomTools.PodWriting;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom;

[PublicAPI]
public class Method
{
    public Type ParentType;
    public Flags Flags;
    public string Name;
    public List<MethodVariable> Variables = [];
    public IEnumerable<MethodVariable> Parameters => Variables.Where(x => x.IsParameter);
    public IEnumerable<MethodVariable> Locals => Variables.Where(x => !x.IsParameter);
    
    public TypeReference ReturnType = TypeReference.Void;
    public MethodBody Body;
    public List<FantomAttribute> Attributes = [];

    public byte MaxStack = 16;

    public MethodReference Reference => new(ParentType.Reference, Name, ReturnType,
        Parameters.Select(x => x.Type).ToArray());

    public Method(Type parent)
    {
        ParentType = parent;
        Name = "<unnamed method>";
        Body = new MethodBody(this);
    }

    public MethodVariable AddParameter(string parameterName, TypeReference? parameterType = null)
    {
        parameterType ??= TypeReference.Object;
        var index = 0;
        var found = false;
        
        // I want to be able to 
        for (var i = 0; i < Variables.Count; i++)
        {
            if (found || !Variables[i].IsParameter)
            {
                if (!found) index = i;
                found = true;
                Variables[i].Index += 1;
            }
        }
        Variables.Insert(index, new MethodVariable
        {
            Index = (ushort)index,
            Name = parameterName,
            Type = parameterType,
            IsParameter = true
        });
        return Variables[index];
    }

    public MethodVariable AddLocal(string localName, TypeReference? localType = null)
    {
        localType ??= TypeReference.Object;
        var index = Variables.Count;
        Variables.Add(new MethodVariable
        {
            Index = (ushort)index,
            Name = localName,
            Type = localType,
            IsParameter = false
        });
        return Variables[index];
    }
    
    public void AddAttribute(FantomAttribute attribute)
    {
        Attributes.Add(attribute);
    }

    public void RemoveAttribute(FantomAttribute attribute)
    {
        Attributes.Remove(attribute);
    }
    
    internal void LoadBody(FantomStreamReader reader)
    {
        Body.Read(reader);
    }
    
    // Helper functions
    public bool MatchesSignature(TypeReference returnType, params TypeReference[] argumentTypes)
    {
        if (returnType != ReturnType) return false;
        var p = Parameters.ToList();
        return argumentTypes.Length == p.Count && argumentTypes.Zip(p).Any(x => x.First != x.Second.Type);
    }
    
    public string Dump(bool dumpBody=false)
    {
        var sb = new StringBuilder();
        sb.Append(Flags.GetString());
        sb.Append(ReturnType);
        sb.AppendLine($" {Name}({string.Join(", ", Parameters.Select(x => $"{x.Type} {x.Name}"))}) [");
        sb.AppendLine($"    .maxstack {MaxStack}");
        foreach (var local in Locals)
        {
            sb.AppendLine($"    .local {local.Type} {local.Name}");
        }
        sb.AppendLine("]\n{");

        if (dumpBody)
        {
            sb.Append(Body.Dump());
        }
        else
        {
            sb.AppendLine("    /* Body has been omitted from the method dump */");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    public void Emit(FantomStreamWriter writer, FantomTables tables)
    {
        writer.WriteU16(tables.Names.Intern(Name));
        writer.WriteU32((uint)Flags);
        var retIntern = tables.TypeReferences.Intern(ReturnType);
        writer.WriteU16(retIntern); // Return type
        writer.WriteU16(retIntern); // Inherited return type (fantom doesn't use covariance?)
        writer.WriteU8(MaxStack);
        writer.WriteU8((byte)Parameters.Count());
        writer.WriteU8((byte)Locals.Count());
        foreach (var variable in Variables)
        {
            variable.Emit(writer, tables);
        }
        Body.Emit(writer, tables);
        writer.WriteU16((ushort)Attributes.Count);
        foreach (var attribute in Attributes)
        {
            attribute.Write(writer, tables);
        }
    }
}
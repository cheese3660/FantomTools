using System.Text;
using FantomTools.Fantom.Attributes;
using FantomTools.PodWriting;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom;

[PublicAPI]
public class Field(Type parentType, string name, TypeReference fieldType)
{
    public Type ParentType => parentType;
    public string Name = name;
    public TypeReference FieldType = fieldType;
    public Flags Flags = 0;
    public List<FantomAttribute> Attributes = [];
    
    public bool Static => Flags.HasFlag(Flags.Static);

    public string Dump()
    {
        var sb = new StringBuilder();
        sb.Append(Flags.GetString());
        sb.AppendLine($"{FieldType} {Name}");
        return sb.ToString();
    }

    public FieldReference Reference => new(ParentType, Name, FieldType);

    public void Emit(FantomStreamWriter writer, FantomTables tables)
    {
        writer.WriteU16(tables.Names.Intern(name));
        writer.WriteU32((uint)Flags);
        writer.WriteU16(tables.TypeReferences.Intern(FieldType));
        writer.WriteU16((ushort)Attributes.Count);
        foreach (var attribute in Attributes)
        {
            attribute.Write(writer, tables);
        }
    }
}
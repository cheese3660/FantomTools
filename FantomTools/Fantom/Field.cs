using System.Text;
using FantomTools.Fantom.Attributes;
using FantomTools.PodWriting;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom;

/// <summary>
/// Represents a field on a type in a pod
/// </summary>
/// <param name="parentType">The type the field is on</param>
/// <param name="name">The name of the field</param>
/// <param name="fieldType">The type of the field</param>
[PublicAPI]
public class Field(Type parentType, string name, TypeReference fieldType)
{
    /// <summary>
    /// The type the field is on
    /// </summary>
    public Type ParentType => parentType;
    /// <summary>
    /// The name of the field
    /// </summary>
    public string Name = name;
    /// <summary>
    /// The type of the field
    /// </summary>
    public TypeReference FieldType = fieldType;
    /// <summary>
    /// The flags on the field, public by default
    /// </summary>
    public Flags Flags = Flags.Public;
    /// <summary>
    /// The attributes upon the field
    /// </summary>
    public List<FantomAttribute> Attributes = [];
    
    /// <summary>
    /// Is the field static?
    /// </summary>
    public bool Static => Flags.HasFlag(Flags.Static);

    /// <summary>
    /// Creates a textual representation of the field
    /// </summary>
    /// <returns>The textual representation</returns>
    public string Dump()
    {
        var sb = new StringBuilder();
        sb.Append(Flags.GetString());
        sb.AppendLine($"{FieldType} {Name}");
        return sb.ToString();
    }

    /// <summary>
    /// Gets a FieldReference to the field
    /// </summary>
    public FieldReference Reference => new(ParentType, Name, FieldType);

    internal void Emit(FantomStreamWriter writer, FantomTables tables)
    {
        writer.WriteU16(tables.Names.Intern(Name));
        writer.WriteU32((uint)Flags);
        writer.WriteU16(tables.TypeReferences.Intern(FieldType));
        writer.WriteU16((ushort)Attributes.Count);
        foreach (var attribute in Attributes)
        {
            attribute.Write(writer, tables);
        }
    }
}
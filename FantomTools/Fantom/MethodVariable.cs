using FantomTools.Fantom.Attributes;
using FantomTools.PodWriting;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom;

/// <summary>
/// Represents a local/parameter in a method
/// </summary>
[PublicAPI]
public class MethodVariable
{
    internal ushort Index;
    /// <summary>
    /// The name of this variable
    /// </summary>
    public string Name = "";
    /// <summary>
    /// The type of this variable
    /// </summary>
    public TypeReference Type = TypeReference.Object;
    /// <summary>
    /// Is this variable a parameter?
    /// </summary>
    public readonly bool IsParameter;

    internal bool IsDuplicate;
    

    
    /// <summary>
    /// The attributes on this variable
    /// </summary>
    public List<FantomAttribute> Attributes = [];
    internal MethodVariable(bool isParameter)
    {
        IsParameter = isParameter;
    }
    
    internal void Emit(FantomStreamWriter writer, FantomTables tables)
    {
        writer.WriteU16(tables.Names.Intern(Name));
        writer.WriteU16(tables.TypeReferences.Intern(Type));
        writer.WriteU8((byte)(IsParameter ? 1 : 0));
        writer.WriteU16((ushort)Attributes.Count);
        foreach (var attr in Attributes)
        {
            attr.Write(writer, tables);
        }
    }
}
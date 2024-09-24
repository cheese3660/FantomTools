using FantomTools.Fantom;
using FantomTools.Utilities;

namespace FantomTools.PodWriting;

public class MethodReferenceTable(FantomTables tables) : FantomTable<MethodReference>
{
    protected override void WriteSingle(FantomStreamWriter writer, MethodReference value)
    {
        writer.WriteU16(tables.TypeReferences.Intern(value.ParentType));
        writer.WriteU16(tables.Names.Intern(value.Name));
        writer.WriteU16(tables.TypeReferences.Intern(value.ReturnType));
        writer.WriteU8((byte)value.Parameters.Length);
        foreach (var parameter in value.Parameters)
        {
            writer.WriteU16(tables.TypeReferences.Intern(parameter));
        }
    }
}
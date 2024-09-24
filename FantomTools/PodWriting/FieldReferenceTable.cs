using FantomTools.Fantom;
using FantomTools.Utilities;

namespace FantomTools.PodWriting;

internal class FieldReferenceTable(FantomTables tables) : FantomTable<FieldReference>
{
    protected override void WriteSingle(FantomStreamWriter writer, FieldReference value)
    {
        writer.WriteU16(tables.TypeReferences.Intern(value.Parent));
        writer.WriteU16(tables.Names.Intern(value.Name));
        writer.WriteU16(tables.TypeReferences.Intern(value.Type));
    }
}
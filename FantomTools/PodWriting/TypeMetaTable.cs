using FantomTools.Utilities;
using Type = FantomTools.Fantom.Type;

namespace FantomTools.PodWriting;

internal class TypeMetaTable(FantomTables tables) : FantomTable<Type>
{
    protected override void WriteSingle(BigEndianWriter writer, Type value)
    {
        value.EmitMeta(writer, tables);
    }
}
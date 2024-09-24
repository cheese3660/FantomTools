using FantomTools.Utilities;
using Type = FantomTools.Fantom.Type;

namespace FantomTools.PodWriting;

internal class TypeMetaTable(FantomTables tables) : FantomTable<Type>
{
    protected override void WriteSingle(FantomStreamWriter writer, Type value)
    {
        value.EmitMeta(writer, tables);
    }
}
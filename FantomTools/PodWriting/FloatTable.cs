using FantomTools.Utilities;

namespace FantomTools.PodWriting;

internal class FloatTable : FantomTable<double>
{
    protected override void WriteSingle(FantomStreamWriter writer, double value)
    {
        writer.WriteF64(value);
    }
}
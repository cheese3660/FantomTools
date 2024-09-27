using FantomTools.Utilities;

namespace FantomTools.PodWriting;

internal class IntegerTable : FantomTable<long>
{
    protected override void WriteSingle(BigEndianWriter writer, long value)
    {
        writer.WriteI64(value);
    }
}
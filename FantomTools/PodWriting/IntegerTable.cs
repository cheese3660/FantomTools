using FantomTools.Utilities;

namespace FantomTools.PodWriting;

public class IntegerTable : FantomTable<long>
{
    protected override void WriteSingle(FantomStreamWriter writer, long value)
    {
        writer.WriteI64(value);
    }
}
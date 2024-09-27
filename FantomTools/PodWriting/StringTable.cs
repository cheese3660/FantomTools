using FantomTools.Utilities;

namespace FantomTools.PodWriting;

internal class StringTable : FantomTable<string>
{
    protected override void WriteSingle(BigEndianWriter writer, string value)
    {
        writer.WriteUtf8(value);
    }
}
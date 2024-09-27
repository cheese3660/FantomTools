using FantomTools.Fantom.Attributes;
using FantomTools.InternalUtilities;
using FantomTools.Utilities;

namespace FantomTools.PodReading;

internal class AttributesReader
{
    public List<FantomAttribute> Attributes = [];

    public static AttributesReader Read(FantomStreamReader reader)
    {
        var numAttrs = reader.ReadU16();
        var attrs = new AttributesReader();
        for (var i = 0; i < numAttrs; i++)
        {
            attrs.Attributes.Add(FantomAttribute.Parse(reader));
        }
        return attrs;
    }
}
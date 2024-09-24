using FantomTools.PodWriting;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom.Attributes;

[PublicAPI]
public class FacetsAttribute : FantomAttribute
{
    
    public List<Facet> Facets = [];
    internal FacetsAttribute(string name, FantomStreamReader reader) : base(name)
    {
        reader.ReadU16();
        var count = reader.ReadU16();
        Facets.EnsureCapacity(count);
        for (var i = 0; i < count; i++)
        {
            Facets.Add(new Facet(reader.PodReader.TypeRefs[reader.ReadU16()].Reference, reader.ReadUtf8()));
        }
    }

    public FacetsAttribute(params Facet[] facets) : base("Facets")
    {
        Facets = facets.ToList();
    }

    public override void WriteBody(FantomStreamWriter writer, FantomTables tables)
    {
        using var dataStream = new MemoryStream();
        var dataWriter = new FantomStreamWriter(dataStream);
        dataWriter.WriteU16((ushort)Facets.Count);
        foreach (var facet in Facets)
        {
            dataWriter.WriteU16(tables.TypeReferences.Intern(facet.FacetType));
            dataWriter.WriteUtf8(facet.Value);
        }
        writer.WriteU16((ushort)dataStream.Length);
        writer.Stream.Write(dataStream.ToArray());
    }
}
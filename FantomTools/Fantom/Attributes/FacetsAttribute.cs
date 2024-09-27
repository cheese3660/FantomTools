using FantomTools.InternalUtilities;
using FantomTools.PodWriting;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom.Attributes;

/// <summary>
/// This attribute contains a list of facets applied to the slot/type
/// </summary>
[PublicAPI]
public class FacetsAttribute : FantomAttribute
{
    
    /// <summary>
    /// The list of facets applied to the slot/type
    /// </summary>
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

    /// <summary>
    /// Create a new FacetsAttribute from a list of facets
    /// </summary>
    /// <param name="facets">The facets to add to the attribute</param>
    public FacetsAttribute(params Facet[] facets) : base("Facets")
    {
        Facets = facets.ToList();
    }

    internal override void WriteBody(BigEndianWriter writer, FantomTables tables)
    {
        using var dataWriter = new FantomBufferStream(writer.Stream);
        dataWriter.WriteU16((ushort)Facets.Count);
        foreach (var facet in Facets)
        {
            dataWriter.WriteU16(tables.TypeReferences.Intern(facet.FacetType));
            dataWriter.WriteUtf8(facet.Value);
        }
    }
}
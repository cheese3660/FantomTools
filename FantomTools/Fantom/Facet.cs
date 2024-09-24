using JetBrains.Annotations;

namespace FantomTools.Fantom;

/// <summary>
/// Represents a facet on a type/field/method/parameter in fantom
/// </summary>
/// <param name="FacetType">The type of the facet</param>
/// <param name="Value">The value of the facet</param>
[PublicAPI]
public record struct Facet(TypeReference FacetType, string Value); 
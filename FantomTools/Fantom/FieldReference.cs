using JetBrains.Annotations;

namespace FantomTools.Fantom;

/// <summary>
/// Represents a reference to a fantom field
/// </summary>
/// <param name="Parent">The type the field is on</param>
/// <param name="Name">The name of the field</param>
/// <param name="Type">The type of the field</param>
[PublicAPI]
public readonly record struct FieldReference(TypeReference Parent, string Name, TypeReference Type)
{
    /// <summary>
    /// Creates a textual representation of the field reference
    /// </summary>
    /// <returns>The textual representation</returns>
    public override string ToString() => $"({Type}){Parent}.{Name}";

    /// <summary>
    /// Creates a field reference from a <see cref="Field"/> instance
    /// </summary>
    /// <param name="field">The field to reference</param>
    /// <returns>A FieldReference referencing the field</returns>
    public static implicit operator FieldReference(Field field) => field.Reference;
}
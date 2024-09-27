namespace FantomTools.Fantom.Code.Instructions;

/// <summary>
/// Represents an instruction that takes a type parameter
/// </summary>
public class TypeInstruction : Instruction
{
    /// <summary>
    /// The type parameter
    /// </summary>
    public TypeReference Value;

    /// <inheritdoc />
    public override ushort Size => 3;
}
namespace FantomTools.Fantom.Code.Instructions;

/// <summary>
/// Represents an instruction that takes 2 types as parameters
/// </summary>
public class TypePairInstruction : Instruction
{
    /// <summary>
    /// The first type parameter
    /// </summary>
    public TypeReference FirstType;
    /// <summary>
    /// The second type parameter
    /// </summary>
    public TypeReference SecondType;

    /// <inheritdoc />
    public override ushort Size => 5;
}
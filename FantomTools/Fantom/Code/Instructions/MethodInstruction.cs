namespace FantomTools.Fantom.Code.Instructions;

/// <summary>
/// Represents an instruction that takes a method reference as a parameter
/// </summary>
public class MethodInstruction : Instruction
{
    /// <summary>
    /// The method reference that this instruction takes
    /// </summary>
    public MethodReference Value;

    /// <inheritdoc />
    public override ushort Size => 3;
}
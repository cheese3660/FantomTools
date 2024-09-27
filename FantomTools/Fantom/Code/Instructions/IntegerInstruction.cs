namespace FantomTools.Fantom.Code.Instructions;

/// <summary>
/// Represents an instruction that takes an integer parameter
/// </summary>
public class IntegerInstruction : Instruction
{
    /// <summary>
    /// The integer parameter
    /// </summary>
    public long Value;

    /// <inheritdoc />
    public override ushort Size => 3;
}
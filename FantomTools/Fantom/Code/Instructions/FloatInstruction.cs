namespace FantomTools.Fantom.Code.Instructions;

/// <summary>
/// This represents an instruction that takes a floating point value as an argument
/// </summary>
public class FloatInstruction : Instruction
{
    /// <summary>
    /// The floating point value
    /// </summary>
    public double Value;

    /// <inheritdoc />
    public override ushort Size => 3;
}
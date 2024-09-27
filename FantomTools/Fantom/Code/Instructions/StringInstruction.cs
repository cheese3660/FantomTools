namespace FantomTools.Fantom.Code.Instructions;

/// <summary>
/// Represents an instruction that takes a string as a parameter
/// </summary>
public class StringInstruction : Instruction
{
    /// <summary>
    /// The string value that this instruction takes
    /// </summary>
    public string Value;

    /// <inheritdoc />
    public override ushort Size => 3;
}
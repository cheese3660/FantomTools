namespace FantomTools.Fantom.Code.Instructions;

/// <summary>
/// Represents an instruction that takes a jump target parameter
/// </summary>
public class JumpInstruction : Instruction
{
    /// <summary>
    /// The jump target, as another instruction
    /// </summary>
    public Instruction Target;

    /// <inheritdoc />
    public override ushort Size => 3;
}
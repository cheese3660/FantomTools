namespace FantomTools.Fantom.Code.Instructions;

/// <summary>
/// Represents an instruction on a register (a method variable)
/// </summary>
public class RegisterInstruction : Instruction
{
    /// <summary>
    /// The variable that this register instruction uses, if this is null it goes to the instance variable ("this")
    /// </summary>
    public MethodVariable? Value;

    /// <inheritdoc />
    public override ushort Size => 3;
}
namespace FantomTools.Fantom.Code.Instructions;

/// <summary>
/// Represents a switch case instruction
/// </summary>
public class SwitchInstruction : Instruction
{
    /// <summary>
    /// The jump targets that this switch case goes to
    /// </summary>
    public List<Instruction> JumpTargets;

    /// <inheritdoc />
    public override ushort Size => (ushort)(3 + (ushort)JumpTargets.Count);
}
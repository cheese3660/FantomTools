namespace FantomTools.Fantom.Code.Instructions;

/// <summary>
/// This represents an instruction that takes a field reference
/// </summary>
public class FieldInstruction : Instruction
{
    /// <summary>
    /// The field reference of this instruction
    /// </summary>
    public FieldReference Value;

    /// <inheritdoc />
    public override ushort Size => 3;
}
namespace FantomTools.Fantom.Code.Instructions;

public class FieldInstruction : Instruction
{
    public FieldReference Value;
    public override ushort Size => 3;
}
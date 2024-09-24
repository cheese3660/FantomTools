namespace FantomTools.Fantom.Code.Instructions;

public class MethodInstruction : Instruction
{
    public MethodReference Value;
    public override ushort Size => 3;
}
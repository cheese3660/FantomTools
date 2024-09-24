namespace FantomTools.Fantom.Code.Instructions;

public class IntegerInstruction : Instruction
{
    public long Value;
    public override ushort Size => 3;
}
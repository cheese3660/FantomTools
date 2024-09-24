namespace FantomTools.Fantom.Code.Instructions;

public class FloatInstruction : Instruction
{
    public double Value;
    public override ushort Size => 3;
}
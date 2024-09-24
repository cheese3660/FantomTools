namespace FantomTools.Fantom.Code.Instructions;

public class JumpInstruction : Instruction
{
    public Instruction Target;
    public override ushort Size => 3;
}
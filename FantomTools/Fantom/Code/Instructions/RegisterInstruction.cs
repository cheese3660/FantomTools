namespace FantomTools.Fantom.Code.Instructions;

public class RegisterInstruction : Instruction
{
    public MethodVariable Value;
    public override ushort Size => 3;
}
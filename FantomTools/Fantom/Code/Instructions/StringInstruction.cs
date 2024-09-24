namespace FantomTools.Fantom.Code.Instructions;

public class StringInstruction : Instruction
{
    public string Value;
    public override ushort Size => 3;
}
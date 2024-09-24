namespace FantomTools.Fantom.Code.Instructions;

public class TypeInstruction : Instruction
{
    public TypeReference Value;
    public override ushort Size => 3;
}
namespace FantomTools.Fantom.Code.Instructions;

public class TypePairInstruction : Instruction
{
    public TypeReference FirstType;
    public TypeReference SecondType;
    public override ushort Size => 5;
}
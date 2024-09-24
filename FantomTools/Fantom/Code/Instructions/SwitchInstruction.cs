namespace FantomTools.Fantom.Code.Instructions;

public class SwitchInstruction : Instruction
{
    public List<Instruction> JumpTargets;
    public override ushort Size => (ushort)(3 + (ushort)JumpTargets.Count);
}
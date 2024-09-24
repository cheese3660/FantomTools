using FantomTools.Fantom.Code.Operations;

namespace FantomTools.Fantom.Code.Instructions;

public class Instruction
{
    public ushort Offset = 0; // This is the instructions offset in the method body, used for jump instructions
    public OperationType OpCode;
    public virtual ushort Size => 1; // This is the amount of bytes of the opcode, used for recalculating offsets
}
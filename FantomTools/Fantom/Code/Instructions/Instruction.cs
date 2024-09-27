using FantomTools.Fantom.Code.Operations;

namespace FantomTools.Fantom.Code.Instructions;

/// <summary>
/// This represents an instruction
/// </summary>
public class Instruction
{
    /// <summary>
    /// The offset of this instruction in the method body, make sure to call Body.ReconstructOffsets before using this field
    /// </summary>
    public ushort Offset = 0; // This is the instructions offset in the method body, used for jump instructions
    /// <summary>
    /// The operation type
    /// </summary>
    public OperationType OpCode;
    /// <summary>
    /// The amount of bytes this instruction takes up, also used for recalculating offsets
    /// </summary>
    public virtual ushort Size => 1;
}
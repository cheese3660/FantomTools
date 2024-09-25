using FantomTools.Fantom.Code.Instructions;

namespace FantomTools.Fantom.Code.ErrorHandling;

public class TryBlock
{
    public Instruction Start;
    public Instruction End;
    public Dictionary<TypeReference, Instruction> ErrorHandlers;
    public Instruction? Finally;
}
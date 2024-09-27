using FantomTools.Fantom.Code.Instructions;

namespace FantomTools.Fantom.Code.ErrorHandling;

/// <summary>
/// This represents a single try/catch/finally chain
/// </summary>
public class TryBlock
{
    /// <summary>
    /// This is the first instruction of the try block
    /// </summary>
    public Instruction Start;
    /// <summary>
    /// This is the last instruction of the try block
    /// </summary>
    public Instruction End;
    /// <summary>
    /// This is a list of error types -> instruction that this try block handles
    /// </summary>
    public Dictionary<TypeReference, Instruction> ErrorHandlers;
    /// <summary>
    /// This is a pointer to the finally block of this try block if it exists
    /// </summary>
    public Instruction? Finally;
}
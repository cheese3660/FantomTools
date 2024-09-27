using FantomTools.Fantom.Code.Instructions;

namespace FantomTools.Fantom.Code.ErrorHandling;

/// <summary>
/// This contains information about the error table for a method
/// </summary>
public class ErrorTable
{
    /// <summary>
    /// The list of try blocks in the error table
    /// </summary>
    public List<TryBlock> TryBlocks = [];
}
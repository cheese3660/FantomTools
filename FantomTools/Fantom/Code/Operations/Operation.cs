namespace FantomTools.Fantom.Code.Operations;

/// <summary>
/// Represents information about an operation
/// </summary>
public struct Operation
{
    /// <summary>
    /// The type of the operation
    /// </summary>
    public OperationType Type;
    /// <summary>
    /// The name of the operation in textual form
    /// </summary>
    public string Name;
    /// <summary>
    /// The type of arguments the operation takes
    /// </summary>
    public OperationSignature Signature;
}
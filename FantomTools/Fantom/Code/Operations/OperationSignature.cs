namespace FantomTools.Fantom.Code.Operations;

/// <summary>
/// Contains information about what parameters an operation expects
/// </summary>
public enum OperationSignature
{
    /// <summary>
    /// This operation takes no parameters (or is a switch case)
    /// </summary>
    None,
    /// <summary>
    /// This operation takes an integer parameter
    /// </summary>
    Integer,
    /// <summary>
    /// This operation takes a float parameter
    /// </summary>
    Float,
    /// <summary>
    /// This operation takes a decimal parameter
    /// </summary>
    Decimal,
    /// <summary>
    /// This operation takes a string parameter
    /// </summary>
    String,
    /// <summary>
    /// This operation takes a duration parameter
    /// </summary>
    Duration,
    /// <summary>
    /// This operation takes a type parameter
    /// </summary>
    Type,
    /// <summary>
    /// This operation takes a URI parameter
    /// </summary>
    Uri,
    /// <summary>
    /// This operation takes a register parameter
    /// </summary>
    Register,
    /// <summary>
    /// This operation takes a field parameter
    /// </summary>
    Field,
    /// <summary>
    /// This operation takes a method parameter
    /// </summary>
    Method,
    /// <summary>
    /// This operation takes a jump parameter
    /// </summary>
    Jump,
    /// <summary>
    /// This operation takes a type-pair parameter
    /// </summary>
    TypePair,
}
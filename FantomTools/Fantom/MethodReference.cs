using JetBrains.Annotations;

namespace FantomTools.Fantom;

/// <summary>
/// 
/// </summary>
/// <param name="ParentType"></param>
/// <param name="Name"></param>
/// <param name="ReturnType"></param>
/// <param name="Parameters"></param>
[PublicAPI]
public readonly record struct MethodReference(TypeReference ParentType, string Name, TypeReference ReturnType, params TypeReference[] Parameters)
{
    /// <summary>
    /// Converts this method reference to a textual form
    /// </summary>
    /// <returns>The textual form</returns>
    public override string ToString()
    {
        return $"{ParentType}.{Name}({string.Join(',', Parameters.Select(x => x.ToString()))}) -> {ReturnType}";
    }

    /// <summary>
    /// Gets a MethodReference from a Method instance
    /// </summary>
    /// <param name="m">The Method instance</param>
    /// <returns>A method reference representing that method</returns>
    public static implicit operator MethodReference(Method m) => m.Reference;
}
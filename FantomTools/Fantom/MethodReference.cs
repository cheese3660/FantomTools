namespace FantomTools.Fantom;

public readonly record struct MethodReference(TypeReference ParentType, string Name, TypeReference ReturnType, params TypeReference[] Parameters)
{
    public override string ToString()
    {
        return $"{ParentType}.{Name}({string.Join(',', Parameters.Select(x => x.ToString()))}) -> {ReturnType}";
    }

    public static implicit operator MethodReference(Method m) => m.Reference;
}
namespace FantomTools.Fantom;

public readonly record struct FieldReference(TypeReference Parent, string Name, TypeReference Type)
{
    public override string ToString()
    {
        return $"({Type}){Parent}::{Name}";
    }

    public static implicit operator FieldReference(Field field) => field.Reference;
}
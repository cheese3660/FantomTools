using FantomTools.Fantom;
using FantomTools.Fantom.Attributes;
using FantomTools.Utilities;

namespace FantomTools.PodReading;

internal class MethodReader : SlotReader
{
    private readonly PodReader _podReader;
    public readonly TypeReferenceReader ReturnType;
    public TypeReferenceReader InheritedReturnType;
    private readonly List<MethodVariableReader> _variables;
    private readonly byte _maxStack;
    private readonly FantomBuffer? _code;
    
    public MethodReader(FantomStreamReader reader) : base(reader)
    {
        _podReader = reader.PodReader;
        ReturnType = _podReader.TypeRefs[reader.ReadU16()];
        InheritedReturnType = _podReader.TypeRefs[reader.ReadU16()];
        // Console.WriteLine($"Found inherited return type on method {Name} ========= {InheritedReturnType}");
        _maxStack = reader.ReadU8();
        var parameterCount = reader.ReadU8();
        var localCount = reader.ReadU8();
        var count = parameterCount + localCount;
        _variables = new List<MethodVariableReader>(count);
        for (var i = 0; i < count; i++)
        {
            _variables.Add(MethodVariableReader.Read(reader));
        }
        _code = FantomBuffer.Read(reader);
        ReadAttrs(reader);
    }

    public void ConstructMethod(Method method)
    {
        foreach (var local in _variables)
        {
            if (local.IsParam)
            {
                var param = method.AddParameter(local.Name, local.Type.Reference);
                param.Attributes.AddRange(local.Attrs.Attributes);
            }
            else
            {
                var variable = method.AddLocal(local.Name, local.Type.Reference);
                variable.Attributes.AddRange(local.Attrs.Attributes);
            }
        }

        method.Flags = Flags;
        method.MaxStack = _maxStack;
        method.CovariantReturnType = ReturnType.Reference;
        if (_code == null) return;
        using var reader = new FantomStreamReader(_podReader, new MemoryStream(_code.Buffer));
        method.LoadBody(reader);
        if (method.Attributes.OfType<ErrorTableAttribute>().FirstOrDefault() is { } errorTableAttribute)
        {
            method.Body.ReadErrorTable(errorTableAttribute);
        }
    }
}
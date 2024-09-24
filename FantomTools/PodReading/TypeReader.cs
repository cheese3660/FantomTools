using FantomTools.Fantom;
using FantomTools.Utilities;
using Type = FantomTools.Fantom.Type;

namespace FantomTools.PodReading;

internal class TypeReader(PodReader podReader, TypeReferenceReader self, TypeReferenceReader baseType, List<TypeReferenceReader> mixins, Flags flags)
{
    // This is set if we haven't read the following information
    private bool _hollow = true;

    private readonly List<MethodReader> _methods = [];
    private readonly List<FieldReader> _fields = [];
    public AttributesReader? Attrs;
    
    
    public static TypeReader ReadMeta(FantomStreamReader reader)
    {
        var pod = reader.PodReader;
        var self = pod.TypeRefs[reader.ReadU16()];
        var baseType = pod.TypeRefs[reader.ReadU16()];
        var mixinCount = reader.ReadU16();
        var mixins = new List<TypeReferenceReader>(mixinCount);
        for (var i = 0; i < mixinCount; i++)
        {
            mixins.Add(pod.TypeRefs[reader.ReadU16()]);
        }

        var flags = (Flags)reader.ReadU32();
        return new TypeReader(pod, self, baseType, mixins, flags);
    }


    private string Filename => $"fcode/{self.TypeName}.fcode";

    public void Read()
    {
        if (!_hollow) return;
        Read(podReader.Archive.ReadFile(Filename,true)!);
    }

    private void Read(FantomStreamReader reader)
    {
        var fieldCount = reader.ReadU16();
        _fields.EnsureCapacity(fieldCount);
        for (var i = 0; i < fieldCount; i++)
        {
            _fields.Add(new FieldReader(reader));
        }

        var methodCount = reader.ReadU16();
        _methods.EnsureCapacity(methodCount);
        for (var i = 0; i < methodCount; i++)
        {
            _methods.Add(new MethodReader(reader));
        }

        Attrs = AttributesReader.Read(reader);
        _hollow = false;
        reader.Dispose();
    }

    public Type ToMemoryType(Pod memoryPod)
    {
        if (_hollow) throw new Exception("Cannot convert hollow type to memory type!");
        var t = new Type(memoryPod, self.TypeName, baseType.Reference, flags,
            mixins.Select(x => x.Reference).ToArray());
        foreach (var attr in Attrs!.Attributes)
        {
            t.AddAttribute(attr);
        }

        foreach (var field in _fields)
        {
            var newField = t.AddField(field.Name, field.Type.Reference);
            foreach (var attr in field.Attrs!.Attributes)
            {
                newField.Attributes.Add(attr);
            }
            newField.Flags = field.Flags;
        }

        foreach (var method in _methods)
        {
            // Everything else can be done in the method type
            var newMethod = t.AddMethod(method.Name, method.ReturnType.Reference);
            method.ConstructMethod(newMethod);
        }
        return t;
    }
}
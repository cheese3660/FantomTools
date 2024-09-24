using System.IO.Compression;
using System.Text;
using FantomTools.Fantom;
using FantomTools.Fantom.Attributes;
using FantomTools.PodWriting;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom;

[PublicAPI]
public class Type(Pod pod, TypeReference selfType) : IEquatable<Type>
{
    public static TypeReference SystemObject = TypeReference.Object;
    public TypeReference Reference => selfType;
    
    public Pod TypePod => pod;
    public TypeReference SelfType => selfType;
    public TypeReference BaseType = SystemObject;
    public List<TypeReference> Mixins = [];
    public Flags Flags = 0;
    public List<Field> Fields = [];
    public List<Method> Methods = [];
    public List<FantomAttribute> Attributes = [];
    public string Name => selfType.Name;
    
    public Type(Pod pod, string name) : this(pod, new BasicTypeReference(pod.MetaData.PodName, name))
    {
    }
    
    public Type(Pod pod, string name, TypeReference baseType, Flags flags = 0, params TypeReference[] mixins) : this(pod, name)
    {
        BaseType = baseType;
        Flags = flags;
        Mixins = mixins.ToList();
    }
    
    public Type(Pod pod, string name, Flags flags = 0, params TypeReference[] mixins) : this(pod, name)
    {
        Flags = flags;
        Mixins = mixins.ToList();
    }
    
    public void AddAttribute(FantomAttribute attribute)
    {
        Attributes.Add(attribute);
    }

    public void RemoveAttribute(FantomAttribute attribute)
    {
        Attributes.Remove(attribute);
    }
    
    public void AddMixin(string mixin)
    {
        Mixins.Add(TypeReference.Reference(mixin));
    }

    public void AddMixin(TypeReference mixin)
    {
        Mixins.Add(mixin);
    }

    // This will add a new method with a given name, set of parameters, and return type
    public Method AddMethod(string name, TypeReference? returnType=null, params (string name, TypeReference type)[] parameters)
    {
        returnType ??= TypeReference.Void;
        var result = new Method(this)
        {
            Name = name,
            ReturnType = returnType,
        };
        foreach (var (parameterName, parameterType) in parameters)
        {
            result.AddParameter(parameterName, parameterType);
        }

        Methods.Add(result);
        return result;
    }
    
    // This will throw an exception if there are multiple given names that match the name, and predicate
    public Method GetMethod(string name, Func<Method,bool>? predicate = null)
    {
        predicate ??= _ => true;
        var results = Methods.Where(x => x.Name == name && predicate(x)).ToList();
        return results.Count switch
        {
            0 => throw new Exception("No method found!"),
            > 1 => throw new Exception("Too many methods found!"),
            _ => results.First()
        };
    }

    public IEnumerable<Method> GetMethods(string name) => Methods.Where(x => x.Name == name);

    public Field AddField(string name, TypeReference? type)
    {
        type ??= TypeReference.Object;
        var result = new Field(this, name, type);
        Fields.Add(result);
        return result;
    }

    public Field GetField(string name)
    {
        return Fields.First(x => x.Name == name);
    }
    
    public bool Equals(Type? other)
    {
        if (other == null) return false;
        return SelfType == other.SelfType;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Type)obj);
    }

    public override int GetHashCode()
    {
        return SelfType.GetHashCode();
    }

    public static bool operator ==(Type? left, Type? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Type? left, Type? right)
    {
        return !Equals(left, right);
    }

    public string Dump(bool dumpBodies = false)
    {
        var sb = new StringBuilder();
        sb.Append(Flags.GetString());
        sb.Append($"class {Name} extends {BaseType} ");
        if (Mixins.Count > 0)
        {
            sb.Append($"mixes {string.Join(", ", Mixins.Select(x => x.ToString()))} ");
        }
        sb.AppendLine("{");
        foreach (var field in Fields)
        {
            // TODO: Field dumping
        }
        if (Fields.Count > 0) sb.AppendLine();
        foreach (var method in Methods)
        {
            var dumped = method.Dump(dumpBodies);
            var lines = dumped.Split('\n').Select(x => x.TrimEnd());
            foreach (var line in lines)
            {
                sb.AppendLine($"    {line}");
            }
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    public void EmitMeta(FantomStreamWriter writer, FantomTables tables)
    {
        writer.WriteU16(tables.TypeReferences.Intern(selfType));
        writer.WriteU16(tables.TypeReferences.Intern(BaseType));
        writer.WriteU16((ushort)Mixins.Count);
        foreach (var mixin in Mixins) writer.WriteU16(tables.TypeReferences.Intern(mixin));
        writer.WriteU32((uint)Flags);
    }
    public void EmitBody(FantomStreamWriter writer, FantomTables tables)
    {
        writer.WriteU16((ushort)Fields.Count);
        foreach (var field in Fields)
        {
            field.Emit(writer, tables);
        }

        writer.WriteU16((ushort)Methods.Count);
        foreach (var method in Methods)
        {
            method.Emit(writer, tables);
        }
        writer.WriteU16((ushort)Attributes.Count);
        foreach (var attribute in Attributes)
        {
            attribute.Write(writer, tables);
        }
    }
}
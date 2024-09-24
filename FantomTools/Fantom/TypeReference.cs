using System.Text;
using JetBrains.Annotations;

namespace FantomTools.Fantom;

[PublicAPI]
public abstract class TypeReference : IEquatable<TypeReference>
{
    public static readonly TypeReference Object = new BasicTypeReference("sys", "Obj");
    public static readonly TypeReference Integer = new BasicTypeReference("sys", "Int");
    public static readonly TypeReference Void = new BasicTypeReference("sys", "Void");
    
    public static TypeReference Reference(string fullTypeName)
    {
        return new TypeParser(fullTypeName).Parse();
    }

    private class TypeParser(string typeName)
    {
        private int _index = 0;
        private bool End => _index >= typeName.Length;
        private char Peek => End ? '\0' : typeName[_index];
        private void Consume()
        {
            //Console.WriteLine($"Consuming {Peek} from {new StackFrame(1, true).GetMethod().Name}()");
            _index += 1;
        }

        private bool NextIs(string toCheck)
        {
            return !End && typeName[_index..].StartsWith(toCheck);
        }
        
        public TypeReference Parse(bool allowMap = true)
        {
            TypeReference t = BaseType();
            var running = true;
            while (running)
            {
                if (End) break;
                switch (Peek)
                {
                    case '[':
                        Consume();
                        if (Peek != ']')
                        {
                            throw new Exception("Invalid list type!");
                        }
                        Consume();
                        t = new ListTypeReference(t);
                        break;
                    case '?':
                        Consume();
                        t = new OptionalTypeReference(t);
                        break;
                    case ':':
                        if (allowMap)
                        {
                            Consume();
                            t = new MapTypeReference(t, Parse());
                        }
                        else
                        {
                            running = false;
                        }
                        break;
                    default:
                        running = false;
                        break;
                }
            }
            return t;
        }

        private TypeReference BaseType()
        {
            if (NextIs("[java]")) return BasicType();

            return Peek switch
            {
                '[' => MapType(),
                '|' => FuncType(),
                _ => BasicType()
            };
        }

        private MapTypeReference MapType()
        {
            Consume();
            var keyType = Parse(false);
            if (Peek != ':') throw new Exception($"Expected ':' between key and value type in map type: {typeName}");
            Consume();
            var valueType = Parse();
            if (Peek != ']') throw new Exception($"Expected ']' to end map type: {typeName}");
            Consume();
            return new MapTypeReference(keyType, valueType);
        }

        private FunctionTypeReference FuncType()
        {
            Consume();
            List<TypeReference> parameters = [];
            var returnType = Void;
            while (!NextIs("->"))
            {
                if (End) throw new Exception($"Unexpected end to type: {typeName}");
                parameters.Add(Parse());
                if (Peek != ',' && !NextIs("->"))
                    throw new Exception($"Expected '->' or ',' after parameter type in func type: {typeName}");
                if (Peek == ',') Consume();
            }

            Consume();
            Consume();
            if (Peek != '|') returnType = Parse();
            if (Peek != '|') throw new Exception($"Expected '|' to end func type: {typeName}");
            Consume();
            return new FunctionTypeReference(returnType, parameters.ToArray());
        }

        private BasicTypeReference BasicType()
        {
            var podName = new StringBuilder();
            while (Peek != ':')
            {
                if (End) throw new Exception($"Unexpected end of type name: {typeName}");
                podName.Append(Peek);
                Consume();
            }

            Consume();
            if (Peek != ':')
            {
                throw new Exception($"Expected 2 ':' in type name: {typeName}");
            }

            Consume();
            var name = new StringBuilder();
            while (char.IsDigit(Peek) || char.IsLetter(Peek) || Peek == '_')
            {
                name.Append(Peek);
                Consume();
            }

            return new BasicTypeReference(podName.ToString(), name.ToString());
        }
    }
    
    
    public bool Equals(TypeReference? other)
    {
        return other is not null && other.ToString() == ToString();
    }

    public override bool Equals(object? obj)
    {
        return obj is TypeReference other && Equals(other);
    }

    public override int GetHashCode()
    {
        return SerializedSignature.GetHashCode();
    }

    public static bool operator ==(TypeReference left, TypeReference right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TypeReference left, TypeReference right)
    {
        return !left.Equals(right);
    }

    public abstract string Pod { get; }
    public abstract string Name { get; }
    public abstract string SerializedSignature { get; }
    public abstract override string ToString();

    public TypeReference Optional => new OptionalTypeReference(this);
    public TypeReference List => new ListTypeReference(this);

    public static implicit operator TypeReference(string s) => Reference(s);
    public static implicit operator TypeReference(Type t) => t.SelfType;
}

[PublicAPI]
public sealed class BasicTypeReference(string pod, string name) : TypeReference
{
    public override string Pod => pod;
    public override string Name => name;
    
    public override string SerializedSignature => "";
    public override string ToString() => $"{Pod}::{Name}";
}

[PublicAPI]
public sealed class OptionalTypeReference(TypeReference baseType) : TypeReference
{
    public TypeReference BaseType => baseType;

    public override string Pod => baseType.Pod;
    public override string Name => baseType.Name;

    public override string SerializedSignature =>
        baseType is BasicTypeReference ? "?" : $"{baseType}?";

    public override string ToString() => $"{baseType}?";
}

[PublicAPI]
public sealed class ListTypeReference(TypeReference baseType) : TypeReference
{
    public TypeReference BaseType => baseType;

    public override string Pod => "sys";
    public override string Name => "List";
    public override string SerializedSignature => $"{baseType}[]";
    public override string ToString() => $"{baseType}[]";
}

[PublicAPI]
public sealed class MapTypeReference(TypeReference keyType, TypeReference valueType) : TypeReference
{
    public TypeReference KeyType => keyType;
    public TypeReference ValueType => valueType;


    public override string Pod => "sys";
    public override string Name => "Map";
    public override string SerializedSignature => $"[{KeyType}:{ValueType}]";
    public override string ToString() => $"[{KeyType}:{ValueType}]";
}

public sealed class FunctionTypeReference(TypeReference returnType, params TypeReference[] parameterTypes)
    : TypeReference
{
    public TypeReference ReturnType => returnType;
    public TypeReference[] ParameterTypes => parameterTypes;
    public override string Pod => "sys";
    public override string Name => "Func";
    public override string SerializedSignature => $"|{string.Join(',', parameterTypes.Select(x => x.ToString()))}->{returnType}|";

    public override string ToString() =>
        $"|{string.Join(',', parameterTypes.Select(x => x.ToString()))}->{returnType}|";

}

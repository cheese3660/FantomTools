using System.Text;
using JetBrains.Annotations;

namespace FantomTools.Fantom;

/// <summary>
/// Represents a reference to a type
/// </summary>
[PublicAPI]
public abstract class TypeReference : IEquatable<TypeReference>
{
    /// <summary>
    /// sys::Obj
    /// </summary>
    public static readonly TypeReference Object = "sys::Obj";
    /// <summary>
    /// sys::Int
    /// </summary>
    public static readonly TypeReference Integer = "sys::Int";
    /// <summary>
    /// sys::Void
    /// </summary>
    public static readonly TypeReference Void = "sys::Void";

    /// <summary>
    /// sys::Err
    /// </summary>
    public static readonly TypeReference Err = "sys::Err";
    
    /// <summary>
    /// Parse a type string into a TypeReference
    /// </summary>
    /// <param name="typeString">the string to parse</param>
    /// <returns>The parsed TypeReference</returns>
    public static TypeReference Parse(string typeString)
    {
        return new TypeParser(typeString).Parse();
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
            while (true)
            {
                while (char.IsDigit(Peek) || char.IsLetter(Peek) || Peek == '_' || Peek == '$')
                {
                    name.Append(Peek);
                    Consume();
                }

                if (NextIs("::"))
                {
                    podName.Append($"::{name}");
                    Consume();
                    Consume();
                    name.Clear();
                }
                else
                {
                    break;
                }
            }

            return new BasicTypeReference(podName.ToString(), name.ToString());
        }
    }


    /// <inheritdoc />
    public bool Equals(TypeReference? other)
    {
        return other is not null && other.ToString() == ToString();
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is TypeReference other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return SerializedSignature.GetHashCode();
    }

    /// <summary>
    /// Compare the equality of 2 type references
    /// </summary>
    /// <param name="left">The first type reference to compare</param>
    /// <param name="right">The second type reference to compare</param>
    /// <returns>True if left is equal to right</returns>
    public static bool operator ==(TypeReference left, TypeReference right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Compare the inequality of 2 type references
    /// </summary>
    /// <param name="left">The first type reference to compare</param>
    /// <param name="right">The second type reference to compare</param>
    /// <returns>True if left is not equal to right</returns>
    public static bool operator !=(TypeReference left, TypeReference right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// The pod that this type is in
    /// </summary>
    public abstract string Pod { get; }
    /// <summary>
    /// The name of this type
    /// </summary>
    public abstract string Name { get; }
    internal abstract string SerializedSignature { get; }
    /// <summary>
    /// A textual representation of this type, that can be parsed using Parse
    /// </summary>
    /// <returns>The textual representation</returns>
    public abstract override string ToString();

    /// <summary>
    /// Get an optional type reference to this type
    /// </summary>
    public TypeReference Optional => new OptionalTypeReference(this);
    /// <summary>
    /// Get a list type reference to this type
    /// </summary>
    public TypeReference List => new ListTypeReference(this);

    /// <summary>
    /// Create a type reference from a string
    /// </summary>
    /// <param name="s">The string to parse into a type reference</param>
    /// <returns>A type reference parsed from the string</returns>
    public static implicit operator TypeReference(string s) => Parse(s);
    /// <summary>
    /// Create a type reference from a Type
    /// </summary>
    /// <param name="t">the Type instance</param>
    /// <returns>A reference to the Type instance</returns>
    public static implicit operator TypeReference(Type t) => t.Reference;
}

/// <summary>
/// Represents a basic type reference, i.e. no qualifiers or generics
/// </summary>
/// <param name="pod">The pod that the type is in</param>
/// <param name="name">The name of the type</param>
[PublicAPI]
public sealed class BasicTypeReference(string pod, string name) : TypeReference
{
    /// <inheritdoc />
    public override string Pod => pod;

    /// <inheritdoc />
    public override string Name => name;
    
    internal override string SerializedSignature => "";

    /// <inheritdoc />
    public override string ToString() => $"{Pod}::{Name}";
}

/// <summary>
/// Represents an optional type reference, e.g. sys::Str?
/// </summary>
/// <param name="baseType">The type that is being treated as an optional</param>
[PublicAPI]
public sealed class OptionalTypeReference(TypeReference baseType) : TypeReference
{
    /// <summary>
    /// The type that is being treated as an optional
    /// </summary>
    public TypeReference BaseType => baseType;

    /// <inheritdoc />
    public override string Pod => baseType.Pod;

    /// <inheritdoc />
    public override string Name => baseType.Name;

    internal override string SerializedSignature =>
        baseType is BasicTypeReference ? "?" : $"{baseType}?";

    /// <inheritdoc />
    public override string ToString() => $"{baseType}?";
}

/// <summary>
/// Represents a list type, e.g. Int[]
/// </summary>
/// <param name="baseType">The type this list contains</param>
[PublicAPI]
public sealed class ListTypeReference(TypeReference baseType) : TypeReference
{
    /// <summary>
    /// The type this list contains
    /// </summary>
    public TypeReference BaseType => baseType;

    /// <inheritdoc />
    public override string Pod => "sys";

    /// <inheritdoc />
    public override string Name => "List";
    internal override string SerializedSignature => $"{baseType}[]";

    /// <inheritdoc />
    public override string ToString() => $"{baseType}[]";
}

/// <summary>
/// Represents a map type, e.g. sys::Str:Int
/// </summary>
/// <param name="keyType">The key type of the map</param>
/// <param name="valueType">The value type of the map</param>
[PublicAPI]
public sealed class MapTypeReference(TypeReference keyType, TypeReference valueType) : TypeReference
{
    /// <summary>
    /// The key type of the map
    /// </summary>
    public TypeReference KeyType => keyType;
    /// <summary>
    /// The value type of the map
    /// </summary>
    public TypeReference ValueType => valueType;


    /// <inheritdoc />
    public override string Pod => "sys";

    /// <inheritdoc />
    public override string Name => "Map";
    internal override string SerializedSignature => $"[{KeyType}:{ValueType}]";

    /// <inheritdoc />
    public override string ToString() => $"[{KeyType}:{ValueType}]";
}

/// <summary>
/// Represents a functional type reference, e.g. |Int->Int|
/// </summary>
/// <param name="returnType">The return type of the functional type</param>
/// <param name="parameterTypes">The parameter types of the functional type</param>
public sealed class FunctionTypeReference(TypeReference returnType, params TypeReference[] parameterTypes)
    : TypeReference
{
    /// <summary>
    /// The return type of the functional type
    /// </summary>
    public TypeReference ReturnType => returnType;
    /// <summary>
    /// The parameter types of the functional types
    /// </summary>
    public TypeReference[] ParameterTypes => parameterTypes;

    /// <inheritdoc />
    public override string Pod => "sys";

    /// <inheritdoc />
    public override string Name => "Func";
    internal override string SerializedSignature => $"|{string.Join(',', parameterTypes.Select(x => x.ToString()))}->{returnType}|";

    /// <inheritdoc />
    public override string ToString() =>
        $"|{string.Join(',', parameterTypes.Select(x => x.ToString()))}->{returnType}|";

}

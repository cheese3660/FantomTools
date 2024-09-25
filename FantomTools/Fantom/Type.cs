using System.IO.Compression;
using System.Text;
using FantomTools.Fantom;
using FantomTools.Fantom.Attributes;
using FantomTools.PodWriting;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom;

/// <summary>
/// Represents a fantom type in a pod
/// </summary>
/// <param name="pod">The pod this type is in</param>
/// <param name="name">The name of this type</param>
[PublicAPI]
public class Type(Pod pod, string name) : IEquatable<Type>
{
    /// <summary>
    /// Get a reference to this type as a TypeReference
    /// </summary>
    public TypeReference Reference => new BasicTypeReference(pod.MetaData.PodName, Name);
    /// <summary>
    /// The pod that this type is in
    /// </summary>
    public Pod TypePod => pod;
    /// <summary>
    /// The name of this type
    /// </summary>
    public string Name = name;
    /// <summary>
    /// The type this type inherits from
    /// </summary>
    public TypeReference BaseType = TypeReference.Object;
    /// <summary>
    /// The mixins this type uses
    /// </summary>
    public List<TypeReference> Mixins = [];
    /// <summary>
    /// The flags of this type
    /// </summary>
    public Flags Flags = Flags.Public;
    /// <summary>
    /// The fields of this type
    /// </summary>
    public List<Field> Fields = [];
    /// <summary>
    /// The methods of this type
    /// </summary>
    public List<Method> Methods = [];
    /// <summary>
    /// The attributes of this type
    /// </summary>
    public List<FantomAttribute> Attributes = [];
    
    /// <summary>
    /// Create a new type
    /// </summary>
    /// <param name="pod">The pod the type is in</param>
    /// <param name="name">The name of the type</param>
    /// <param name="baseType">The type that the type being created inherits from</param>
    /// <param name="flags">The flags of the type</param>
    /// <param name="mixins">The mixin that the type uses</param>
    public Type(Pod pod, string name, TypeReference baseType, Flags flags = Flags.Public, params TypeReference[] mixins) : this(pod, name)
    {
        BaseType = baseType;
        Flags = flags;
        Mixins = mixins.ToList();
    }
    
    /// <summary>
    /// Create a new type
    /// </summary>
    /// <param name="pod">The pod the type is in</param>
    /// <param name="name">The name of the type</param>
    /// <param name="flags">The flags of the type</param>
    /// <param name="mixins">The mixin that the type uses</param>
    public Type(Pod pod, string name, Flags flags = Flags.Public, params TypeReference[] mixins) : this(pod, name)
    {
        Flags = flags;
        Mixins = mixins.ToList();
    }
    
    /// <summary>
    /// Add an attribute to a type
    /// </summary>
    /// <param name="attribute">The attribute to add</param>
    public void AddAttribute(FantomAttribute attribute)
    {
        Attributes.Add(attribute);
    }

    /// <summary>
    /// Remove an attribute from a type
    /// </summary>
    /// <param name="attribute">The attribute to remove</param>
    public void RemoveAttribute(FantomAttribute attribute)
    {
        Attributes.Remove(attribute);
    }
    
    /// <summary>
    /// Add a mixin to a type
    /// </summary>
    /// <param name="mixin">The mixin to add</param>
    public void AddMixin(TypeReference mixin)
    {
        Mixins.Add(mixin);
    }

    /// <summary>
    /// Create and add a method to a type
    /// </summary>
    /// <param name="name">The name of the type to create</param>
    /// <param name="returnType">The return type of the method</param>
    /// <param name="parameters">name, type pairs representing method parameters</param>
    /// <returns>The newly created method</returns>
    public Method AddMethod(string name, TypeReference? returnType=null, params (string name, TypeReference type)[] parameters)
    {
        returnType ??= TypeReference.Void;
        var result = new Method(this)
        {
            Name = name,
            ReturnType = returnType,
            CovariantReturnType = returnType
        };
        foreach (var (parameterName, parameterType) in parameters)
        {
            result.AddParameter(parameterName, parameterType);
        }

        Methods.Add(result);
        return result;
    }
    
    /// <summary>
    /// Get a method by name, and with an optional predicate to match overloads
    /// </summary>
    /// <param name="name">The name of the method to get</param>
    /// <param name="predicate">The predicate to filter methods</param>
    /// <returns>The method that matches the name and optional predicate</returns>
    /// <exception cref="Exception">Thrown if no methods are found, or if too many methods are found</exception>
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

    /// <summary>
    /// Get all methods with a given name
    /// </summary>
    /// <param name="name">The name to search for</param>
    /// <returns>All the methods with the given name</returns>
    public IEnumerable<Method> GetMethods(string name) => Methods.Where(x => x.Name == name);

    /// <summary>
    /// Add a field to the type
    /// </summary>
    /// <param name="name">The name of the field</param>
    /// <param name="type">The type of the field, defaults to sys::Obj</param>
    /// <returns>The newly added field</returns>
    public Field AddField(string name, TypeReference? type = null)
    {
        type ??= TypeReference.Object;
        var result = new Field(this, name, type);
        Fields.Add(result);
        return result;
    }

    /// <summary>
    /// Get a field from the type
    /// </summary>
    /// <param name="name">The name of the field</param>
    /// <returns>The field with the name</returns>
    public Field GetField(string name)
    {
        return Fields.First(x => x.Name == name);
    }

    /// <inheritdoc />
    public bool Equals(Type? other)
    {
        if (other == null) return false;
        return Reference == other.Reference;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Type)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Reference.GetHashCode();
    }

    /// <summary>
    /// Compare the equality of 2 types
    /// </summary>
    /// <param name="left">The first type to compare</param>
    /// <param name="right">The second type to compare</param>
    /// <returns>True if left is equal to right</returns>
    public static bool operator ==(Type? left, Type? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Compare the inequality of 2 types
    /// </summary>
    /// <param name="left">The first type to compare</param>
    /// <param name="right">The second type to compare</param>
    /// <returns>True if left is not equal to right</returns>
    public static bool operator !=(Type? left, Type? right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    /// Generate a textual representation of the type
    /// </summary>
    /// <param name="dumpBodies">Should method bytecode disassembly be dumped?</param>
    /// <param name="dumpDecompilationGuesses">Should method bytecode decompilation guesses be dumped?</param>
    /// <returns>The textual representation</returns>
    public string Dump(bool dumpBodies = false, bool dumpDecompilationGuesses=false)
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
            var dumped = field.Dump();
            var lines = dumped.Split('\n').Select(x => x.TrimEnd());
            foreach (var line in lines)
            {
                sb.AppendLine($"    {line}");
            }
        }
        foreach (var method in Methods)
        {
            var dumped = method.Dump(dumpBodies,dumpDecompilationGuesses);
            var lines = dumped.Split('\n').Select(x => x.TrimEnd());
            foreach (var line in lines)
            {
                sb.AppendLine($"    {line}");
            }
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    internal void EmitMeta(FantomStreamWriter writer, FantomTables tables)
    {
        writer.WriteU16(tables.TypeReferences.Intern(Reference));
        writer.WriteU16(tables.TypeReferences.Intern(BaseType));
        writer.WriteU16((ushort)Mixins.Count);
        foreach (var mixin in Mixins) writer.WriteU16(tables.TypeReferences.Intern(mixin));
        writer.WriteU32((uint)Flags);
    }
    internal void EmitBody(FantomStreamWriter writer, FantomTables tables)
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
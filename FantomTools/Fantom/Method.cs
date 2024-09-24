using System.Text;
using FantomTools.Fantom.Attributes;
using FantomTools.Fantom.Code;
using FantomTools.PodWriting;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom;

/// <summary>
/// Represents a fantom method contained in a type
/// </summary>
[PublicAPI]
public class Method
{
    /// <summary>
    /// The type this method is attached to
    /// </summary>
    public readonly Type ParentType;
    /// <summary>
    /// The flags of this method
    /// </summary>
    public Flags Flags = Flags.Public;
    /// <summary>
    /// The name of the method
    /// </summary>
    public string Name;

    /// <summary>
    /// All the variables in this method (parameters & locals)
    /// </summary>
    public IReadOnlyList<MethodVariable> Variables => _variables;
    private List<MethodVariable> _variables = [];
    /// <summary>
    /// All the parameters for this method
    /// </summary>
    public IEnumerable<MethodVariable> Parameters => Variables.Where(x => x.IsParameter);
    /// <summary>
    /// All the locals in this method
    /// </summary>
    public IEnumerable<MethodVariable> Locals => Variables.Where(x => !x.IsParameter);
    
    /// <summary>
    /// The return type of the method
    /// </summary>
    public TypeReference ReturnType = TypeReference.Void;
    /// <summary>
    /// The base type of the return type that this is inherited from, if it is covariant
    /// </summary>
    public TypeReference CovariantReturnType = TypeReference.Void;
    /// <summary>
    /// The body of the method
    /// </summary>
    public MethodBody Body;
    /// <summary>
    /// The attributes on this method
    /// </summary>
    public List<FantomAttribute> Attributes = [];

    /// <summary>
    /// The maximum stack size of the method, defaults to 16
    /// </summary>
    public byte MaxStack = 16;

    /// <summary>
    /// Get a MethodReference that references this method
    /// </summary>
    public MethodReference Reference => new(ParentType.Reference, Name, ReturnType,
        Parameters.Select(x => x.Type).ToArray());

    /// <summary>
    /// Is this method static, i.e. does it only have a static constructor
    /// </summary>
    public bool IsStatic => Flags.HasFlag(Flags.Static);

    internal Method(Type parent)
    {
        ParentType = parent;
        Name = "<unnamed method>";
        Body = new MethodBody(this);
    }

    /// <summary>
    /// Add a parameter to this method
    /// </summary>
    /// <param name="parameterName">The name of the parameter</param>
    /// <param name="parameterType">The type of the parameter</param>
    /// <returns>The new parameter</returns>
    public MethodVariable AddParameter(string parameterName, TypeReference? parameterType = null)
    {
        parameterType ??= TypeReference.Object;
        var index = 0;
        var found = false;
        
        // I want to be able to 
        for (var i = 0; i < Variables.Count; i++)
        {
            if (found || !Variables[i].IsParameter)
            {
                if (!found) index = i;
                found = true;
                Variables[i].Index += 1;
            }
        }
        _variables.Insert(index, new MethodVariable(true)
        {
            Index = (ushort)index,
            Name = parameterName,
            Type = parameterType,
        });
        return Variables[index];
    }

    /// <summary>
    /// Add a local to this method
    /// </summary>
    /// <param name="localName">The name of the local</param>
    /// <param name="localType">The type of the local</param>
    /// <returns>AThe new local</returns>
    public MethodVariable AddLocal(string localName, TypeReference? localType = null)
    {
        localType ??= TypeReference.Object;
        var index = Variables.Count;
        _variables.Add(new MethodVariable(false)
        {
            Index = (ushort)index,
            Name = localName,
            Type = localType,
        });
        return Variables[index];
    }

    
    /// <summary>
    /// Gets a variable from a method, if it exists
    /// </summary>
    /// <param name="name">The name of the variable</param>
    /// <returns>The variable</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the variable does not exist on the method</exception>
    public MethodVariable GetVariable(string name)
    {
        foreach (var variable in Variables)
        {
            if (variable.Name == name)
            {
                return variable;
            }
        }

        throw new KeyNotFoundException(name);
    }

    /// <summary>
    /// Remove a variable from a method by name
    /// </summary>
    /// <param name="name">The name of the variable to remove</param>
    /// <exception cref="KeyNotFoundException">Thrown if the variable does not exist on the method</exception>
    public void RemoveVariable(string name)
    {
        var found = false;
        for (var i = Variables.Count - 1; i >= 0; i--)
        {
            if (Variables[i].Name == name)
            {
                _variables.RemoveAt(i);
                found = true;
                break;
            }

            if (!found)
            {
                Variables[i].Index -= 1;
            }
        }

        if (!found) throw new KeyNotFoundException(name);
    }

    /// <summary>
    /// Remove a variable from a method
    /// </summary>
    /// <param name="variable">The variable to remove</param>
    public void RemoveVariable(MethodVariable variable)
    {
        for (var i = Variables.Count - 1; i > variable.Index; i--)
        {
            Variables[i].Index -= 1;
        }

        _variables.RemoveAt(variable.Index);
    }
    
    /// <summary>
    /// Add an attribute to this method
    /// </summary>
    /// <param name="attribute">The attribute to add</param>
    public void AddAttribute(FantomAttribute attribute)
    {
        Attributes.Add(attribute);
    }

    /// <summary>
    /// Remove an attribute from this method
    /// </summary>
    /// <param name="attribute">The attribute to remove</param>
    public void RemoveAttribute(FantomAttribute attribute)
    {
        Attributes.Remove(attribute);
    }
    
    internal void LoadBody(FantomStreamReader reader)
    {
        Body.Read(reader);
    }
    
    /// <summary>
    /// Does this method match the given signature?
    /// </summary>
    /// <param name="returnType">The return type to match</param>
    /// <param name="argumentTypes">The argument types to match</param>
    /// <returns>True if the method matches the given return type and parameter types</returns>
    public bool MatchesSignature(TypeReference returnType, params TypeReference[] argumentTypes)
    {
        if (returnType != ReturnType) return false;
        var p = Parameters.ToList();
        return argumentTypes.Length == p.Count && argumentTypes.Zip(p).Any(x => x.First != x.Second.Type);
    }
    
    /// <summary>
    /// Dump this method into textual form
    /// </summary>
    /// <param name="dumpBody">Should this dump the disassembly of the fantom bytecode?</param>
    /// <returns>The textual form of the method</returns>
    public string Dump(bool dumpBody=false)
    {
        var sb = new StringBuilder();
        sb.Append(Flags.GetString());
        sb.Append(ReturnType);
        sb.AppendLine($" {Name}({string.Join(", ", Parameters.Select(x => $"{x.Type} {x.Name}"))}) [");
        sb.AppendLine($"    .maxstack {MaxStack}");
        foreach (var local in Locals)
        {
            sb.AppendLine($"    .local {local.Type} {local.Name}");
        }
        sb.AppendLine("]\n{");

        if (dumpBody)
        {
            sb.Append(Body.Dump());
        }
        else
        {
            sb.AppendLine("    /* Body has been omitted from the method dump */");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    internal void Emit(FantomStreamWriter writer, FantomTables tables)
    {
        writer.WriteU16(tables.Names.Intern(Name));
        writer.WriteU32((uint)Flags);
        writer.WriteU16(tables.TypeReferences.Intern(ReturnType)); // Return type
        writer.WriteU16(tables.TypeReferences.Intern(CovariantReturnType)); // Inherited return type (fantom doesn't use covariance?)
        writer.WriteU8(MaxStack);
        writer.WriteU8((byte)Parameters.Count());
        writer.WriteU8((byte)Locals.Count());
        foreach (var variable in Variables)
        {
            variable.Emit(writer, tables);
        }
        Body.Emit(writer, tables);
        writer.WriteU16((ushort)Attributes.Count);
        foreach (var attribute in Attributes)
        {
            attribute.Write(writer, tables);
        }
    }
}
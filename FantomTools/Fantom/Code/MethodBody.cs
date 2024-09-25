using System.Text;
using System.Web;
using FantomTools.Fantom.Attributes;
using FantomTools.Fantom.Code.DisassemblyTools;
using FantomTools.Fantom.Code.ErrorHandling;
using FantomTools.Fantom.Code.Instructions;
using FantomTools.Fantom.Code.Operations;
using FantomTools.PodWriting;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom.Code;

/// <summary>
/// Represents the code of a method
/// </summary>
/// <param name="method">The method</param>
[PublicAPI]
public class MethodBody(Method method)
{
    /// <summary>
    /// The method that this is a body of
    /// </summary>
    public Method Method => method;
    
    /// <summary>
    /// The instructions contained within this body
    /// </summary>
    public List<Instruction> Instructions = [];

    public ErrorTable ErrorTable = new();
    
    internal void Read(FantomStreamReader reader)
    {
        Dictionary<ushort, List<Action<Instruction>>> pendingLabels = [];
        Dictionary<ushort, Instruction> previousInstructions = [];
        ushort currentOffset = 0;
        var op = 0;
        while ((op = reader.Stream.ReadByte()) > 0)
        {
            var offset = currentOffset;
            currentOffset += 1;
            var instruction = GetOperation(op);
            AddInstruction(offset, instruction);
        }
        
        if (pendingLabels.Count > 0)
        {
            throw new Exception("Read instruction body without making every label");
        }

        void PendLabel(ushort offset, Action<Instruction> onResolve)
        {
            if (previousInstructions.TryGetValue(offset, out var resolved))
            {
                onResolve(resolved);
            }
            else
            {
                if (pendingLabels.TryGetValue(offset, out var pending))
                {
                    pending.Add(onResolve);
                }
                else
                {
                    pendingLabels[offset] = [onResolve];
                }
            }
        }

        void AddInstruction(ushort offset, Instruction instruction)
        {
            if (pendingLabels.TryGetValue(offset, out var pending))
            {
                foreach (var resolver in pending)
                {
                    resolver(instruction);
                }
                pendingLabels.Remove(offset);
            }
            previousInstructions[offset] = instruction;
            Instructions.Add(instruction);
        }

        Instruction GetOperation(int opcode)
        {
            var operationType = (OperationType)opcode;
            if (Operations.Operations.OperationsByType.TryGetValue(operationType, out var operation))
            {
                if (operationType == OperationType.Switch)
                {
                    var count = reader.ReadU16();
                    currentOffset += 2;
                    var inst = new SwitchInstruction
                    {
                        OpCode = OperationType.Switch,
                        JumpTargets = new List<Instruction>(count)
                    };
                    for (var i = 0; i < count; i++)
                    {
                        inst.JumpTargets.Add(null!);
                        var i1 = i;
                        PendLabel(reader.ReadU16(), target => inst.JumpTargets[i1] = target);
                        currentOffset += 2;
                    }

                    return inst;
                }

                switch (operation.Signature)
                {
                    case OperationSignature.None: return new Instruction { OpCode = operationType };
                    case OperationSignature.Integer:
                        currentOffset += 2;
                        return new IntegerInstruction
                            { OpCode = operationType, Value = reader.PodReader.Integers[reader.ReadU16()] };
                    case OperationSignature.Float:
                        currentOffset += 2;
                        return new FloatInstruction
                            { OpCode = operationType, Value = reader.PodReader.Floats[reader.ReadU16()] };
                    case OperationSignature.Decimal:
                        currentOffset += 2;
                        return new StringInstruction
                            { OpCode = operationType, Value = reader.PodReader.Decimals[reader.ReadU16()] };
                    case OperationSignature.String:
                        currentOffset += 2;
                        return new StringInstruction
                            { OpCode = operationType, Value = reader.PodReader.Strings[reader.ReadU16()] };
                    case OperationSignature.Duration:
                        currentOffset += 2;
                        return new IntegerInstruction
                            { OpCode = operationType, Value = reader.PodReader.Durations[reader.ReadU16()] };
                    case OperationSignature.Uri:
                        currentOffset += 2;
                        return new StringInstruction
                            { OpCode = operationType, Value = reader.PodReader.Uris[reader.ReadU16()] };
                    case OperationSignature.Type:
                        currentOffset += 2;
                        return new TypeInstruction
                            { OpCode = operationType, Value = reader.PodReader.TypeRefs[reader.ReadU16()].Reference };
                    case OperationSignature.Register:
                        currentOffset += 2;
                        if (method.IsStatic)
                        {
                            return new RegisterInstruction
                            {
                                OpCode = operationType, Value = method.Variables[reader.ReadU16()]
                            };
                        }
                        else
                        {
                            var idx = reader.ReadU16();
                            if (idx == 0) return new RegisterInstruction { OpCode = operationType };
                            return new RegisterInstruction
                                { OpCode = operationType, Value = method.Variables[idx - 1] };
                        }
                    case OperationSignature.Field:
                        currentOffset += 2;
                        return new FieldInstruction
                        {
                            OpCode = operationType, Value = reader.PodReader.FieldRefs[reader.ReadU16()].Reference
                        };
                    case OperationSignature.Method:
                        currentOffset += 2;
                        return new MethodInstruction
                        {
                            OpCode = operationType, Value = reader.PodReader.MethodRefs[reader.ReadU16()].Reference
                        };
                    case OperationSignature.Jump:
                    {
                        currentOffset += 2;
                        var jumpInst = new JumpInstruction { OpCode = operationType };
                        PendLabel(reader.ReadU16(), x => jumpInst.Target = x);
                        return jumpInst;
                    }
                    case OperationSignature.TypePair:
                        currentOffset += 4;
                        return new TypePairInstruction
                        {
                            OpCode = operationType,
                            FirstType = reader.PodReader.TypeRefs[reader.ReadU16()].Reference,
                            SecondType = reader.PodReader.TypeRefs[reader.ReadU16()].Reference
                        };
                    default:
                        throw new ArgumentOutOfRangeException();
                        
                }
            }

            throw new Exception($"Unknown opcode: {opcode}");
        }
    }

    internal void ReadErrorTable(ErrorTableAttribute attribute)
    {
        ReconstructOffsets();
        foreach (var record in attribute.Entries)
        {
            var start = Instructions.First(x => x.Offset == record.TryStart);
            var end = Instructions.First(x => x.Offset == record.TryEnd);
            // Remove extraneous finally blocks
            if (start.OpCode is OperationType.CatchAllStart or OperationType.CatchErrStart) continue;
            var handler = Instructions.First(x => x.Offset == record.Handler);
            var possibleTryBlock = ErrorTable.TryBlocks.FirstOrDefault(x => x.Start == start && x.End == end);
            if (possibleTryBlock is not null)
            {
                if (end.OpCode is not OperationType.FinallyStart)
                {
                    possibleTryBlock.ErrorHandlers[record.ErrorType] = handler;
                }
                else
                {
                    possibleTryBlock.Finally = handler;
                }
            }
            else
            {
                if (end.OpCode is not OperationType.FinallyStart)
                {
                    var tb = new TryBlock
                    {
                        Start = start,
                        End = end,
                        ErrorHandlers = new Dictionary<TypeReference, Instruction>
                        {
                            [record.ErrorType] = handler
                        }
                    };
                    ErrorTable.TryBlocks.Add(tb);
                }
                else
                {
                    var tb = new TryBlock
                    {
                        Start = start,
                        End = end,
                        ErrorHandlers = [],
                        Finally = handler
                    };
                    ErrorTable.TryBlocks.Add(tb);
                }
            }
        }
    }

    internal ErrorTableAttribute ReconstructErrorTable()
    {
        ReconstructOffsets();
        ErrorTableAttribute attribute = new();
        foreach (var block in ErrorTable.TryBlocks)
        {
            var start = block.Start.Offset;
            var end = block.End.Offset;
            foreach (var (type, handler) in block.ErrorHandlers)
            {
                attribute.Entries.Add(new ErrorTableEntry(start, end, handler.Offset, type));
            }
        }
        return attribute;
    }

    /// <summary>
    /// Get a textual disassembly of the method body
    /// </summary>
    /// <returns>The textual disassembly</returns>
    public string Dump(bool addDecompilationGuesses=false) => DisassemblyBuilder.DisassembleAll(addDecompilationGuesses);

    internal void Emit(FantomStreamWriter writer, FantomTables tables)
    {
        ReconstructOffsets();
        using var bodyStream = new MemoryStream();
        var bodyWriter = new FantomStreamWriter(bodyStream);
        foreach (var instruction in Instructions)
        {
            bodyWriter.WriteU8((byte)instruction.OpCode);
            switch (instruction)
            {
                case IntegerInstruction integerInstruction:
                    bodyWriter.WriteU16(instruction.OpCode == OperationType.LoadDuration
                        ? tables.Durations.Intern(integerInstruction.Value)
                        : tables.Integers.Intern(integerInstruction.Value));
                    break;
                case FloatInstruction floatInstruction:
                    bodyWriter.WriteU16(tables.Floats.Intern(floatInstruction.Value));
                    break;
                case StringInstruction stringInstruction:
                    bodyWriter.WriteU16(instruction.OpCode switch
                    {
                        OperationType.LoadStr => tables.Strings.Intern(stringInstruction.Value),
                        OperationType.LoadDecimal => tables.Decimals.Intern(stringInstruction.Value),
                        _ => tables.Uris.Intern(stringInstruction.Value)
                    });
                    break;
                case RegisterInstruction registerInstruction:
                    if (method.IsStatic) bodyWriter.WriteU16(registerInstruction.Value.Index);
                    else if (registerInstruction.Value is not null) bodyWriter.WriteU16((ushort)(registerInstruction.Value.Index + 1));
                    else bodyWriter.WriteU16(0);
                    break;
                case TypeInstruction typeInstruction:
                    bodyWriter.WriteU16(tables.TypeReferences.Intern(typeInstruction.Value));
                    break;
                case FieldInstruction fieldInstruction:
                    bodyWriter.WriteU16(tables.FieldReferences.Intern(fieldInstruction.Value));
                    break;
                case MethodInstruction methodInstruction:
                    bodyWriter.WriteU16(tables.MethodReferences.Intern(methodInstruction.Value));
                    break;
                case TypePairInstruction typePairInstruction:
                    bodyWriter.WriteU16(tables.TypeReferences.Intern(typePairInstruction.FirstType));
                    bodyWriter.WriteU16(tables.TypeReferences.Intern(typePairInstruction.SecondType));
                    break;
                case JumpInstruction jumpInstruction:
                    bodyWriter.WriteU16(jumpInstruction.Target.Offset);
                    break;
                case SwitchInstruction switchInstruction:
                {
                    bodyWriter.WriteU16((ushort)switchInstruction.JumpTargets.Count);
                    foreach (var target in switchInstruction.JumpTargets) bodyWriter.WriteU16(target.Offset);
                    break;
                }
            }
        }

        var bytes = bodyStream.ToArray();
        writer.WriteU16((ushort)bytes.Length);
        writer.Stream.Write(bytes);
    }

    

    public void ReconstructOffsets()
    {
        ushort offset = 0;
        foreach (var instruction in Instructions)
        {
            instruction.Offset = offset;
            offset += instruction.Size;
        }
    }
    
    
    /// <summary>
    /// Get a cursor for easier method modification
    /// </summary>
    public MethodCursor Cursor => new(this);

    /// <summary>
    /// Get a disassembly builder for disassembling this method
    /// </summary>
    public DisassemblyBuilder DisassemblyBuilder => new(this);
}
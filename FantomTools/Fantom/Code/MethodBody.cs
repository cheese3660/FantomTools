using System.Text;
using System.Web;
using FantomTools.Fantom.Attributes;
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
    public string Dump()
    {
        ReconstructOffsets();
        var labels = ConstructLabels();
        var padding = labels.Count > 0 ? labels.Values.Select(x => x.Length).Max() + 2 : 4;
        var (tryStarts, tryEnds, catches, finallies) = GetErrorHandlingInformation();
        var sb = new StringBuilder();
        // So now we need to build up a list of handlers
        var catchStack = new Stack<(string, string)>();
        var finallyStack = new Stack<string>();
        foreach (var instruction in Instructions)
        {
            
            if (tryEnds.TryGetValue(instruction, out var tryEnd))
            {
                for (var i = 0; i < padding; i++)
                {
                    sb.Append(' ');
                }

                sb.AppendLine($"/* end try-block {tryEnd} */");
            }

            if (catches.TryGetValue(instruction, out var c))
            {
                for (var i = 0; i < padding; i++)
                {
                    sb.Append(' ');
                }

                sb.AppendLine($"/* begin try-block {c.block} catch {c.type} */");
                catchStack.Push((c.block,c.type));
            }
            if (finallies.TryGetValue(instruction, out var f))
            {
                for (var i = 0; i < padding; i++)
                {
                    sb.Append(' ');
                }

                sb.AppendLine($"/* begin try-block {f} finally */");
                finallyStack.Push(f);
            }
            

            if (tryStarts.TryGetValue(instruction, out var tryStartList))
            {
                foreach (var tryBlock in tryStartList)
                {
                    for (var i = 0; i < padding; i++)
                    {
                        sb.Append(' ');
                    }

                    sb.AppendLine($"/* begin try-block {tryBlock} */");
                }
            }

            if (labels.TryGetValue(instruction.Offset, out var label))
            {
                sb.Append($"{label}: ");
                for (var i = $"{label}: ".Length; i < padding; i++)
                {
                    sb.Append(' ');
                }
            }
            else
            {
                for (var i = 0; i < padding; i++)
                {
                    sb.Append(' ');
                }
            }

            sb.Append($"{Operations.Operations.OperationsByType[instruction.OpCode].Name}");
            switch (instruction)
            {
                case IntegerInstruction integerInstruction:
                {
                    DumpIntegerInstruction(sb, integerInstruction, instruction);
                    break;
                }
                case FloatInstruction floatInstruction:
                    sb.AppendLine($" {floatInstruction.Value}");
                    break;
                case StringInstruction stringInstruction:
                    DumpStringInstruction(instruction, sb, stringInstruction);
                    break;
                case RegisterInstruction registerInstruction:
                    sb.AppendLine($" {registerInstruction.Value?.Name ?? "this"}");
                    break;
                case TypeInstruction typeInstruction:
                    sb.AppendLine($" {typeInstruction.Value}");
                    break;
                case FieldInstruction fieldInstruction:
                    sb.AppendLine($" {fieldInstruction.Value}");
                    break;
                case MethodInstruction methodInstruction:
                    sb.AppendLine($" {methodInstruction.Value}");
                    break;
                case TypePairInstruction typePairInstruction:
                    sb.AppendLine($" {typePairInstruction.FirstType}; {typePairInstruction.SecondType}");
                    break;
                case JumpInstruction jumpInstruction:
                    sb.AppendLine($" {labels[jumpInstruction.Target.Offset]}");
                    break;
                case SwitchInstruction switchInstruction:
                {
                    DumpSwitchInstruction(sb, switchInstruction, padding, labels);
                    break;
                }
                default:
                    sb.AppendLine();
                    break;
            }

            switch (instruction.OpCode)
            {
                case OperationType.CatchEnd when catchStack.Count > 0:
                {
                    var (block, type) = catchStack.Pop();
                    for (var i = 0; i < padding; i++)
                    {
                        sb.Append(' ');
                    }

                    sb.AppendLine($"/* end try-block {block} catch {type} */");
                    break;
                }
                case OperationType.CatchEnd:
                    sb.AppendLine("/* unknown catch block end??? corrupted error table or bad code? */");
                    break;
                case OperationType.FinallyEnd when finallyStack.Count > 0:
                {
                    var block = finallyStack.Pop();
                    for (var i = 0; i < padding; i++)
                    {
                        sb.Append(' ');
                    }
                    sb.AppendLine($"/* end try-block {block} finally */");
                    break;
                }
                case OperationType.FinallyEnd:
                    sb.AppendLine("/* unknown finally block end??? corrupted error table or bad code? */");
                    break;
                default:
                    break;
            }
        }
        return sb.ToString();
    }

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

    private static void DumpSwitchInstruction(StringBuilder sb, SwitchInstruction switchInstruction, int padding,
        Dictionary<ushort, string> labels)
    {
        sb.AppendLine(" {");
        for (var i = 0; i < switchInstruction.JumpTargets.Count; i++)
        {
            for (var j = 0; j < padding; j++)
            {
                sb.Append(' ');
            }

            sb.Append("    ");
            sb.AppendLine($"{i} -> {labels[switchInstruction.JumpTargets[i].Offset]}");
        }
        for (var i = 0; i < padding; i++)
        {
            sb.Append(' ');
        }
        sb.AppendLine("}");
        return;
    }

    private static void DumpStringInstruction(Instruction instruction, StringBuilder sb,
        StringInstruction stringInstruction)
    {
        switch (instruction.OpCode)
        {
            case OperationType.LoadStr or OperationType.LoadUri:
                sb.AppendLine($" {HttpUtility.JavaScriptStringEncode(stringInstruction.Value,true)}");
                break;
            case OperationType.LoadDecimal:
                sb.AppendLine($" {stringInstruction.Value}");
                break;
        }

        return;
    }

    private static void DumpIntegerInstruction(StringBuilder sb, IntegerInstruction integerInstruction,
        Instruction instruction)
    {
        sb.Append($" {integerInstruction.Value}");
        if (instruction.OpCode is OperationType.LoadDuration)
        {
            sb.Append(" ticks");
        }
        sb.AppendLine();
        return;
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

    public Dictionary<ushort, string> ConstructLabels()
    {
        ushort nextLabelNumber = 0;
        Dictionary<ushort, string> result = [];
        foreach (var instruction in Instructions)
        {
            if (instruction is JumpInstruction jumpInstruction)
            {
                var targetIndex = jumpInstruction.Target.Offset;
                if (!result.ContainsKey(targetIndex))
                {
                    result[targetIndex] = $"L{nextLabelNumber++}";
                }
            } else if (instruction is SwitchInstruction switchInstruction)
            {
                foreach (var targetIndex in switchInstruction.JumpTargets.Select(target => target.Offset)
                             .Where(targetIndex => !result.ContainsKey(targetIndex)))
                {
                    result[targetIndex] = $"L{nextLabelNumber++}";
                }
            }
        }
        return result;
    }

    
    // Could likely be made public?
    internal (Dictionary<Instruction, List<string>> Starts, Dictionary<Instruction, string> Ends, Dictionary<Instruction, (string block, string type)> Handlers, Dictionary<Instruction, string> Finallies) GetErrorHandlingInformation()
    {
        Dictionary<Instruction, List<string>> starts = [];
        Dictionary<Instruction, string> ends = [];
        Dictionary<Instruction, (string block,string type)> handlers = [];
        Dictionary<Instruction, string> finallies = []; 
        var i = 0;
        foreach (var handler in ErrorTable.TryBlocks)
        {
            if (starts.TryGetValue(handler.Start, out var startsList))
            {
                startsList.Add($"T{i}");
            }
            else
            {
                starts[handler.Start] = [$"T{i}"];
            }
            ends[handler.End] = $"T{i}";

            foreach (var (type, inst) in handler.ErrorHandlers)
            {
                handlers[inst] = ($"T{i}", type.ToString());
            }

            if (handler.Finally is { } @finally)
            {
                finallies[handler.Finally] = $"T{i}";
            }
            i++;
        }
        return (starts, ends, handlers, finallies);
    }
    
    /// <summary>
    /// Get a cursor for easier method modification
    /// </summary>
    public MethodCursor Cursor => new(this);
}
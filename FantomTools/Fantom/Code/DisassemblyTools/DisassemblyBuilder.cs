using System.Text;
using System.Web;
using FantomTools.Fantom.Code.Instructions;
using FantomTools.Fantom.Code.Operations;

namespace FantomTools.Fantom.Code.DisassemblyTools;

public class DisassemblyBuilder
{
    public readonly MethodBody Body;
    private Method Method => Body.Method;

    public readonly IReadOnlyDictionary<Instruction, string> Labels;
    public readonly IReadOnlyDictionary<Instruction, List<string>> TryStarts;
    public readonly IReadOnlyDictionary<Instruction, string> TryEnds;
    public readonly IReadOnlyDictionary<Instruction, (string blockName, string typeName)> CatchStarts;
    public readonly IReadOnlyDictionary<Instruction, string> FinallyStarts;
    
    public DisassemblyBuilder(MethodBody body)
    {
        Body = body;
        Labels = ConstructLabels();
        (TryStarts, TryEnds, CatchStarts, FinallyStarts) = GetErrorHandlingInformation();
    }






    public string DisassembleAll(bool addDecompilationGuesses = false)
    {
        var labels = ConstructLabels();
        var padding = labels.Count > 0 ? labels.Values.Select(x => x.Length).Max() + 2 : 4;
        var (tryStarts, tryEnds, catches, finallies) = GetErrorHandlingInformation();
        var sb = new StringBuilder();
        // So now we need to build up a list of handlers
        var catchStack = new Stack<(string, string)>();
        var finallyStack = new Stack<string>();
        foreach (var instruction in Body.Instructions)
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

            if (labels.TryGetValue(instruction, out var label))
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
                    sb.AppendLine($" {labels[jumpInstruction.Target]}");
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

    public string DisassembleRange(Instruction? begin, Instruction? end = null, bool addDecompilationGuesses = false)
    {
        throw new NotImplementedException();
    }

    public string DisassembleSingle(Instruction instruction, bool addLabels = false)
    {
        throw new NotImplementedException();
    }
    
    private Dictionary<Instruction, string> ConstructLabels()
    {
        ushort nextLabelNumber = 0;
        ushort nextReturnNumber = 0;
        Dictionary<Instruction, string> result = [];
        if (Body.Instructions.Count > 0)
        {
            result[Body.Instructions[0]] = "start";
        }
        foreach (var instruction in Body.Instructions.Skip(1))
        {
            switch (instruction)
            {
                case JumpInstruction jumpInstruction:
                {
                    if (!result.ContainsKey(jumpInstruction.Target))
                    {
                        if (jumpInstruction.Target.OpCode != OperationType.Return)
                        {
                            result[jumpInstruction.Target] = $"L{nextLabelNumber++}";
                        }
                        else
                        {
                            result[jumpInstruction.Target] = $"RET{nextReturnNumber++}";
                        }
                    }

                    break;
                }
                case SwitchInstruction switchInstruction:
                {
                    foreach (var targetInst in switchInstruction.JumpTargets.Where(x => !result.ContainsKey(x)))
                    {
                        if (targetInst.OpCode != OperationType.Return)
                        {
                            result[targetInst] = $"L{nextLabelNumber++}";
                        }
                        else
                        {
                            result[targetInst] = $"RET{nextReturnNumber++}";
                        }
                    }

                    break;
                }
            }
        }
        return result;
    }
    
    private (Dictionary<Instruction, List<string>> Starts, Dictionary<Instruction, string> Ends, Dictionary<Instruction, (string block, string type)> Handlers, Dictionary<Instruction, string> Finallies) GetErrorHandlingInformation()
    {
        Dictionary<Instruction, List<string>> starts = [];
        Dictionary<Instruction, string> ends = [];
        Dictionary<Instruction, (string block,string type)> handlers = [];
        Dictionary<Instruction, string> finallies = []; 
        var i = 0;
        foreach (var handler in Body.ErrorTable.TryBlocks)
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
    
    private static void DumpSwitchInstruction(StringBuilder sb, SwitchInstruction switchInstruction, int padding,
        Dictionary<Instruction, string> labels)
    {
        sb.AppendLine(" {");
        for (var i = 0; i < switchInstruction.JumpTargets.Count; i++)
        {
            for (var j = 0; j < padding; j++)
            {
                sb.Append(' ');
            }

            sb.Append("    ");
            sb.AppendLine($"{i} -> {labels[switchInstruction.JumpTargets[i]]}");
        }
        for (var i = 0; i < padding; i++)
        {
            sb.Append(' ');
        }
        sb.AppendLine("}");
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
    }
}
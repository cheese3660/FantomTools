using System.Text;
using System.Web;
using FantomTools.Fantom.Code.Instructions;
using FantomTools.Fantom.Code.Operations;
using JetBrains.Annotations;

namespace FantomTools.Fantom.Code.DisassemblyTools;

[PublicAPI]
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






    // Time to do a better decompilation tool
    public string DisassembleAll(bool addDecompilationGuesses = false) => DisassembleRange(null, null, true, true, true);

    public string DisassembleRange(Instruction? begin, Instruction? end = null, bool addLabels = false, bool addDecompilationGuesses = false, bool addTryCatches=false)
    {
        var startIndex = begin is null ? 0 : Body.Instructions.IndexOf(begin);
        if (startIndex == -1) throw new ArgumentException("Instruction is not in body!", nameof(begin));
        var endIndex = end is null ? Body.Instructions.Count - 1 : Body.Instructions.IndexOf(end);
        if (startIndex > endIndex) return "";
        var labels = ConstructLabels();
        var padding = addLabels ? labels.Count > 0 ? labels.Values.Select(x => x.Length).Max() + 2 : 4 : 4;
        //var (tryStarts, tryEnds, catches, finallies) = addTryCatches ? GetErrorHandlingInformation() : (null,null,null,null);
        var disassemblyBuilder = new StringBuilder();
        StatementDecompilationBuilder? decompilationBuilder = addDecompilationGuesses ? new() : null;
        var isVoid = addDecompilationGuesses && Method.ReturnType == TypeReference.Void;
        // So now we need to build up a list of handlers
        var tryIndentation = 0;
        var catchStack = new Stack<(string, string)>();
        var finallyStack = new Stack<string>();
        var shouldSkipNextInstruction = false;
        var nextIndex = 0;
        for (var index = startIndex; index <= endIndex; index++)
        {
            nextIndex += 1;
            if (shouldSkipNextInstruction)
            {
                shouldSkipNextInstruction = false;
                continue;
            }
            var instruction = Body.Instructions[index];

            if (addTryCatches)
            {
                if (TryEnds.TryGetValue(instruction, out var tryEnd))
                {
                    if (addDecompilationGuesses) disassemblyBuilder.Append(decompilationBuilder!.EndStatement());
                    tryIndentation -= 1;
                    Indent(disassemblyBuilder);
                    disassemblyBuilder.AppendLine($"}} /* {tryEnd} */");
                }
                if (TryStarts.TryGetValue(instruction, out var tryStartList))
                {
                    if (addDecompilationGuesses) disassemblyBuilder.Append(decompilationBuilder!.EndStatement());
                    foreach (var tryBlock in tryStartList)
                    {
                        Indent(disassemblyBuilder);
                        disassemblyBuilder.AppendLine($"try /* {tryBlock} */ {{");
                        tryIndentation += 1;
                    }
                }
                if (CatchStarts.TryGetValue(instruction, out var c))
                {
                    
                    if (addDecompilationGuesses) disassemblyBuilder.Append(decompilationBuilder!.EndStatement());
                    Indent(disassemblyBuilder);
                    
                    if (instruction.OpCode == OperationType.CatchErrStart)
                    {
                        if (nextIndex < Body.Instructions.Count &&
                            Body.Instructions[nextIndex].OpCode == OperationType.StoreVar)
                        {
                            var name = (Body.Instructions[nextIndex] as RegisterInstruction)?.Value?.Name ?? "this";
                            disassemblyBuilder.AppendLine($"catch ({c.typeName} {name}) /* {c.blockName} */ {{");
                            shouldSkipNextInstruction = true;
                        }
                        else
                        {
                            disassemblyBuilder.AppendLine($"catch ({c.typeName} <unknown>) /* {c.blockName} */ {{");
                        }
                    }
                    else
                    {
                        disassemblyBuilder.AppendLine($"catch /* {c.blockName} */ {{");
                    }
                    catchStack.Push((c.blockName,c.typeName));
                    continue;
                }
                if (FinallyStarts.TryGetValue(instruction, out var f))
                {
                    if (addDecompilationGuesses) disassemblyBuilder.Append(decompilationBuilder!.EndStatement());
                    if (labels.TryGetValue(instruction, out var lab))
                    {
                        Indent(disassemblyBuilder, lab);
                    }
                    else
                    {
                        Indent(disassemblyBuilder);
                    }
                    disassemblyBuilder.AppendLine($"finally /* {f} */ {{");
                    finallyStack.Push(f);
                    continue;
                }
            }
            var sb = new StringBuilder();
            if (addLabels && labels.TryGetValue(instruction, out var label))
            {
                Indent(sb,label);
            }
            else
            {
                Indent(sb);
            }
            sb.Append(DisassembleSingle(instruction));
            if (addTryCatches)
            {
                if (instruction.OpCode == OperationType.FinallyEnd && finallyStack.Count > 0)
                {
                    if (addDecompilationGuesses) disassemblyBuilder.Append(decompilationBuilder!.EndStatement());
                    var value = finallyStack.Pop();
                    Indent(disassemblyBuilder);
                    disassemblyBuilder.AppendLine($"}} /* {value} finally */");
                    continue;
                }

                if (instruction.OpCode == OperationType.CatchEnd && catchStack.Count > 0)
                {

                    if (addDecompilationGuesses) disassemblyBuilder.Append(decompilationBuilder!.EndStatement());
                    var (type, value) = catchStack.Pop();
                    Indent(disassemblyBuilder);
                    disassemblyBuilder.AppendLine($"}} /* {value} catch {type} */");
                    continue;
                }
            }
            if (addDecompilationGuesses)
            {
                var consumed = decompilationBuilder!.Consume(instruction, sb.ToString(), padding * (1 + Math.Max(tryIndentation + catchStack.Count + finallyStack.Count,0)), isVoid, labels);
                if (consumed is not null) disassemblyBuilder.Append(consumed);
            }
            else
            {
                disassemblyBuilder.Append(sb);
            }
        }
        if (addDecompilationGuesses) disassemblyBuilder.Append(decompilationBuilder!.EndStatement());
        return disassemblyBuilder.ToString();
        void Indent(StringBuilder sb, string label="")
        {
            sb.Append(new string(' ', padding * (tryIndentation + catchStack.Count + finallyStack.Count)));
            if (!addLabels) return;
            if (label != "")
            {
                sb.Append($"{label}:");
                sb.Append(new string(' ', padding - (label.Length + 1)));
            }
            else
            {
                sb.Append(new string(' ', padding));
            }
        }
    }

    public string DisassembleSingle(Instruction instruction, bool addLabels = false)
    {
        var labels = addLabels ? Labels : null;
        var padding = (labels?.Count ?? 0) > 0 ? labels!.Values.Select(x => x.Length).Max() + 2 : 4;
        var sb = new StringBuilder();
        
        if (addLabels & labels!.TryGetValue(instruction, out var label))
        {
            Indent(label!);
        }
        else
        {
            Indent();
        }
        
        sb.Append($"{Operations.Operations.OperationsByType[instruction.OpCode].Name}");
        switch (instruction)
        {
            case IntegerInstruction integerInstruction:
            {
                sb.Append(DumpIntegerInstruction(integerInstruction, instruction));
                break;
            }
            case FloatInstruction floatInstruction:
                sb.Append($" {floatInstruction.Value}");
                break;
            case StringInstruction stringInstruction:
                sb.Append(DumpStringInstruction(instruction, stringInstruction));
                break;
            case RegisterInstruction registerInstruction:
                sb.Append($" {registerInstruction.Value?.Name ?? "this"}");
                break;
            case TypeInstruction typeInstruction:
                sb.Append($" {typeInstruction.Value}");
                break;
            case FieldInstruction fieldInstruction:
                sb.Append($" {fieldInstruction.Value}");
                break;
            case MethodInstruction methodInstruction:
                sb.Append($" {methodInstruction.Value}");
                break;
            case TypePairInstruction typePairInstruction:
                sb.Append($" {typePairInstruction.FirstType}; {typePairInstruction.SecondType}");
                break;
            case JumpInstruction jumpInstruction:
                sb.Append($" {labels[jumpInstruction.Target]}");
                break;
            case SwitchInstruction switchInstruction:
            {
                sb.Append(DumpSwitchInstruction(switchInstruction, padding));
                break;
            }
        }

        return sb.ToString();
        
        void Indent(string label="")
        {
            sb.Append(new string(' ', padding));
            if (label != "")
            {
                sb.Append($"{label}:");
                sb.Append(new string(' ', padding - (label.Length + 1)));
            }
            else
            {
                sb.Append(new string(' ', padding));
            }
        }
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
    
    private StringBuilder DumpSwitchInstruction(SwitchInstruction switchInstruction, int padding)
    {
        var sb = new StringBuilder();
        sb.AppendLine(" {");
        for (var i = 0; i < switchInstruction.JumpTargets.Count; i++)
        {
            for (var j = 0; j < padding; j++)
            {
                sb.Append(' ');
            }

            sb.Append("    ");
            sb.AppendLine($"{i} -> {Labels[switchInstruction.JumpTargets[i]]}");
        }
        for (var i = 0; i < padding; i++)
        {
            sb.Append(' ');
        }
        sb.Append('}');
        return sb;
    }

    private static StringBuilder DumpStringInstruction(Instruction instruction,
        StringInstruction stringInstruction)
    {
        var sb = new StringBuilder();
        switch (instruction.OpCode)
        {
            case OperationType.LoadStr or OperationType.LoadUri:
                sb.Append($" {HttpUtility.JavaScriptStringEncode(stringInstruction.Value, true)}");
                break;
            case OperationType.LoadDecimal:
                sb.Append($" {stringInstruction.Value}");
                break;
        }

        return sb;
    }

    private static StringBuilder DumpIntegerInstruction(IntegerInstruction integerInstruction,
        Instruction instruction)
    {
        var sb = new StringBuilder();
        sb.Append($" {integerInstruction.Value}");
        if (instruction.OpCode is OperationType.LoadDuration)
        {
            sb.Append(" ticks");
        }
        return sb;
    }
}
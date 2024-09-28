using System.Text;
using System.Web;
using FantomTools.Fantom.Code.Instructions;
using FantomTools.Fantom.Code.Operations;
using FantomTools.InternalUtilities;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom.Code.DisassemblyTools;

/// <summary>
/// This is a class providing tools for creating disassemblies of methods
/// </summary>
[PublicAPI]
public class DisassemblyBuilder
{
    /// <summary>
    /// The method body that this disassembly builder is attached to
    /// </summary>
    public readonly MethodBody Body;
    
    /// <summary>
    /// The method that this disassembly builder is attached to
    /// </summary>
    public Method Method => Body.Method;

    /// <summary>
    /// This is a dictionary of Instruction to String of the labels that this disassembly builder generated
    /// </summary>
    public readonly IReadOnlyDictionary<Instruction, string> Labels;
    
    /// <summary>
    /// This is a dictionary of Instruction to List&lt;string&gt; denoting the starts of try blocks
    /// </summary>
    public readonly IReadOnlyDictionary<Instruction, List<string>> TryStarts;
    /// <summary>
    /// This is dictionary of Instruction to String denoting the ends of try blocks
    /// </summary>
    public readonly IReadOnlyDictionary<Instruction, string> TryEnds;
    
    /// <summary>
    /// This is a dictionary of instruction to a tuple of string, string denoting the start of catches, what blocks they are in, and the type they catch
    /// </summary>
    public readonly IReadOnlyDictionary<Instruction, (string blockName, string typeName)> CatchStarts;
    /// <summary>
    /// This is a dictionary of instruction to a string denoting the starts of finally blocks
    /// </summary>
    public readonly IReadOnlyDictionary<Instruction, string> FinallyStarts;

    /// <summary>
    /// Create a disassembly builder from a method body
    /// </summary>
    /// <param name="body">The method body</param>
    /// <param name="labelOverride">A dictionary that overrides the set of labels in the method, mostly used from the assembly builder</param>
    public DisassemblyBuilder(MethodBody body, Dictionary<Instruction,string>? labelOverride = null)
    {
        Body = body;
        Labels = labelOverride ?? ConstructLabels();
        (TryStarts, TryEnds, CatchStarts, FinallyStarts) = GetErrorHandlingInformation();
    }






    /// <summary>
    /// Disassemble the entire method body
    /// </summary>
    /// <param name="addDecompilationGuesses">Should decompilation guesses be added as comments?</param>
    /// <returns>A string disassembly of the method body</returns>
    public string DisassembleAll(bool addDecompilationGuesses = false) => DisassembleRange(null, null, true, true, true);

    /// <summary>
    /// Disassemble a range of instructions in a method body
    /// </summary>
    /// <param name="begin">The starting instruction, null to be the first instruction</param>
    /// <param name="end">The ending instruction, null to be the last instruction</param>
    /// <param name="addLabels">Should labels be added to the disassembly?</param>
    /// <param name="addDecompilationGuesses">Should decompilation guesses be added to the disassembly?</param>
    /// <param name="addTryCatches">Should try/catch/finally blocks be added to the disassembly?</param>
    /// <returns>A string disassembly of the given range of instruction</returns>
    /// <exception cref="ArgumentException">Thrown when either begin/end are not in the method body</exception>
    public string DisassembleRange(Instruction? begin, Instruction? end = null, bool addLabels = false, bool addDecompilationGuesses = false, bool addTryCatches=false)
    {
        var startIndex = begin is null ? 0 : Body.Instructions.IndexOf(begin);
        if (startIndex == -1) throw new ArgumentException("Instruction is not in body!", nameof(begin));
        var endIndex = end is null ? Body.Instructions.Count - 1 : Body.Instructions.IndexOf(end);
        if (startIndex > endIndex)
            throw new ArgumentException("Instruction is not in body, or before start!", nameof(end));
        var padding = addLabels ? Labels.Count > 0 ? Labels.Values.Select(x => x.Length).Max() + 2 : 4 : 4;
        var disassemblyBuilder = new StringBuilder();
        StatementDecompilationBuilder? decompilationBuilder = addDecompilationGuesses ? new StatementDecompilationBuilder() : null;
        var isVoid = addDecompilationGuesses && Method.ReturnType == TypeReference.Void;
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
                    if (TryEnds.TryGetValue(instruction, out var tryEnd))
                    {
                        tryIndentation -= 1;
                        Indent(disassemblyBuilder);
                        disassemblyBuilder.AppendLine($"}} /* {tryEnd} */");
                    }
                    
                    if (addDecompilationGuesses) disassemblyBuilder.Append(decompilationBuilder!.EndStatement());
                    Indent(disassemblyBuilder);
                    
                    if (instruction.OpCode == OperationType.CatchErrStart)
                    {
                        if (nextIndex < Body.Instructions.Count &&
                            Body.Instructions[nextIndex].OpCode == OperationType.StoreVar)
                        {
                            var name = (Body.Instructions[nextIndex] as RegisterInstruction)?.Value?.Name ?? "this";
                            disassemblyBuilder.AppendLine($"catch ({name}, {c.typeName}) /* {c.blockName} */ {{");
                            shouldSkipNextInstruction = true;
                        }
                        else
                        {
                            disassemblyBuilder.AppendLine($"catch (<unknown>, {c.typeName}) /* {c.blockName} */ {{");
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
                    if (TryEnds.TryGetValue(instruction, out var tryEnd))
                    {
                        tryIndentation -= 1;
                        Indent(disassemblyBuilder);
                        disassemblyBuilder.AppendLine($"}} /* {tryEnd} */");
                    }
                    
                    if (Labels.TryGetValue(instruction, out var lab))
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
            if (addLabels && Labels.TryGetValue(instruction, out var label))
            {
                Indent(sb,label);
            }
            else
            {
                Indent(sb);
            }
            sb.AppendLine(DisassembleSingle(instruction));
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
                if (TryEnds.TryGetValue(instruction, out var tryEnd))
                {
                    var consumed = decompilationBuilder!.Consume(instruction, sb.ToString(), padding * (1 + Math.Max(tryIndentation + catchStack.Count + finallyStack.Count,0)), isVoid, Labels);
                    disassemblyBuilder.Append(consumed ?? decompilationBuilder.EndStatement());
                    tryIndentation -= 1;
                    Indent(disassemblyBuilder);
                    disassemblyBuilder.AppendLine($"}} /* {tryEnd} */");
                    continue;
                }
            }
            if (addDecompilationGuesses)
            {
                var consumed = decompilationBuilder!.Consume(instruction, sb.ToString(), padding * (1 + Math.Max(tryIndentation + catchStack.Count + finallyStack.Count,0)), isVoid, Labels);
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

    /// <summary>
    /// Disassemble a single instruction in context of the method body
    /// </summary>
    /// <param name="instruction">The instruction to disassemble</param>
    /// <param name="addLabels">Whether any labels on the instruction should be output</param>
    /// <param name="paddingOverride">The override to the left padding amount</param>
    /// <returns>A string disassembly of the single instruction</returns>
    public string DisassembleSingle(Instruction instruction, bool addLabels = false, int paddingOverride = -1)
    {
        var padding = addLabels ? Labels.Count > 0 ? Labels.Values.Select(x => x.Length).Max() + 2 : 4 : 0;
        if (paddingOverride > -1)
        {
            padding = paddingOverride;
        }
        var sb = new StringBuilder();
        
        if (addLabels && Labels.TryGetValue(instruction, out var label))
        {
            Indent(label);
        }
        else if (addLabels)
        {
            Indent();
        }
        
        sb.Append($"{Operations.Operations.OperationsByType[instruction.OpCode].Name}");
        switch (instruction)
        {
            case IntegerInstruction integerInstruction:
            {
                sb.Append(DumpIntegerInstruction(integerInstruction));
                break;
            }
            case FloatInstruction floatInstruction:
                sb.Append($" {floatInstruction.Value}");
                break;
            case StringInstruction stringInstruction:
                sb.Append(DumpStringInstruction(stringInstruction));
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
                sb.Append($" {Labels![jumpInstruction.Target]}");
                break;
            case SwitchInstruction switchInstruction:
            {
                sb.Append(DumpSwitchInstruction(switchInstruction, padding));
                break;
            }
        }

        return sb.ToString();
        
        void Indent(string lab="")
        {
            if (lab != "")
            {
                sb.Append($"{lab}:");
                sb.Append(new string(' ', padding - (lab.Length + 1)));
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
        foreach (var instruction in Body.Instructions)
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
        // We actually likely want to sort the try starts, so let's do that
        var indices = Body.Instructions.ToDictionary(x => x, x => Body.Instructions.IndexOf(x));
        Dictionary<Instruction, List<string>> starts = [];
        Dictionary<Instruction, string> ends = [];
        Dictionary<Instruction, (string block,string type)> handlers = [];
        Dictionary<Instruction, string> finallies = [];
        Dictionary<string, Instruction> previousEnds = [];
        var i = 0;
        foreach (var handler in Body.ErrorTable.TryBlocks)
        {
            if (starts.TryGetValue(handler.Start, out var startsList))
            {
                // We need to sort this such that the ones that are wider are earlier in the list
                // startsList.Add($"T{i}");
                int index;
                for (index = 0; index < startsList.Count; index++)
                {
                    if (indices[previousEnds[startsList[index]]] <= indices[handler.End])
                    {
                        break;
                    }
                }
                startsList.Insert(index, $"T{i}");
            }
            else
            {
                starts[handler.Start] = [$"T{i}"];
            }
            ends[handler.End] = $"T{i}";
            previousEnds[$"T{i}"] = handler.End;

            foreach (var (type, inst) in handler.ErrorHandlers)
            {
                handlers[inst] = ($"T{i}", type.ToString());
            }

            if (handler.Finally is { } @finally)
            {
                finallies[@finally] = $"T{i}";
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

    private static StringBuilder DumpStringInstruction(StringInstruction stringInstruction)
    {
        var sb = new StringBuilder();
        switch (stringInstruction.OpCode)
        {
            case OperationType.LoadStr or OperationType.LoadUri:
                sb.Append($" \"{stringInstruction.Value.Escape()}\"");
                break;
            case OperationType.LoadDecimal:
                sb.Append($" {stringInstruction.Value}");
                break;
        }

        return sb;
    }

    private static StringBuilder DumpIntegerInstruction(IntegerInstruction integerInstruction)
    {
        var sb = new StringBuilder();
        if (integerInstruction.OpCode is OperationType.LoadDuration)
        {
            sb.Append($" {integerInstruction.Value.ToDurationString()}");
        }
        else
        {
            sb.Append($" {integerInstruction.Value}");
        }
        return sb;
    }
}
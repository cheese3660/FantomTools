using System.Text;
using FantomTools.Fantom;
using FantomTools.Fantom.Code;
using FantomTools.Fantom.Code.Instructions;
using FantomTools.Fantom.Code.Operations;

var sb = new StringBuilder();
sb.AppendLine("flowchart TD");
var pod = Pod.FromFile(args[0]);
var types = args[2].Split(",");
foreach (var type in pod.Types.Where(type => types.Contains(type.Name)))
{
    sb.AppendLine("    subgraph " + type.Name);
    foreach (var method in type.Methods)
    {
        const string methodIndent = "            ";
        var methodPrefix = $"{type.Name}::{method.Name}";
        sb.AppendLine("        subgraph " + methodPrefix);
        var builder = method.Body.DisassemblyBuilder;
        var generatedLabels = builder.Labels.ToDictionary();
        // Reconstruct offsets so we can sort stuff
        method.Body.ReconstructOffsets();
        var sortedLabels = new SortedList<Instruction, string>(generatedLabels, Comparer<Instruction>.Create((x,y) => x.Offset.CompareTo(y.Offset)));
        
        // generate a list of labels used in the method, for easily labeling later (ie. by hand when analyzing)
        
        // foreach (var label in sortedLabels)
        // {
        //     sb.AppendLine($"{methodIndent}{methodPrefix}_{label.Value}[{label.Value}]");
        // }

        // sb.AppendLine();

        var sbStack = new Stack<StringBuilder>();
        sbStack.Push(new StringBuilder());
        
        // iterate over the instructions in the method body, tracking which label we're under
        var curLabel = "start";
        var generateJumpToNext = true; // tracks if we need to generate a jump to the new label when changing labels
        //
        sb.AppendLine($"{methodIndent}{methodPrefix}_start[start]");

        var linkSb = new StringBuilder();
        
        // This keeps all the catches for a try (for finally blocks)
        Dictionary<string, List<string>> catches = [];
        
        foreach (var instr in method.Body.Instructions)
        {
            // figure out if our current label changed
            var possibleNewLabels = sortedLabels
                .Where(l => l.Key == instr)
                .ToList();
            if (possibleNewLabels.Count != 0)
            {
                var newLabel = possibleNewLabels.FirstOrDefault().Value;
                if (newLabel != curLabel)
                {
                    sbStack.Peek().AppendLine($"{Indent(methodIndent, sbStack.Count - 1)}{methodPrefix}_{newLabel}[{newLabel}]");
                    if (generateJumpToNext)
                        linkSb.AppendLine(
                                $"{methodIndent}{methodPrefix}_{curLabel} --> {methodPrefix}_{newLabel}");
                    generateJumpToNext = true;
                    curLabel = newLabel;
                }
            }
            
            var tryStarts = builder.TryStarts.GetValueOrDefault(instr, []);
            foreach (var start in tryStarts)
            {
                var newLabel = $"try_{start}";
                if (generateJumpToNext)
                {
                    linkSb.AppendLine($"{methodIndent}{methodPrefix}_{curLabel} --> {methodPrefix}_{newLabel}");
                }
                sbStack.Peek().AppendLine($"{Indent(methodIndent,sbStack.Count-1)}subgraph {methodPrefix}_{newLabel}[try {start}]");
                sbStack.Push(new StringBuilder());
                sbStack.Peek().AppendLine(
                        $"{Indent(methodIndent, sbStack.Count - 1)}{methodPrefix}_{newLabel}_begin[entry]");
                curLabel = $"{newLabel}_begin";
                catches[start] = [];
            }

            if (builder.CatchStarts.TryGetValue(instr, out var ctch))
            {
                var catchLabel = $"{ctch.blockName}_catch_{ctch.typeName}";
                catches[ctch.blockName].Add(catchLabel);
                linkSb.AppendLine($"{methodIndent}{methodPrefix}_try_{ctch.blockName} -- catch {ctch.typeName} --> {methodPrefix}_{catchLabel}");
                sbStack.Peek().AppendLine(
                        $"{Indent(methodIndent, sbStack.Count - 1)}subgraph {methodPrefix}_{catchLabel}[catch {ctch.typeName} -- {ctch.blockName}]");
                sbStack.Push(new StringBuilder());
                sbStack.Peek().AppendLine(
                        $"{Indent(methodIndent, sbStack.Count - 1)}{methodPrefix}_{catchLabel}_begin[entry]");
                curLabel = $"{catchLabel}_begin";
            }

            if (builder.FinallyStarts.TryGetValue(instr, out var blockName))
            {
                var finallyLabel = $"{blockName}_finally";
                linkSb.AppendLine(
                        $"{methodIndent}{methodPrefix}_try_{blockName} -- finally --> {methodPrefix}_{finallyLabel}");
                foreach (var catchLabel in catches[blockName])
                {
                    linkSb.AppendLine(
                            $"{methodIndent}{methodPrefix}_{catchLabel} -- finally --> {methodPrefix}_{finallyLabel}");
                }
                sbStack.Peek().AppendLine(
                        $"{Indent(methodIndent, sbStack.Count - 1)}subgraph {methodPrefix}_{finallyLabel}[finally -- {blockName}]");
                sbStack.Push(new StringBuilder());
                sbStack.Peek().AppendLine(
                        $"{Indent(methodIndent, sbStack.Count - 1)}{methodPrefix}_{finallyLabel}_begin[entry]");
                curLabel = $"{finallyLabel}_begin";
            }

            if (instr.OpCode is OperationType.Jump or OperationType.Leave or OperationType.JumpFinally)
            {
                generateJumpToNext = false;
            }
            
            switch (instr)
            {
                case JumpInstruction jumpInstruction:
                {
                    var destLabel = sortedLabels[jumpInstruction.Target];
                    var opcodeLabel = Operations.OperationsByType[jumpInstruction.OpCode].Name;
                    linkSb.AppendLine($"{methodIndent}{methodPrefix}_{curLabel} -- {opcodeLabel} --> {methodPrefix}_{destLabel}");
                    break;
                }
                case SwitchInstruction switchInstruction:
                {
                    var opcodeLabel = Operations.OperationsByType[switchInstruction.OpCode].Name;
                    foreach (var target in switchInstruction.JumpTargets)
                    {
                        var destLabel = sortedLabels[target];
                        linkSb.AppendLine(
                            $"{methodIndent}{methodPrefix}_{curLabel} -- {opcodeLabel} --> {methodPrefix}_{destLabel}");
                    }
                    break;
                }
                default:
                    switch (instr.OpCode)
                    {
                        case OperationType.FinallyEnd:
                        case OperationType.CatchEnd:
                        {
                            var body = sbStack.Pop();
                            sbStack.Peek().Append(body);
                            sbStack.Peek().AppendLine($"{Indent(methodIndent, sbStack.Count - 1)}end");
                            sbStack.Peek().AppendLine();
                        }
                            break;
                    }
                    break;
            }
            var tryEnd = builder.TryEnds.GetValueOrDefault(instr);
            if (tryEnd is not null)
            {
                var body = sbStack.Pop();
                sbStack.Peek().Append(body);
                sbStack.Peek().AppendLine($"{Indent(methodIndent, sbStack.Count - 1)}end");
                sbStack.Peek().AppendLine();
            }
        }
        var subGraphs = sbStack.Pop();
        sb.Append(subGraphs);
        sb.Append(linkSb);
        sb.AppendLine("        end");
    }
    sb.AppendLine("    end");
}
Console.WriteLine(System.Environment.CurrentDirectory);
Console.WriteLine(args[1]);
File.WriteAllText(args[1], sb.ToString());

string Indent(string baseIndent, int depth)
{
    StringBuilder sb2 = new();
    sb2.Append(baseIndent);
    sb2.Append(new string(' ', depth * 4));
    return sb2.ToString();
}
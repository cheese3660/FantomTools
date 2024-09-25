using System.Text;
using FantomTools.Fantom;
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
        var methodPrefix = $"{type.Name}::{method.Name}";
        sb.AppendLine("        subgraph " + methodPrefix);
        method.Body.ReconstructOffsets();
        var generatedLabels = method.Body.ConstructLabels();
        var sortedLabels = new SortedList<ushort, string>(generatedLabels);
        
        // generate a list of labels used in the method, for easily labeling later (ie. by hand when analyzing)
        if (!sortedLabels.ContainsKey(0))
        {
            sb.AppendLine($"            {methodPrefix}_start[start]");
        }
        foreach (var label in sortedLabels)
        {
            sb.AppendLine($"            {methodPrefix}_{label.Value}[{label.Value}]");
        }

        sb.AppendLine();
        
        // iterate over the instructions in the method body, tracking which label we're under
        var curLabel = "start";
        foreach (var instr in method.Body.Instructions)
        {
            // figure out if our current label changed
            var possibleNewLabels = sortedLabels
                .Where(l => l.Key > instr.Offset)
                .ToList();
            if (possibleNewLabels.Count != 0)
                curLabel = possibleNewLabels.FirstOrDefault().Value;

            switch (instr)
            {
                case JumpInstruction jumpInstruction:
                {
                    var destLabel = sortedLabels[jumpInstruction.Target.Offset];
                    var opcodeLabel = Operations.OperationsByType[jumpInstruction.OpCode].Name;
                
                    sb.AppendLine($"        {methodPrefix}_{curLabel} -- {opcodeLabel} --> {methodPrefix}_{destLabel}");
                    break;
                }
                case SwitchInstruction switchInstruction:
                {
                    var opcodeLabel = Operations.OperationsByType[switchInstruction.OpCode].Name;
                    foreach (var target in switchInstruction.JumpTargets)
                    {
                        var destLabel = sortedLabels[target.Offset];
                        sb.AppendLine(
                            $"            {methodPrefix}_{curLabel} -- {opcodeLabel} --> {methodPrefix}_{destLabel}");
                    }
                    break;
                }
            }
        }
        
        sb.AppendLine("        end");
    }
    sb.AppendLine("    end");
}
Console.WriteLine(System.Environment.CurrentDirectory);
Console.WriteLine(args[1]);
File.WriteAllText(args[1], sb.ToString());

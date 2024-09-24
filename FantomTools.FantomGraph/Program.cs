using System.Text;
using FantomTools.Fantom;

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
        
        // iterate over the instructions in the method body, tracking which label we're under
        var curLabel = "start";
        foreach (var instr in method.Body.Instructions)
        {
            // figure out if our current label changed
            var possibleNewLabels = 
            if (newLabel != null)
            {
                
            }
                
            if ()
        }
        
        sb.AppendLine("        end");
    }
    sb.AppendLine("    end");
}
Console.WriteLine(System.Environment.CurrentDirectory);
Console.WriteLine(args[1]);
File.WriteAllText(args[1], sb.ToString());

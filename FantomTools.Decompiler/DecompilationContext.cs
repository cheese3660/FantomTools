using FantomTools.Fantom;
using FantomTools.Fantom.Code;
using FantomTools.Fantom.Code.DisassemblyTools;
using FantomTools.Fantom.Code.Instructions;

namespace FantomTools.Decompiler;

public class DecompilationContext
{
    public readonly MethodBody MethodBody;
    public readonly Method Method;
    public readonly DisassemblyBuilder DisassemblyBuilder;
    public readonly List<MethodVariable> AlreadyAssignedVariables = [];
    public readonly HashSet<Instruction> IgnoredReturns = []; 
    
    public DecompilationContext(Method m)
    {
        Method = m;
        MethodBody = m.Body;
        DisassemblyBuilder = MethodBody.DisassemblyBuilder;
    }
}
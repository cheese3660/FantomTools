using FantomTools.Fantom;
using FantomTools.Fantom.Code;
using FantomTools.Fantom.Code.DisassemblyTools;

namespace FantomTools.Decompiler;

public class DecompilationContext
{
    public MethodBody MethodBody;
    public Method Method;
    public DisassemblyBuilder DisassemblyBuilder;

    public DecompilationContext(Method m)
    {
        Method = m;
        MethodBody = m.Body;
        DisassemblyBuilder = MethodBody.DisassemblyBuilder;
    }
}
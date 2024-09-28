using System.Text;
using JetBrains.Annotations;

namespace FantomTools.Decompiler;

[PublicAPI]
public class Decompiler
{
    public DecompilationContext RootContext;





    private List<BlockContext> SplitIntoBlocks()
    {
        throw new NotImplementedException();
    }

    private void DecompileBlock(StringBuilder sb, BlockContext context)
    {
        // So now this is where the root of everything is going to go
    }
}
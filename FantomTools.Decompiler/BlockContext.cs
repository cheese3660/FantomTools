using System.Text;
using FantomTools.Fantom.Code.Instructions;

namespace FantomTools.Decompiler;

/// <summary>
/// Represents the context for a single decompilation block
/// </summary>
public class BlockContext(string? header, string? footer, int indentationDepth, IReadOnlyList<Instruction> instructions)
{
    public int Index = 0;
}
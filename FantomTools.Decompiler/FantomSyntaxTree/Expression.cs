using System.Text;

namespace FantomTools.Decompiler.FantomSyntaxTree;

public abstract class Expression
{
    public abstract string Dump();
    public abstract int Precedence { get; }
}
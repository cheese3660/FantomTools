using System.Text;

namespace FantomTools.Decompiler.FantomSyntaxTree;

public abstract class Statement
{
    public abstract void Write(StringBuilder sb, int indentation = 0);
}
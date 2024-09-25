using System.Text;
using System.Web;
using FantomTools.Fantom.Code.Instructions;
using FantomTools.Fantom.Code.Operations;

namespace FantomTools.Fantom.Code.DisassemblyTools;

internal class StatementDecompilationBuilder
{
    private Stack<string> _decompiledStatements = new();
    private StringBuilder _statementBuilder = new();




    public string? Consume(Instruction instruction, string disassembly, int decompilationPadding, bool isVoidMethod, Dictionary<Instruction, string> labels)
    {
        // It will already have a newline!
        var methodInst = instruction as MethodInstruction;
        var fieldInst = instruction as FieldInstruction;
        var jumpInst = instruction as JumpInstruction;
        var registerInst = instruction as RegisterInstruction;
        var typeInst = instruction as TypeInstruction;
        var floatInst = instruction as FloatInstruction;
        var intInst = instruction as IntegerInstruction;
        var strInst = instruction as StringInstruction;
        
        _statementBuilder.Append(disassembly);
        switch (instruction.OpCode)
        {
            case OperationType.LoadNull:
                _decompiledStatements.Push("null");
                return null;
            case OperationType.LoadTrue:
                _decompiledStatements.Push("true");
                return null;
            case OperationType.LoadFalse:
                _decompiledStatements.Push("false");
                return null;
            case OperationType.LoadFloat:
                _decompiledStatements.Push($"{floatInst!.Value}f");
                return null;
            case OperationType.LoadDecimal:
                _decompiledStatements.Push($"{strInst!.Value}d");
                return null;
            case OperationType.LoadInt:
                _decompiledStatements.Push($"{intInst!.Value}");
                return null;
            case OperationType.LoadStr:
                _decompiledStatements.Push(
                    $"{HttpUtility.JavaScriptStringEncode(strInst!.Value, true)}");
                return null;
            case OperationType.LoadUri:
                _decompiledStatements.Push($"`{HttpUtility.JavaScriptStringEncode(strInst!.Value)}`");
                return null;
            case OperationType.LoadDuration:
                _decompiledStatements.Push($"{intInst!.Value}ns");
                return null;
            case OperationType.LoadType:
                _decompiledStatements.Push($"{typeInst!.Value}#");
                return null;
            case OperationType.LoadVar:
                _decompiledStatements.Push(registerInst!.Value?.Name ?? "this");
                return null;
            case OperationType.LoadInstance when _decompiledStatements.Count > 0:
                _decompiledStatements.Push($"{_decompiledStatements.Pop()}.{(instruction as FieldInstruction)!.Value.Name}");
                return null;
            case OperationType.LoadStatic or OperationType.LoadMixinStatic:
            {
                _decompiledStatements.Push($"{fieldInst!.Value.Type}.{fieldInst.Value.Name}");
                return null;
            }
            case OperationType.CallNew when _decompiledStatements.Count >= methodInst!.Value.Parameters.Length:
            {
                var paramCount = methodInst.Value.Parameters.Length;
                List<string> parameters = [];
                for (var i = 0; i < paramCount; i++) parameters.Insert(0, _decompiledStatements.Pop());
                _decompiledStatements.Push($"{methodInst.Value.ParentType}.{methodInst.Value.Name}({string.Join(", ", parameters)})");
                return null;
            }
            case OperationType.CallVirtual or OperationType.CallNonVirtual or OperationType.CallMixinVirtual
                or OperationType.CallMixinNonVirtual
                when _decompiledStatements.Count >= methodInst!.Value.Parameters.Length+1:
            {
                var paramCount = methodInst.Value.Parameters.Length;
                List<string> parameters = [];
                for (var i = 0; i < paramCount; i++) parameters.Insert(0, _decompiledStatements.Pop());
                var instance = _decompiledStatements.Pop();
                var statement = $"{instance}.{methodInst.Value.Name}({string.Join(", ", parameters)})";
                if (methodInst.Value.ReturnType == TypeReference.Void)
                {
                    if (_decompiledStatements.Count == 0)
                        return GenerateDisassemblyComment(statement, decompilationPadding);
                    var result = _statementBuilder.ToString();
                    _statementBuilder.Clear();
                    _decompiledStatements.Clear();
                    return result;

                }
                _decompiledStatements.Push(statement);
                return null;
            }
            case OperationType.CallStatic or OperationType.CallMixinStatic
                when _decompiledStatements.Count >= methodInst!.Value.Parameters.Length:
            {
                var paramCount = methodInst.Value.Parameters.Length;
                List<string> parameters = [];
                for (var i = 0; i < paramCount; i++) parameters.Insert(0, _decompiledStatements.Pop());
                var statement =
                    $"{methodInst.Value.ParentType}.{methodInst.Value.Name}({string.Join(", ", parameters)})";
                if (methodInst.Value.ReturnType == TypeReference.Void)
                {
                    if (_decompiledStatements.Count == 0)
                        return GenerateDisassemblyComment(statement, decompilationPadding);
                    var result = _statementBuilder.ToString();
                    _statementBuilder.Clear();
                    _decompiledStatements.Clear();
                    return result;

                }
                _decompiledStatements.Push(statement);
                return null;
            }
            case OperationType.CompareEq when _decompiledStatements.Count >= 2:
            {
                var b = _decompiledStatements.Pop();
                var a = _decompiledStatements.Pop();
                _decompiledStatements.Push($"({a} == {b})");
                return null;
            }
            case OperationType.CompareNe when _decompiledStatements.Count >= 2:
            {
                var b = _decompiledStatements.Pop();
                var a = _decompiledStatements.Pop();
                _decompiledStatements.Push($"({a} != {b})");
                return null;
            }
            case OperationType.Compare when _decompiledStatements.Count >= 2:
            {
                var b = _decompiledStatements.Pop();
                var a = _decompiledStatements.Pop();
                _decompiledStatements.Push($"({a}.compare({b}))");
                return null;
            }
            case OperationType.CompareLe when _decompiledStatements.Count >= 2:
            {
                var b = _decompiledStatements.Pop();
                var a = _decompiledStatements.Pop();
                _decompiledStatements.Push($"({a} <= {b})");
                return null;
            }
            case OperationType.CompareLt when _decompiledStatements.Count >= 2:
            {
                var b = _decompiledStatements.Pop();
                var a = _decompiledStatements.Pop();
                _decompiledStatements.Push($"({a} < {b})");
                return null;
            }
            case OperationType.CompareGe when _decompiledStatements.Count >= 2:
            {
                var b = _decompiledStatements.Pop();
                var a = _decompiledStatements.Pop();
                _decompiledStatements.Push($"({a} >= {b})");
                return null;
            }
            case OperationType.CompareGt when _decompiledStatements.Count >= 2:
            {
                var b = _decompiledStatements.Pop();
                var a = _decompiledStatements.Pop();
                _decompiledStatements.Push($"({a} > {b})");
                return null;
            }
            case OperationType.CompareSame when _decompiledStatements.Count >= 2:
            {
                var b = _decompiledStatements.Pop();
                var a = _decompiledStatements.Pop();
                _decompiledStatements.Push($"({a} === {b})");
                return null;
            }
            case OperationType.CompareNotSame when _decompiledStatements.Count >= 2:
            {
                var b = _decompiledStatements.Pop();
                var a = _decompiledStatements.Pop();
                _decompiledStatements.Push($"({a} !== {b})");
                return null;
            }
            case OperationType.CompareNull when _decompiledStatements.Count >= 1:
                _decompiledStatements.Push($"({_decompiledStatements.Pop()} == null)");
                return null;
            case OperationType.CompareNotNull when _decompiledStatements.Count >= 1:
                _decompiledStatements.Push($"({_decompiledStatements.Pop()} != null)");
                return null;
            case OperationType.As when _decompiledStatements.Count > 0:
            {
                var statement = _decompiledStatements.Pop();
                _decompiledStatements.Push($"({statement} as {typeInst!.Value})");
                return null;
            }
            case OperationType.Is when _decompiledStatements.Count > 0:
            {
                var statement = _decompiledStatements.Pop();
                _decompiledStatements.Push($"({statement} is {typeInst!.Value})");
                return null;
            }
            case OperationType.Coerce:
                return null;
            case OperationType.Return when _decompiledStatements.Count == (isVoidMethod ? 0 : 1):
                return GenerateDisassemblyComment(isVoidMethod ? "return" : $"return {_decompiledStatements.Pop()}", decompilationPadding);
            case OperationType.Jump or OperationType.Leave when jumpInst is { Target.OpCode: OperationType.Return } && _decompiledStatements.Count == (isVoidMethod ? 0 : 1):
                return GenerateDisassemblyComment(isVoidMethod ? "return" : $"return {_decompiledStatements.Pop()}", decompilationPadding);
            case OperationType.JumpTrue when _decompiledStatements.Count == 1:
                return GenerateDisassemblyComment(
                    $"if ({_decompiledStatements.Pop()}) goto {labels[jumpInst!.Target]}",
                    decompilationPadding);
            case OperationType.JumpFalse when _decompiledStatements.Count == 1:
                return GenerateDisassemblyComment(
                    $"if ({_decompiledStatements.Pop()} == false) goto {labels[jumpInst!.Target]}",
                    decompilationPadding);
            case OperationType.Switch when _decompiledStatements.Count == 1:
                // For this one, you can see which values go where
                return GenerateDisassemblyComment($"switch ({_decompiledStatements.Pop()})", decompilationPadding);
            case OperationType.StoreVar when _decompiledStatements.Count == 1:
            {
                var value = _decompiledStatements.Pop();
                return GenerateDisassemblyComment($"{registerInst!.Value?.Name ?? "this"} = {value}",
                    decompilationPadding);
            }
            case OperationType.StoreStatic or OperationType.StoreMixinStatic when _decompiledStatements.Count == 1:
            {
                var value = _decompiledStatements.Pop();
                return GenerateDisassemblyComment($"{fieldInst!.Value.Type}.{fieldInst.Value.Name} = {value}",
                    decompilationPadding);
            }
            case OperationType.StoreInstance when _decompiledStatements.Count == 2:
            {
                var value = _decompiledStatements.Pop();
                var instance = _decompiledStatements.Pop();
                return GenerateDisassemblyComment(
                    $"{instance}.{fieldInst!.Value.Name} = {value}", decompilationPadding);
            }
            case OperationType.Pop when _decompiledStatements.Count == 1:
            {
                var last = _decompiledStatements.Pop();
                return GenerateDisassemblyComment(last, decompilationPadding);
            }
            default:
            {
                // At this point, we have an unknown decompilation, so we just spit out the whole block
                var result = _statementBuilder.ToString(); // trim the end so that there's no gap unless
                _statementBuilder.Clear();
                _decompiledStatements.Clear();
                return result;
            }
        }
    }

    private string GenerateDisassemblyComment(string disassembly, int decompilationPadding)
    {
        var padded = new string(' ', decompilationPadding);
        var result = $"\n{padded}/* BEGIN: {disassembly} */\n{_statementBuilder}{padded}/* END */\n";
        _statementBuilder.Clear();
        _decompiledStatements.Clear();
        return result;
    }

    // Used to terminate a statement in case of something like the end of a block or something
    public string EndStatement() => _statementBuilder.ToString();
}
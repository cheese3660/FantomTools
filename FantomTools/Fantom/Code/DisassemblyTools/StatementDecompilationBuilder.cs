using System.Text;
using System.Web;
using FantomTools.Fantom.Code.Instructions;
using FantomTools.Fantom.Code.Operations;

namespace FantomTools.Fantom.Code.DisassemblyTools;

internal class StatementDecompilationBuilder
{
    private Stack<string> _decompiledStatements = new();
    private StringBuilder _statementBuilder = new();

    // Now we have to understand the 2 null safety operators behaviors,
    // Which will both trigger on a `dup` instruction, and should hopefully be detectable :3

    // x?.y(...)
    //                  ... put x on stack ...
    //                  dup
    //                  cmp.null x
    //                  jmp.true PROPAGATE_NULL
    //                  ... push arguments on stack ...
    //                  call(.virtual) y(...) -> ...
    //                  jmp NON_NULL
    // PROPAGATE_NULL:  pop
    //                  [push null if non-void method]
    // NON_NULL:        continue as normal
    //
    
    // x ?: y
    //          ... put x on stack ...
    //          dup
    //          cmp.null
    //          jmp.true ON_NULL
    //          jmp ON_NON_NULL
    // ON_NULL: pop
    //          ... put y on stack ... 
    // ON_NON_NULL:
    //          ... continue ...

    private NullSafetyState _nullSafety = NullSafetyState.NoSafety;
    private Instruction? _lastNullSafetyJump = null;
    private bool _lastCallWasVoid = false;
    private int _nullPropagationCount = 0;
    private Stack<Instruction> _elvisTargets = [];

    private enum NullSafetyState
    {
        // This is the train for null safety
        NoSafety,
        JustDuplicated,
        JustCompared,
        JustJumped,
        
        JustDidNullPropagationSkip,
        ExpectPushNull,
        ExpectElvisPop,
    }

    private enum NullSafetyType
    {
        Elvis,
        Propagation
    }

    public string? Consume(Instruction instruction, string disassembly, int decompilationPadding, bool isVoidMethod,
        Dictionary<Instruction, string> labels)
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
        if (_elvisTargets.Count > 0 && _elvisTargets.Peek() == instruction && _decompiledStatements.Count >= 2)
        {
            _elvisTargets.Pop();
            var onNull = _decompiledStatements.Pop();
            var def = _decompiledStatements.Pop();
            _decompiledStatements.Push($"({def} ?: {onNull})");
        }
        switch (_nullSafety)
        {
            case NullSafetyState.JustDidNullPropagationSkip when instruction.OpCode == OperationType.Pop:
                if (_lastCallWasVoid)
                {
                    _nullSafety = NullSafetyState.NoSafety;
                    return null;
                }
                _nullSafety = NullSafetyState.ExpectPushNull;
                return null;
            case NullSafetyState.ExpectPushNull:
                if (instruction.OpCode != OperationType.LoadNull)
                    return UnknownDisassembly;
                _nullSafety = NullSafetyState.NoSafety;
                return null;
            case NullSafetyState.JustDuplicated when instruction.OpCode == OperationType.CompareNull:
                _nullSafety = NullSafetyState.JustCompared;
                return null;
            case NullSafetyState.JustCompared when instruction.OpCode == OperationType.JumpTrue:
                _nullSafety = NullSafetyState.JustJumped;
                _lastNullSafetyJump = jumpInst!.Target;
                return null;
            case NullSafetyState.JustJumped when instruction.OpCode == OperationType.Jump:
                _elvisTargets.Push(jumpInst!.Target);
                _nullSafety = NullSafetyState.ExpectElvisPop;
                return null;
            case NullSafetyState.ExpectElvisPop when instruction.OpCode == OperationType.Pop:
                _nullSafety = NullSafetyState.NoSafety;
                return null;
            case NullSafetyState.JustJumped when instruction.OpCode != OperationType.Jump:
                // Now we know that this is meant to be a null propagation operator, so we can add a question mark to the top statement
                _decompiledStatements.Push($"{_decompiledStatements.Pop()}?");
                _nullSafety = NullSafetyState.NoSafety;
                _nullPropagationCount += 1;
                goto case NullSafetyState.NoSafety;
            case NullSafetyState.NoSafety:
                switch (instruction.OpCode)
                {
                    case OperationType.Dup when _decompiledStatements.Count >= 1:
                        // Now we must go into the just duplicated state
                        _nullSafety = NullSafetyState.JustDuplicated;
                        return null;
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
                        _decompiledStatements.Push(
                            $"{_decompiledStatements.Pop()}.{(instruction as FieldInstruction)!.Value.Name}");
                        return null;
                    case OperationType.Throw when _decompiledStatements.Count == 1:
                        return GenerateDisassemblyComment($"throw {_decompiledStatements.Pop()}", decompilationPadding);
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
                        _decompiledStatements.Push(
                            $"{methodInst.Value.ParentType}.{methodInst.Value.Name}({string.Join(", ", parameters)})");
                        return null;
                    }
                    case OperationType.CallVirtual or OperationType.CallNonVirtual or OperationType.CallMixinVirtual
                        or OperationType.CallMixinNonVirtual
                        when _decompiledStatements.Count >= methodInst!.Value.Parameters.Length + 1:
                    {
                        var paramCount = methodInst.Value.Parameters.Length;
                        List<string> parameters = [];
                        for (var i = 0; i < paramCount; i++) parameters.Insert(0, _decompiledStatements.Pop());
                        var instance = _decompiledStatements.Pop();
                        var statement = $"{instance}.{methodInst.Value.Name}({string.Join(", ", parameters)})";
                        if (methodInst.Value.ReturnType == TypeReference.Void)
                        {
                            _lastCallWasVoid = true;
                            switch (_nullPropagationCount)
                            {
                                case 0:
                                    return _decompiledStatements.Count == 0
                                        ? GenerateDisassemblyComment(statement, decompilationPadding)
                                        : UnknownDisassembly;
                                case 1:
                                    return null;
                                case > 1:
                                    return UnknownDisassembly;
                            }
                        }
                        _lastCallWasVoid = false;
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
                            return _decompiledStatements.Count == 0 ? GenerateDisassemblyComment(statement, decompilationPadding) : UnknownDisassembly;
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
                        return GenerateDisassemblyComment(
                            isVoidMethod ? "return" : $"return {_decompiledStatements.Pop()}", decompilationPadding);
                    case OperationType.Jump when _nullPropagationCount > 0:
                    {
                        Console.WriteLine($"JustDidNullPropagationSkip");
                        _nullPropagationCount -= 1;
                        _nullSafety = NullSafetyState.JustDidNullPropagationSkip;
                        return null;
                    }
                    case OperationType.Jump or OperationType.Leave
                        when jumpInst is { Target.OpCode: OperationType.Return } &&
                             _decompiledStatements.Count == (isVoidMethod ? 0 : 1):
                        return GenerateDisassemblyComment(
                            isVoidMethod ? "return" : $"return {_decompiledStatements.Pop()}", decompilationPadding);
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
                        return GenerateDisassemblyComment($"switch ({_decompiledStatements.Pop()})",
                            decompilationPadding);
                    case OperationType.StoreVar when _decompiledStatements.Count == 1:
                    {
                        var value = _decompiledStatements.Pop();
                        return GenerateDisassemblyComment($"{registerInst!.Value?.Name ?? "this"} = {value}",
                            decompilationPadding);
                    }
                    case OperationType.StoreStatic or OperationType.StoreMixinStatic
                        when _decompiledStatements.Count == 1:
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
                        return UnknownDisassembly;
                }
            default:
                return UnknownDisassembly;
        }
    }

    private string GenerateDisassemblyComment(string disassembly, int decompilationPadding)
    {
        var padded = new string(' ', decompilationPadding);
        var result = $"\n{padded}/* BEGIN: {disassembly} */\n{_statementBuilder}{padded}/* END */\n";
        _statementBuilder.Clear();
        _decompiledStatements.Clear();
        _nullPropagationCount = 0;
        return result;
    }

    private string UnknownDisassembly
    {
        get
        {
            var result = _statementBuilder.ToString(); // trim the end so that there's no gap unless
            _statementBuilder.Clear();
            _decompiledStatements.Clear();
            _nullSafety = NullSafetyState.NoSafety;
            _nullPropagationCount = 0;
            return result;
        }
    }

    // Used to terminate a statement in case of something like the end of a block or something
    public string EndStatement() => _statementBuilder.ToString();
}
using FantomTools.Fantom.Code.Instructions;
using FantomTools.Fantom.Code.Operations;

namespace FantomTools.Fantom.Code;

public static class InstructionHelper
{
    public static Instruction Nop => new()
    {
        OpCode = OperationType.Nop
    };

    public static Instruction LoadNull => new()
    {
        OpCode = OperationType.LoadNull
    };

    public static Instruction LoadFalse => new()
    {
        OpCode = OperationType.LoadFalse
    };
    
    public static Instruction LoadTrue => new()
    {
        OpCode = OperationType.LoadTrue
    };

    public static Instruction LoadInt(long value) => new IntegerInstruction
    {
        OpCode = OperationType.LoadInt,
        Value = value
    };

    public static Instruction LoadFloat(double value) => new FloatInstruction
    {
        OpCode = OperationType.LoadFloat,
        Value = value
    };

    public static Instruction LoadDecimal(string dec) => new StringInstruction
    {
        OpCode = OperationType.LoadDecimal,
        Value = dec
    };

    public static Instruction LoadStr(string str) => new StringInstruction
    {
        OpCode = OperationType.LoadStr,
        Value = str
    };

    public static Instruction LoadDuration(long ticks) => new IntegerInstruction
    {
        OpCode = OperationType.LoadDuration,
        Value = ticks
    };

    public static Instruction LoadType(TypeReference type) => new TypeInstruction
    {
        OpCode = OperationType.LoadType,
        Value = type,
    };

    public static Instruction LoadUri(string uri) => new StringInstruction
    {
        OpCode = OperationType.LoadUri,
        Value = uri
    };

    public static Instruction LoadVar(MethodVariable variable) => new RegisterInstruction
    {
        OpCode = OperationType.LoadVar,
        Value = variable
    };

    public static Instruction StoreVar(MethodVariable variable) => new RegisterInstruction
    {
        OpCode = OperationType.StoreVar,
        Value = variable
    };

    public static Instruction LoadInstance(FieldReference field) => new FieldInstruction
    {
        OpCode = OperationType.LoadInstance,
        Value = field
    };
    
    public static Instruction StoreInstance(FieldReference field) => new FieldInstruction
    {
        OpCode = OperationType.StoreInstance,
        Value = field
    };
    
    public static Instruction LoadStatic(FieldReference field) => new FieldInstruction
    {
        OpCode = OperationType.LoadStatic,
        Value = field
    };
    
    public static Instruction StoreStatic(FieldReference field) => new FieldInstruction
    {
        OpCode = OperationType.StoreStatic,
        Value = field
    };
    
    public static Instruction LoadMixinStatic(FieldReference field) => new FieldInstruction
    {
        OpCode = OperationType.LoadMixinStatic,
        Value = field
    };
    
    public static Instruction StoreMixinStatic(FieldReference field) => new FieldInstruction
    {
        OpCode = OperationType.StoreMixinStatic,
        Value = field
    };

    public static Instruction CallNew(MethodReference method) => new MethodInstruction
    {
        OpCode = OperationType.CallNew,
        Value = method
    };
    
    public static Instruction CallCtor(MethodReference method) => new MethodInstruction
    {
        OpCode = OperationType.CallCtor,
        Value = method
    };
    
    public static Instruction CallStatic(MethodReference method) => new MethodInstruction
    {
        OpCode = OperationType.CallStatic,
        Value = method
    };
    
    public static Instruction CallVirtual(MethodReference method) => new MethodInstruction
    {
        OpCode = OperationType.CallVirtual,
        Value = method
    };
    
    public static Instruction CallNonVirtual(MethodReference method) => new MethodInstruction
    {
        OpCode = OperationType.CallNonVirtual,
        Value = method
    };
    
    public static Instruction CallMixinStatic(MethodReference method) => new MethodInstruction
    {
        OpCode = OperationType.CallMixinStatic,
        Value = method
    };
    
    public static Instruction CallMixinVirtual(MethodReference method) => new MethodInstruction
    {
        OpCode = OperationType.CallMixinVirtual,
        Value = method
    };

    public static Instruction CallMixinNonVirtual(MethodReference method) => new MethodInstruction
    {
        OpCode = OperationType.CallMixinNonVirtual,
        Value = method
    };

    public static Instruction Jump(Instruction target) => new JumpInstruction
    {
        OpCode = OperationType.Jump,
        Target = target
    };
    
    public static Instruction JumpTrue(Instruction target) => new JumpInstruction
    {
        OpCode = OperationType.JumpTrue,
        Target = target
    };
    
    public static Instruction JumpFalse(Instruction target) => new JumpInstruction
    {
        OpCode = OperationType.JumpFalse,
        Target = target
    };

    public static Instruction CompareEq(TypeReference t1, TypeReference t2) => new TypePairInstruction
    {
        OpCode = OperationType.CompareEq,
        FirstType = t1,
        SecondType = t2
    };

    public static Instruction CompareNe(TypeReference t1, TypeReference t2) => new TypePairInstruction
    {
        OpCode = OperationType.CompareNe,
        FirstType = t1,
        SecondType = t2
    };

    public static Instruction Compare(TypeReference t1, TypeReference t2) => new TypePairInstruction
    {
        OpCode = OperationType.Compare,
        FirstType = t1,
        SecondType = t2
    };

    public static Instruction CompareLe(TypeReference t1, TypeReference t2) => new TypePairInstruction
    {
        OpCode = OperationType.CompareLe,
        FirstType = t1,
        SecondType = t2
    };

    public static Instruction CompareLt(TypeReference t1, TypeReference t2) => new TypePairInstruction
    {
        OpCode = OperationType.CompareLt,
        FirstType = t1,
        SecondType = t2
    };

    public static Instruction CompareGe(TypeReference t1, TypeReference t2) => new TypePairInstruction
    {
        OpCode = OperationType.CompareGe,
        FirstType = t1,
        SecondType = t2
    };

    public static Instruction CompareGt(TypeReference t1, TypeReference t2) => new TypePairInstruction
    {
        OpCode = OperationType.CompareGt,
        FirstType = t1,
        SecondType = t2
    };
    
    public static Instruction CompareSame => new()
    {
        OpCode = OperationType.CompareSame
    };
    
    public static Instruction CompareNotSame => new()
    {
        OpCode = OperationType.CompareNotSame
    };

    public static Instruction CompareNull(TypeReference t) => new TypeInstruction
    {
        OpCode = OperationType.CompareNull,
        Value = t
    };

    public static Instruction CompareNotNull(TypeReference t) => new TypeInstruction
    {
        OpCode = OperationType.CompareNotNull,
        Value = t
    };

    public static Instruction Return => new()
    {
        OpCode = OperationType.Return
    };
    
    public static Instruction Pop(TypeReference t) => new TypeInstruction
    {
        OpCode = OperationType.Pop,
        Value = t
    };
    
    public static Instruction Dup(TypeReference t) => new TypeInstruction
    {
        OpCode = OperationType.Dup,
        Value = t
    };
    
    public static Instruction Is(TypeReference t) => new TypeInstruction
    {
        OpCode = OperationType.Is,
        Value = t
    };
    
    public static Instruction As(TypeReference t) => new TypeInstruction
    {
        OpCode = OperationType.As,
        Value = t
    };

    public static Instruction Coerce(TypeReference t1, TypeReference t2) => new TypePairInstruction
    {
        OpCode = OperationType.Coerce,
        FirstType = t1,
        SecondType = t2
    };

    public static Instruction Switch(params Instruction[] targets) => new SwitchInstruction
    {
        OpCode = OperationType.Switch,
        JumpTargets = targets.ToList(),
    };
    
    public static Instruction Throw => new()
    {
        OpCode = OperationType.Throw
    };
    
    public static Instruction Leave(Instruction target) => new JumpInstruction
    {
        OpCode = OperationType.Leave,
        Target = target
    };
    
    public static Instruction JumpFinally(Instruction target) => new JumpInstruction
    {
        OpCode = OperationType.JumpFinally,
        Target = target
    };
    
    public static Instruction CatchAllStart => new()
    {
        OpCode = OperationType.CatchAllStart
    };
    
    public static Instruction CatchErrStart(TypeReference t) => new TypeInstruction
    {
        OpCode = OperationType.CatchErrStart,
        Value = t
    };
    
    public static Instruction CatchEnd => new()
    {
        OpCode = OperationType.CatchEnd
    };

    public static Instruction FinallyStart => new()
    {
        OpCode = OperationType.FinallyStart
    };

    public static Instruction FinallyEnd => new()
    {
        OpCode = OperationType.FinallyEnd
    };
}
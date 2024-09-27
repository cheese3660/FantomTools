using FantomTools.Fantom.Code.Instructions;
using FantomTools.Fantom.Code.Operations;

namespace FantomTools.Fantom.Code;

/// <summary>
/// Contains a bunch of helper methods and getters to generate instructions
/// </summary>
public static class InstructionHelper
{
    /// <summary>
    /// Generates a "nop" instruction
    /// </summary>
    public static Instruction Nop => new()
    {
        OpCode = OperationType.Nop
    };

    /// <summary>
    /// Generates a "ld.null" instruction
    /// </summary>
    public static Instruction LoadNull => new()
    {
        OpCode = OperationType.LoadNull
    };

    /// <summary>
    /// Generates a "ld.false" instruction
    /// </summary>
    public static Instruction LoadFalse => new()
    {
        OpCode = OperationType.LoadFalse
    };
    
    /// <summary>
    /// Generates a "ld.true" instruction
    /// </summary>
    public static Instruction LoadTrue => new()
    {
        OpCode = OperationType.LoadTrue
    };

    /// <summary>
    /// Generates a "ld.int" instruction
    /// </summary>
    /// <param name="value">The integer to push</param>
    /// <returns>The generated instruction</returns>
    public static Instruction LoadInt(long value) => new IntegerInstruction
    {
        OpCode = OperationType.LoadInt,
        Value = value
    };
    
    /// <summary>
    /// Generates a "ld.float" instruction
    /// </summary>
    /// <param name="value">The float to push</param>
    /// <returns>The generated instruction</returns>
    public static Instruction LoadFloat(double value) => new FloatInstruction
    {
        OpCode = OperationType.LoadFloat,
        Value = value
    };

    /// <summary>
    /// Generates a "ld.decimal" instruction
    /// </summary>
    /// <param name="value">The decimal to push</param>
    /// <returns>The generated instruction</returns>
    public static Instruction LoadDecimal(string value) => new StringInstruction
    {
        OpCode = OperationType.LoadDecimal,
        Value = value
    };


    /// <summary>
    /// Generates a "ld.str" instruction
    /// </summary>
    /// <param name="value">The string to push</param>
    /// <returns>The generated instruction</returns>
    public static Instruction LoadStr(string value) => new StringInstruction
    {
        OpCode = OperationType.LoadStr,
        Value = value
    };


    /// <summary>
    /// Generates a "ld.duration" instruction
    /// </summary>
    /// <param name="ticks">The duration to push in ticks</param>
    /// <returns>The generated instruction</returns>
    public static Instruction LoadDuration(long ticks) => new IntegerInstruction
    {
        OpCode = OperationType.LoadDuration,
        Value = ticks
    };

    /// <summary>
    /// Generates a "ld.type" instruction
    /// </summary>
    /// <param name="type">The type to push</param>
    /// <returns>The generated instruction</returns>
    public static Instruction LoadType(TypeReference type) => new TypeInstruction
    {
        OpCode = OperationType.LoadType,
        Value = type,
    };


    /// <summary>
    /// Generates a "ld.uri" instruction
    /// </summary>
    /// <param name="value">The uri to push</param>
    /// <returns>The generated instruction</returns>
    public static Instruction LoadUri(string value) => new StringInstruction
    {
        OpCode = OperationType.LoadUri,
        Value = value
    };

    /// <summary>
    /// Generates a "ld.var" instruction
    /// </summary>
    /// <param name="variable">The variable to push, if it is null, it loads the `this` variable</param>
    /// <returns>The generated instruction</returns>
    public static Instruction LoadVar(MethodVariable? variable) => new RegisterInstruction
    {
        OpCode = OperationType.LoadVar,
        Value = variable
    };
    
    /// <summary>
    /// Generates a "ld.var this" instruction
    /// </summary>
    public static Instruction LoadThis => new RegisterInstruction
    {
        OpCode = OperationType.LoadVar,
        Value = null
    };

    /// <summary>
    /// Generates a "st.var" instruction
    /// </summary>
    /// <param name="variable">The variable to store into</param>
    /// <returns>The generated instruction</returns>
    public static Instruction StoreVar(MethodVariable variable) => new RegisterInstruction
    {
        OpCode = OperationType.StoreVar,
        Value = variable
    };

    /// <summary>
    /// Generates a "ld.instance" instruction
    /// </summary>
    /// <param name="field">The field to read from and push</param>
    /// <returns>The generated instruction</returns>
    public static Instruction LoadInstance(FieldReference field) => new FieldInstruction
    {
        OpCode = OperationType.LoadInstance,
        Value = field
    };
    
    /// <summary>
    /// Generates a "st.instance" instruction
    /// </summary>
    /// <param name="field">The field to store into</param>
    /// <returns>The generated instruction</returns>
    public static Instruction StoreInstance(FieldReference field) => new FieldInstruction
    {
        OpCode = OperationType.StoreInstance,
        Value = field
    };
    
    /// <summary>
    /// Generates a "ld.static" instruction
    /// </summary>
    /// <param name="field">The field to read from and push</param>
    /// <returns>The generated instruction</returns>
    public static Instruction LoadStatic(FieldReference field) => new FieldInstruction
    {
        OpCode = OperationType.LoadStatic,
        Value = field
    };
    
    
    /// <summary>
    /// Generates a "st.static" instruction
    /// </summary>
    /// <param name="field">The field to store into</param>
    /// <returns>The generated instruction</returns>
    public static Instruction StoreStatic(FieldReference field) => new FieldInstruction
    {
        OpCode = OperationType.StoreStatic,
        Value = field
    };
    
    
    /// <summary>
    /// Generates a "ld.mixin" instruction
    /// </summary>
    /// <param name="field">The field to read from and push</param>
    /// <returns>The generated instruction</returns>
    public static Instruction LoadMixinStatic(FieldReference field) => new FieldInstruction
    {
        OpCode = OperationType.LoadMixinStatic,
        Value = field
    };
    
    /// <summary>
    /// Generates a "st.mixin" instruction
    /// </summary>
    /// <param name="field">The field to store into</param>
    /// <returns>The generated instruction</returns>
    public static Instruction StoreMixinStatic(FieldReference field) => new FieldInstruction
    {
        OpCode = OperationType.StoreMixinStatic,
        Value = field
    };

    
    /// <summary>
    /// Generates a "new" instruction
    /// </summary>
    /// <param name="method">The constructor to call</param>
    /// <returns>The generated instruction</returns>
    public static Instruction CallNew(MethodReference method) => new MethodInstruction
    {
        OpCode = OperationType.CallNew,
        Value = method
    };
    
    /// <summary>
    /// Generates a "call.ctor" instruction
    /// </summary>
    /// <param name="method">The constructor to call</param>
    /// <returns>The generated instruction</returns>
    public static Instruction CallCtor(MethodReference method) => new MethodInstruction
    {
        OpCode = OperationType.CallCtor,
        Value = method
    };
    
    /// <summary>
    /// Generates a "call.static" instruction
    /// </summary>
    /// <param name="method">The method to call</param>
    /// <returns>The generated instruction</returns>
    public static Instruction CallStatic(MethodReference method) => new MethodInstruction
    {
        OpCode = OperationType.CallStatic,
        Value = method
    };
    
    /// <summary>
    /// Generates a "call.virtual" instruction
    /// </summary>
    /// <param name="method">The method to call</param>
    /// <returns>The generated instruction</returns>
    public static Instruction CallVirtual(MethodReference method) => new MethodInstruction
    {
        OpCode = OperationType.CallVirtual,
        Value = method
    };
    
    /// <summary>
    /// Generates a "call" instruction
    /// </summary>
    /// <param name="method">The method to call</param>
    /// <returns>The generated instruction</returns>
    public static Instruction CallNonVirtual(MethodReference method) => new MethodInstruction
    {
        OpCode = OperationType.CallNonVirtual,
        Value = method
    };
    
    /// <summary>
    /// Generates a "call.mixin.static" instruction
    /// </summary>
    /// <param name="method">The method to call</param>
    /// <returns>The generated instruction</returns>
    public static Instruction CallMixinStatic(MethodReference method) => new MethodInstruction
    {
        OpCode = OperationType.CallMixinStatic,
        Value = method
    };
    
    /// <summary>
    /// Generates a "call.mixin.virtual" instruction
    /// </summary>
    /// <param name="method">The method to call</param>
    /// <returns>The generated instruction</returns>
    public static Instruction CallMixinVirtual(MethodReference method) => new MethodInstruction
    {
        OpCode = OperationType.CallMixinVirtual,
        Value = method
    };
    
    /// <summary>
    /// Generates a "call.mixin" instruction
    /// </summary>
    /// <param name="method">The method to call</param>
    /// <returns>The generated instruction</returns>
    public static Instruction CallMixinNonVirtual(MethodReference method) => new MethodInstruction
    {
        OpCode = OperationType.CallMixinNonVirtual,
        Value = method
    };

    /// <summary>
    /// Generates a "jmp" instruction
    /// </summary>
    /// <param name="target">The target to jump to</param>
    /// <returns>The generated instruction</returns>
    public static Instruction Jump(Instruction target) => new JumpInstruction
    {
        OpCode = OperationType.Jump,
        Target = target
    };
    
    /// <summary>
    /// Generates a "jmp.true" instruction
    /// </summary>
    /// <param name="target">The target to jump to</param>
    /// <returns>The generated instruction</returns>
    public static Instruction JumpTrue(Instruction target) => new JumpInstruction
    {
        OpCode = OperationType.JumpTrue,
        Target = target
    };
    
    /// <summary>
    /// Generates a "jmp.false" instruction
    /// </summary>
    /// <param name="target">The target to jump to</param>
    /// <returns>The generated instruction</returns>    
    public static Instruction JumpFalse(Instruction target) => new JumpInstruction
    {
        OpCode = OperationType.JumpFalse,
        Target = target
    };

    /// <summary>
    /// Generates a "cmp.eq" instruction
    /// </summary>
    /// <param name="t1">The type of the first parameter</param>
    /// <param name="t2">The type of the second parameter</param>
    /// <returns>The generated instruction</returns>
    public static Instruction CompareEq(TypeReference t1, TypeReference t2) => new TypePairInstruction
    {
        OpCode = OperationType.CompareEq,
        FirstType = t1,
        SecondType = t2
    };
    
    /// <summary>
    /// Generates a "cmp.ne" instruction
    /// </summary>
    /// <param name="t1">The type of the first parameter</param>
    /// <param name="t2">The type of the second parameter</param>
    /// <returns>The generated instruction</returns>
    public static Instruction CompareNe(TypeReference t1, TypeReference t2) => new TypePairInstruction
    {
        OpCode = OperationType.CompareNe,
        FirstType = t1,
        SecondType = t2
    };
    
    /// <summary>
    /// Generates a "cmp" instruction
    /// </summary>
    /// <param name="t1">The type of the first parameter</param>
    /// <param name="t2">The type of the second parameter</param>
    /// <returns>The generated instruction</returns>
    public static Instruction Compare(TypeReference t1, TypeReference t2) => new TypePairInstruction
    {
        OpCode = OperationType.Compare,
        FirstType = t1,
        SecondType = t2
    };

    /// <summary>
    /// Generates a "cmp.le" instruction
    /// </summary>
    /// <param name="t1">The type of the first parameter</param>
    /// <param name="t2">The type of the second parameter</param>
    /// <returns>The generated instruction</returns>
    public static Instruction CompareLe(TypeReference t1, TypeReference t2) => new TypePairInstruction
    {
        OpCode = OperationType.CompareLe,
        FirstType = t1,
        SecondType = t2
    };
    
    /// <summary>
    /// Generates a "cmp.lt" instruction
    /// </summary>
    /// <param name="t1">The type of the first parameter</param>
    /// <param name="t2">The type of the second parameter</param>
    /// <returns>The generated instruction</returns>
    public static Instruction CompareLt(TypeReference t1, TypeReference t2) => new TypePairInstruction
    {
        OpCode = OperationType.CompareLt,
        FirstType = t1,
        SecondType = t2
    };

    /// <summary>
    /// Generates a "cmp.ge" instruction
    /// </summary>
    /// <param name="t1">The type of the first parameter</param>
    /// <param name="t2">The type of the second parameter</param>
    /// <returns>The generated instruction</returns>
    public static Instruction CompareGe(TypeReference t1, TypeReference t2) => new TypePairInstruction
    {
        OpCode = OperationType.CompareGe,
        FirstType = t1,
        SecondType = t2
    };
    
    /// <summary>
    /// Generates a "cmp.gt" instruction
    /// </summary>
    /// <param name="t1">The type of the first parameter</param>
    /// <param name="t2">The type of the second parameter</param>
    /// <returns>The generated instruction</returns>
    public static Instruction CompareGt(TypeReference t1, TypeReference t2) => new TypePairInstruction
    {
        OpCode = OperationType.CompareGt,
        FirstType = t1,
        SecondType = t2
    };
    
    /// <summary>
    /// Generates a "cmp.same" instruction
    /// </summary>
    public static Instruction CompareSame => new()
    {
        OpCode = OperationType.CompareSame
    };
    
    /// <summary>
    /// Generates a "cmp.different" instruction
    /// </summary>
    public static Instruction CompareNotSame => new()
    {
        OpCode = OperationType.CompareNotSame
    };

    /// <summary>
    /// Generates a "cmp.null" instruction
    /// </summary>
    /// <param name="t">The type of the parameter</param>
    /// <returns>The generated instruction</returns>
    public static Instruction CompareNull(TypeReference t) => new TypeInstruction
    {
        OpCode = OperationType.CompareNull,
        Value = t
    };
    
    /// <summary>
    /// Generates a "cmp.notnull" instruction
    /// </summary>
    /// <param name="t">The type of the parameter</param>
    /// <returns>The generated instruction</returns>
    public static Instruction CompareNotNull(TypeReference t) => new TypeInstruction
    {
        OpCode = OperationType.CompareNotNull,
        Value = t
    };

    /// <summary>
    /// Generates a "ret" instruction
    /// </summary>
    public static Instruction Return => new()
    {
        OpCode = OperationType.Return
    };
    
    /// <summary>
    /// Generates a "pop" instruction
    /// </summary>
    /// <param name="t">The type to pop</param>
    /// <returns>The generated instruction</returns>
    public static Instruction Pop(TypeReference t) => new TypeInstruction
    {
        OpCode = OperationType.Pop,
        Value = t
    };
    
    /// <summary>
    /// Generates a "dup" instruction
    /// </summary>
    /// <param name="t">The type to duplicate</param>
    /// <returns>The generated instruction</returns>
    public static Instruction Dup(TypeReference t) => new TypeInstruction
    {
        OpCode = OperationType.Dup,
        Value = t
    };
    
    /// <summary>
    /// Generates an "is" instruction
    /// </summary>
    /// <param name="t">The type to check</param>
    /// <returns>The generated instruction</returns>
    public static Instruction Is(TypeReference t) => new TypeInstruction
    {
        OpCode = OperationType.Is,
        Value = t
    };
    
    /// <summary>
    /// Generates an "as" instruction
    /// </summary>
    /// <param name="t">The type to convert to</param>
    /// <returns>The generated instruction</returns>
    public static Instruction As(TypeReference t) => new TypeInstruction
    {
        OpCode = OperationType.As,
        Value = t
    };

    /// <summary>
    /// Generates a "coerce" instruction
    /// </summary>
    /// <param name="t1">The type to coerce from</param>
    /// <param name="t2">The type to coerce to</param>
    /// <returns>The generated instruction</returns>
    public static Instruction Coerce(TypeReference t1, TypeReference t2) => new TypePairInstruction
    {
        OpCode = OperationType.Coerce,
        FirstType = t1,
        SecondType = t2
    };

    /// <summary>
    /// Generates a "switch" instruction
    /// </summary>
    /// <param name="targets">The targets of the switch case</param>
    /// <returns>The generated instruction</returns>
    public static Instruction Switch(params Instruction[] targets) => new SwitchInstruction
    {
        OpCode = OperationType.Switch,
        JumpTargets = targets.ToList(),
    };
    
    /// <summary>
    /// Generates a "throw" instruction
    /// </summary>
    public static Instruction Throw => new()
    {
        OpCode = OperationType.Throw
    };
    
    /// <summary>
    /// Generates a "leave" instruction
    /// </summary>
    /// <param name="target">The target to jump to</param>
    /// <returns>The generated instruction</returns>
    public static Instruction Leave(Instruction target) => new JumpInstruction
    {
        OpCode = OperationType.Leave,
        Target = target
    };
    
    /// <summary>
    /// Generates a "jmp.finally" instruction
    /// </summary>
    /// <param name="target">The target to jump to</param>
    /// <returns>The generated instruction</returns>
    public static Instruction JumpFinally(Instruction target) => new JumpInstruction
    {
        OpCode = OperationType.JumpFinally,
        Target = target
    };
    
    /// <summary>
    /// Generates a "catch.all" instruction
    /// </summary>
    public static Instruction CatchAllStart => new()
    {
        OpCode = OperationType.CatchAllStart
    };
    
    /// <summary>
    /// Generates a "catch.err" instruction
    /// </summary>
    /// <param name="t">The instruction type to catch</param>
    /// <returns>The generated instruction</returns>
    public static Instruction CatchErrStart(TypeReference t) => new TypeInstruction
    {
        OpCode = OperationType.CatchErrStart,
        Value = t
    };
    
    /// <summary>
    /// Generates a "catch.end" instruction
    /// </summary>
    public static Instruction CatchEnd => new()
    {
        OpCode = OperationType.CatchEnd
    };

    /// <summary>
    /// Generates a "finally.start" instruction
    /// </summary>
    public static Instruction FinallyStart => new()
    {
        OpCode = OperationType.FinallyStart
    };

    /// <summary>
    /// Generates a "finally.end" instruction
    /// </summary>
    public static Instruction FinallyEnd => new()
    {
        OpCode = OperationType.FinallyEnd
    };
}
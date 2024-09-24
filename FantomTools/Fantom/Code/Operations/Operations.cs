namespace FantomTools.Fantom.Code.Operations;

public class Operations
{
    public static Dictionary<OperationType, Operation> OperationsByType =
    new(){
        [OperationType.Nop] = new()
        {
            Name = "nop",
            Signature = OperationSignature.None,
            Type = OperationType.Nop
        },
        [OperationType.LoadNull] = new ()
        {
            Name = "ld.null",
            Signature = OperationSignature.None,
            Type = OperationType.LoadNull
        },
        [OperationType.LoadFalse] = new()
        {
            Name = "ld.false",
            Signature = OperationSignature.None,
            Type = OperationType.LoadFalse
        },
        [OperationType.LoadTrue] = new()
        {
            Name = "ld.true",
            Signature = OperationSignature.None,
            Type = OperationType.LoadTrue
        },
        [OperationType.LoadInt] = new ()
        {
            Name = "ld.int",
            Signature = OperationSignature.Integer,
            Type = OperationType.LoadInt
        },
        [OperationType.LoadFloat] = new ()
        {
            Name = "ld.float",
            Signature = OperationSignature.Float,
            Type = OperationType.LoadFloat
        },
        [OperationType.LoadDecimal] = new()
        {
            Name = "ld.decimal",
            Signature = OperationSignature.Decimal,
            Type = OperationType.LoadDecimal
        },
        [OperationType.LoadStr] = new()
        {
            Name = "ld.str",
            Signature = OperationSignature.String,
            Type = OperationType.LoadStr
        },
        [OperationType.LoadDuration] = new() {
            Name = "ld.duration",
            Signature = OperationSignature.Duration,
            Type = OperationType.LoadDuration
        },
        [OperationType.LoadType] = new ()
        {
            Name = "ld.type",
            Signature = OperationSignature.Type,
            Type = OperationType.LoadType
        },
        [OperationType.LoadUri] = new ()
        {
            Name = "ld.uri",
            Signature = OperationSignature.Uri,
            Type = OperationType.LoadUri
        },
        [OperationType.LoadVar] = new ()
        {
            Name = "ld.var",
            Signature = OperationSignature.Register,
            Type = OperationType.LoadVar
        },
        [OperationType.StoreVar] = new ()
        {
            Name = "st.var",
            Signature = OperationSignature.Register,
            Type = OperationType.StoreVar
        },
        [OperationType.LoadInstance] = new ()
        {
            Name = "ld.instance",
            Signature = OperationSignature.Field,
            Type = OperationType.LoadInstance
        },
        [OperationType.StoreInstance] = new ()
        {
            Name = "st.instance",
            Signature = OperationSignature.Field,
            Type = OperationType.StoreInstance
        },
        [OperationType.LoadStatic] = new ()
        {
            Name = "ld.static",
            Signature = OperationSignature.Field,
            Type = OperationType.LoadStatic
        },
        [OperationType.StoreStatic] = new()
        {
            Name = "st.static",
            Signature = OperationSignature.Field,
            Type = OperationType.StoreStatic
        },
        [OperationType.LoadMixinStatic] = new ()
        {
            Name = "ld.mixin",
            Signature = OperationSignature.Field,
            Type = OperationType.LoadMixinStatic
        },
        [OperationType.StoreMixinStatic] = new()
        {
            Name = "st.mixin",
            Signature = OperationSignature.Field,
            Type = OperationType.StoreMixinStatic
        },
        [OperationType.CallNew] = new()
        {
            Name = "new",
            Signature = OperationSignature.Method,
            Type = OperationType.CallNew
        },
        [OperationType.CallCtor] = new()
        {
            Name = "ctor",
            Signature = OperationSignature.Method,
            Type = OperationType.CallCtor
        },
        [OperationType.CallStatic] = new()
        {
            Name = "call.static",
            Signature = OperationSignature.Method,
            Type = OperationType.CallStatic
        },
        [OperationType.CallVirtual] = new()
        {
            Name = "call.virtual",
            Signature = OperationSignature.Method,
            Type = OperationType.CallVirtual
        },
        [OperationType.CallNonVirtual] = new()
        {
            Name = "call",
            Signature = OperationSignature.Method,
            Type = OperationType.CallNonVirtual
        },
        [OperationType.CallMixinStatic] = new()
        {
            Name = "call.mixin.static",
            Signature = OperationSignature.Method,
            Type = OperationType.CallMixinStatic
        },
        [OperationType.CallMixinVirtual] = new()
        {
            Name = "call.mixin.virtual",
            Signature = OperationSignature.Method,
            Type = OperationType.CallMixinVirtual
        },
        [OperationType.CallMixinNonVirtual] = new()
        {
            Name = "call.mixin",
            Signature = OperationSignature.Method,
            Type = OperationType.CallMixinNonVirtual
        },
        [OperationType.Jump] = new()
        {
            Name = "jmp",
            Signature = OperationSignature.Jump,
            Type = OperationType.Jump
        },
        [OperationType.JumpTrue] = new()
        {
            Name = "jmp.true",
            Signature = OperationSignature.Jump,
            Type = OperationType.JumpTrue
        },
        [OperationType.JumpFalse] = new()
        {
            Name = "jmp.false",
            Signature = OperationSignature.Jump,
            Type = OperationType.JumpFalse
        },
        [OperationType.CompareEq] = new()
        {
            Name = "cmp.eq",
            Signature = OperationSignature.TypePair,
            Type = OperationType.CompareEq
        },
        [OperationType.CompareNe] = new()
        {
            Name = "cmp.ne",
            Signature = OperationSignature.TypePair,
            Type = OperationType.CompareNe
        },
        [OperationType.Compare] = new()
        {
            Name = "cmp",
            Signature = OperationSignature.TypePair,
            Type = OperationType.Compare
        },
        [OperationType.CompareLe] = new()
        {
            Name = "cmp.le",
            Signature = OperationSignature.TypePair,
            Type = OperationType.CompareLe
        },
        [OperationType.CompareLt] = new()
        {
            Name = "cmp.lt",
            Signature = OperationSignature.TypePair,
            Type = OperationType.CompareLt
        },
        [OperationType.CompareGe] = new()
        {
            Name = "cmp.ge",
            Signature = OperationSignature.TypePair,
            Type = OperationType.CompareGe
        },
        [OperationType.CompareGt] = new()
        {
            Name = "cmp.gt",
            Signature = OperationSignature.TypePair,
            Type = OperationType.CompareGt
        },
        [OperationType.CompareSame] = new()
        {
            Name = "cmp.same",
            Signature = OperationSignature.None,
            Type = OperationType.CompareSame
        },
        [OperationType.CompareNotSame] = new()
        {
            Name = "cmp.different",
            Signature = OperationSignature.None,
            Type = OperationType.CompareNotSame
        },
        [OperationType.CompareNull] = new()
        {
            Name = "cmp.null",
            Signature = OperationSignature.Type,
            Type = OperationType.CompareNull
        },
        [OperationType.CompareNotNull] = new()
        {
            Name = "cmp.notnull",
            Signature = OperationSignature.Type,
            Type = OperationType.CompareNotNull
        },
        [OperationType.Return] = new()
        {
            Name = "ret",
            Signature = OperationSignature.None,
            Type = OperationType.Return
        },
        [OperationType.Pop] = new()
        {
            Name = "pop",
            Signature = OperationSignature.Type,
            Type = OperationType.Pop
        },
        [OperationType.Dup] = new()
        {
            Name = "dup",
            Signature =  OperationSignature.Type,
            Type = OperationType.Dup
        },
        [OperationType.Is] = new()
        {
            Name = "is",
            Signature = OperationSignature.Type,
            Type = OperationType.Is
        },
        [OperationType.As] = new()
        {
            Name = "as",
            Signature = OperationSignature.Type,
            Type = OperationType.As
        },
        [OperationType.Coerce] = new()
        {
            Name = "coerce",
            Signature = OperationSignature.TypePair,
            Type = OperationType.Coerce
        },
        [OperationType.Switch] = new()
        {
            Name = "switch",
            Signature = OperationSignature.None,
            Type = OperationType.Switch
        },
        [OperationType.Throw] = new()
        {
            Name = "throw",
            Signature = OperationSignature.None,
            Type = OperationType.Throw
        },
        [OperationType.Leave] = new()
        {
            Name = "leave",
            Signature = OperationSignature.Jump,
            Type = OperationType.Leave
        },
        [OperationType.JumpFinally] = new()
        {
            Name = "jmp.finally",
            Signature = OperationSignature.Jump,
            Type = OperationType.JumpFinally
        },
        [OperationType.CatchAllStart] = new()
        {
            Name = "catch.all",
            Signature = OperationSignature.None,
            Type = OperationType.CatchAllStart
        },
        [OperationType.CatchErrStart] = new()
        {
            Name = "catch.err",
            Signature = OperationSignature.Type,
            Type = OperationType.CatchErrStart
        },
        [OperationType.CatchEnd] = new()
        {
            Name = "catch.end",
            Signature = OperationSignature.None,
            Type = OperationType.CatchEnd,
        },
        [OperationType.FinallyStart] = new()
        {
            Name = "finally.start",
            Signature = OperationSignature.None,
            Type = OperationType.FinallyStart
        },
        [OperationType.FinallyEnd] = new()
        {
            Name = "finally.end",
            Signature = OperationSignature.None,
            Type = OperationType.FinallyEnd
        }
    };
}
namespace FantomTools.Fantom.Code.Operations;

/// <summary>
/// This represents the type of operation
/// </summary>
public enum OperationType
{
    /// <summary>
    /// Does nothing
    /// </summary>
    Nop = 0,
    /// <summary>
    /// Pushes a null reference to the stack
    /// </summary>
    LoadNull = 1,
    /// <summary>
    /// Pushes a boolean with the value false to the stack
    /// </summary>
    LoadFalse = 2,
    /// <summary>
    /// Pushes a boolean with the value true to the stack
    /// </summary>
    LoadTrue = 3,
    /// <summary>
    /// Pushes an integer to the stack
    /// </summary>
    LoadInt = 4,
    /// <summary>
    /// Pushes a floating point number to the stack
    /// </summary>
    LoadFloat = 5,
    /// <summary>
    /// Pushes a decimal value onto the stack
    /// </summary>
    LoadDecimal = 6,
    /// <summary>
    /// Pushes a string onto the stack
    /// </summary>
    LoadStr = 7,
    /// <summary>
    /// Pushes a duration value onto the stack
    /// </summary>
    LoadDuration = 8,
    /// <summary>
    /// Pushes a type reference onto the stack
    /// </summary>
    LoadType = 9,
    /// <summary>
    /// Pushes a uri onto the stack
    /// </summary>
    LoadUri = 10,
    /// <summary>
    /// Pushes the value of a method variable (this/parameter/local) onto the stack
    /// </summary>
    LoadVar = 11,
    /// <summary>
    /// Pops a value off the stack and stores it into a method variable
    /// </summary>
    StoreVar = 12,
    /// <summary>
    /// Pops a reference to an instance of a type off the stack, and pushes the value of the given field for that instance
    /// </summary>
    LoadInstance = 13,
    /// <summary>
    /// Pops a value, and a reference to an instance of a type off the stack, and stores the value into the given field for that instance
    /// </summary>
    StoreInstance = 14,
    /// <summary>
    /// Pushes the value of a static field onto the stack
    /// </summary>
    LoadStatic = 15,
    /// <summary>
    /// Pops a value off the stack and stores it into a static field
    /// </summary>
    StoreStatic = 16,
    /// <summary>
    /// Pushes the value of a static on mixin field onto the stack
    /// </summary>
    LoadMixinStatic = 17,
    /// <summary>
    /// Pops a value off the stack and stores it into a static on mixin field
    /// </summary>
    StoreMixinStatic = 18,
    /// <summary>
    /// Allocates a new instance of a type, and calls the given constructor with the parameters being popped off the stack, then pushes the allocated instance
    /// </summary>
    CallNew = 19,
    /// <summary>
    /// Calls a constructor for a type popping the instance and parameters from the stack. Does not push instance back on
    /// </summary>
    CallCtor = 20,
    /// <summary>
    /// Calls a static method popping the parameters off the stack and pushing the return value
    /// </summary>
    CallStatic = 21,
    /// <summary>
    /// Calls a virtual (overridable) method, popping the parameters off the stack and pushing the return value
    /// </summary>
    CallVirtual = 22,
    /// <summary>
    /// Calls a (non-overridable) method, popping the parameters off the stack and pushing the return value
    /// </summary>
    CallNonVirtual = 23,
    /// <summary>
    /// Calls a static mixin method popping the parameters off the stack and pushing the return value
    /// </summary>
    CallMixinStatic = 24,
    /// <summary>
    /// Calls a virtual (overridable) mixin method popping the parameters off the stack and pushing the return value
    /// </summary>
    CallMixinVirtual = 25,
    /// <summary>
    /// Calls a (non-overridable) mixin method popping the parameters off the stack and pushing the return value
    /// </summary>
    CallMixinNonVirtual = 26,
    /// <summary>
    /// Jumps to target instruction
    /// </summary>
    Jump = 27,
    /// <summary>
    /// Pops the top value off the stack, if it is a boolean true, jump to the target, otherwise continue
    /// </summary>
    JumpTrue = 28,
    /// <summary>
    /// Pops the top value off the stack, if it is a boolean false, jump to the target, otherwise continue
    /// </summary>
    JumpFalse = 29,
    /// <summary>
    /// Pops the top 2 values off the stack, and checks if they are equal, pushing the boolean result of that comparison to the stack
    /// </summary>
    CompareEq = 30,
    /// <summary>
    /// Pops the top 2 values off the stack, and checks if they are not equal, pushing the boolean result of that comparison to the stack
    /// </summary>
    CompareNe = 31,
    /// <summary>
    /// Pops the top 2 values off the stack, and compares them as if using the Compare method from C#, pushing the integer result to the stack
    /// </summary>
    Compare = 32,
    /// <summary>
    /// Pops the top 2 values off the stack, and checks if the one that was pushed first is lesser than or equal to the one pushed second, pushing the boolean result of that comparison to the stack
    /// </summary>
    CompareLe = 33,
    /// <summary>
    /// Pops the top 2 values off the stack, and checks if the one that was pushed first is lesser than the one pushed second, pushing the boolean result of that comparison to the stack
    /// </summary>
    CompareLt = 34,
    /// <summary>
    /// Pops the top 2 values off the stack, and checks if the one that was pushed first is greater than the one pushed second, pushing the boolean result of that comparison to the stack
    /// </summary>
    CompareGt = 35,
    /// <summary>
    /// Pops the top 2 values off the stack, and checks if the one that was pushed first is greater than or equal to the one pushed second, pushing the boolean result of that comparison to the stack
    /// </summary>
    CompareGe = 36,
    /// <summary>
    /// Pops the top 2 values off the stack and checks if they are the same (as with ===) pushes the boolean result onto the stack
    /// </summary>
    CompareSame = 37,
    /// <summary>
    /// Pops the top 2 values off the stack and checks if they are different (as with !==) pushes the boolean result onto the stack
    /// </summary>
    CompareNotSame = 38,
    /// <summary>
    /// Pops the top value off the stack and checks if it's null pushing the boolean result onto the stack, takes a type parameter
    /// </summary>
    CompareNull = 39,
    /// <summary>
    /// Pops the top value off the stack and checks if it's not null pushing the boolean result onto the stack, takes a type parameter
    /// </summary>
    CompareNotNull = 40,
    /// <summary>
    /// Pops the top value if the method is non-void and returns that as the return value, exits the method
    /// </summary>
    Return = 41,
    /// <summary>
    /// Pops the top value off the stack, takes a type parameter
    /// </summary>
    Pop = 42,
    /// <summary>
    /// Duplicates the top value of the stack, takes a type parameter
    /// </summary>
    Dup = 43,
    /// <summary>
    /// Checks if the top value of the stack is an instance of the given type
    /// </summary>
    Is = 44,
    /// <summary>
    /// Converts the top value of the stack into an instance of the given type
    /// </summary>
    As = 45,
    /// <summary>
    /// Coerces the top value of the stack, which has to be of the first given type, into the second given type
    /// </summary>
    Coerce = 46,
    /// <summary>
    /// Pops the top value off the stack, then selects a target to go to from indexing the array of targets with the value, continues to the next instruction if a value is not found
    /// </summary>
    Switch = 47,
    /// <summary>
    /// Throw the top value of the stack as an exception
    /// </summary>
    Throw = 48,
    /// <summary>
    /// Leave a protected region to a target
    /// </summary>
    Leave = 49,
    /// <summary>
    /// Leave a protected region to a finally block
    /// </summary>
    JumpFinally = 50,
    /// <summary>
    /// Start a catch block without the error being pushed onto the stack
    /// </summary>
    CatchAllStart = 51,
    /// <summary>
    /// Start a catch block with the error being pushed onto the stack, takes a type that is the error type
    /// </summary>
    CatchErrStart = 52,
    /// <summary>
    /// End a catch block
    /// </summary>
    CatchEnd = 53,
    /// <summary>
    /// Start a finally block
    /// </summary>
    FinallyStart = 54,
    /// <summary>
    /// End a finally block
    /// </summary>
    FinallyEnd = 55
}
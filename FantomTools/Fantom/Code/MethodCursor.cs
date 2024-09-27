using FantomTools.Fantom.Code.AssemblyTools;
using FantomTools.Fantom.Code.Instructions;
using JetBrains.Annotations;

namespace FantomTools.Fantom.Code;

/// <summary>
/// Represents a movable cursor over a method body for easier modification
/// </summary>
/// <param name="body">The body that you want to edit with this cursor</param>
[PublicAPI]
public class MethodCursor(MethodBody body)
{
    /// <summary>
    /// The current index of the cursor
    /// </summary>
    public ushort Index;
    /// <summary>
    /// The current instruction of the cursor
    /// </summary>
    public Instruction? Current => Index < body.Instructions.Count ? body.Instructions[Index] : null;
    /// <summary>
    /// The method body this cursor is attached to
    /// </summary>
    public MethodBody Body => body;

    
    /// <summary>
    /// Have the method cursor seek for an instruction that matches a given predicate
    /// </summary>
    /// <param name="predicate">The predicate to use to check each instruction</param>
    /// <param name="mode">Before if the pointer should point to the index of the resultant instruction, After if it should point to the index after</param>
    /// <param name="direction">The direction to seek in</param>
    /// <exception cref="Exception">Thrown if there is no instruction found that matches the predicate</exception>
    public void Seek(Func<Instruction, bool> predicate, SeekMode mode = SeekMode.Before, SeekDirection direction = SeekDirection.Forwards)
    {
        while (Current is not null && !predicate(Current))
        {
            if (direction == SeekDirection.Backwards)
            {
                if (Index == 0) throw new Exception("Seek reached end without stopping");
                Index--;
            }
            else
            {
                if (Index + 1 == body.Instructions.Count) throw new Exception("Seek reached end without stopping");
                Index++;
            }
        }

        if (mode == SeekMode.After) Index++;
    }

    /// <summary>
    /// Replace the current instruction with a new instruction, updating all jump targets
    /// </summary>
    /// <param name="newInstruction">The instruction to replace the current with</param>
    public void Replace(Instruction newInstruction)
    {
        foreach (var instruction in body.Instructions.Where(instruction => instruction != Current))
        {
            switch (instruction)
            {
                case JumpInstruction jumpInstruction when jumpInstruction.Target == Current:
                    jumpInstruction.Target = newInstruction;
                    break;
                case SwitchInstruction switchInstruction:
                {
                    for (var i = 0; i < switchInstruction.JumpTargets.Count; i++)
                    {
                        if (switchInstruction.JumpTargets[i] == Current)
                        {
                            switchInstruction.JumpTargets[i] = newInstruction;
                        }
                    }

                    break;
                }
            }
        }
        
        foreach (var tryCatch in Body.ErrorTable.TryBlocks)
        {
            if (tryCatch.Start == Current) tryCatch.Start = newInstruction;
            if (tryCatch.End == Current) tryCatch.End = newInstruction;
            if (tryCatch.Finally == Current) tryCatch.End = newInstruction;
            foreach (var (ty, handler) in tryCatch.ErrorHandlers)
            {
                if (handler == Current) tryCatch.ErrorHandlers[ty] = newInstruction;
            }
        }
        body.Instructions[Index] = newInstruction;
    }

    /// <summary>
    /// Remove the current instruction
    /// </summary>
    /// <param name="jumpRemovalRetargetMode">Choose what to do if an instruction jumps to this instruction, should it error, or should it be retargeted to the next instruction</param>
    /// <exception cref="Exception">Thrown if an instruction jumps to the current instruction, and jumpRemovalRetargetMode = Error</exception>
    public void Remove(RemovalRetargetMode jumpRemovalRetargetMode = RemovalRetargetMode.Error)
    {
        foreach (var instruction in body.Instructions.Where(instruction => instruction != Current))
        {
            switch (instruction)
            {
                case JumpInstruction jumpInstruction when jumpInstruction.Target == Current:
                {
                    if (jumpRemovalRetargetMode == RemovalRetargetMode.Error)
                        throw new Exception("Removing instruction will retarget jump!");
                    jumpInstruction.Target = body.Instructions[Index+1];
                    break;
                }
                case SwitchInstruction switchInstruction:
                {
                    for (var i = 0; i < switchInstruction.JumpTargets.Count; i++)
                    {
                        if (switchInstruction.JumpTargets[i] != Current) continue;
                        if (jumpRemovalRetargetMode == RemovalRetargetMode.Error)
                            throw new Exception("Removing instruction will retarget jump!");
                        switchInstruction.JumpTargets[i] = body.Instructions[Index + 1];
                    }
                    break;
                }
            }
        }
        foreach (var tryCatch in Body.ErrorTable.TryBlocks)
        {
            if (tryCatch.Start == Current)
            {
                if (jumpRemovalRetargetMode == RemovalRetargetMode.Error)
                    throw new Exception("Removing instruction will retarget jump!");
                tryCatch.Start = body.Instructions[Index + 1];
            }
            if (tryCatch.End == Current)
            {
                if (jumpRemovalRetargetMode == RemovalRetargetMode.Error)
                    throw new Exception("Removing instruction will retarget jump!");
                tryCatch.End = body.Instructions[Index + 1];
            }
            if (tryCatch.Finally == Current)
            {
                if (jumpRemovalRetargetMode == RemovalRetargetMode.Error)
                    throw new Exception("Removing instruction will retarget jump!");
                tryCatch.Finally = body.Instructions[Index + 1];
            }
            foreach (var (ty, handler) in tryCatch.ErrorHandlers)
            {
                if (handler != Current) continue;
                if (jumpRemovalRetargetMode == RemovalRetargetMode.Error)
                    throw new Exception("Removing instruction will retarget jump!");
                tryCatch.ErrorHandlers[ty] = body.Instructions[Index+1];
            }
        }
        body.Instructions.RemoveAt(Index);
    }

    /// <summary>
    /// Insert an instruction at the current position
    /// </summary>
    /// <param name="instruction">The instruction to insert</param>
    /// <param name="advanceCursor">Should the cursor advance to point after the instruction just inserted?</param>
    /// <param name="retargetMode">How should jumps to the current instruction be treated</param>
    public void Insert(Instruction instruction, bool advanceCursor = true, InsertionRetargetMode retargetMode=InsertionRetargetMode.KeepCurrentTarget)
    {
        if (retargetMode == InsertionRetargetMode.ReplaceWithInserted)
        {
            foreach (var inst in body.Instructions)
            {
                switch (inst)
                {
                    case JumpInstruction jumpInstruction when jumpInstruction.Target == Current:
                        jumpInstruction.Target = instruction;
                        break;
                    case SwitchInstruction switchInstruction:
                    {
                        for (var i = 0; i < switchInstruction.JumpTargets.Count; i++)
                        {
                            if (switchInstruction.JumpTargets[i] == Current)
                            {
                                switchInstruction.JumpTargets[i] = instruction;
                            }
                        }

                        break;
                    }
                }
            }
            foreach (var tryCatch in Body.ErrorTable.TryBlocks)
            {
                if (tryCatch.Start == Current) tryCatch.Start = instruction;
                if (tryCatch.End == Current) tryCatch.End = instruction;
                if (tryCatch.Finally == Current) tryCatch.End = instruction;
                foreach (var (ty, handler) in tryCatch.ErrorHandlers)
                {
                    if (handler == Current) tryCatch.ErrorHandlers[ty] = instruction;
                }
            }
        }
        body.Instructions.Insert(Index, instruction);
        if (advanceCursor)
        {
            Index++;
        }
    }

    /// <summary>
    /// Insert a range of instructions at the current position, advancing the cursor
    /// </summary>
    /// <param name="instructions">The instructions to add</param>
    /// <param name="retargetMode">How should jumps to the current instruction be treated</param>
    public void Insert(IEnumerable<Instruction> instructions, InsertionRetargetMode retargetMode=InsertionRetargetMode.KeepCurrentTarget)
    {
        var first = true;
        foreach (var instruction in instructions)
        {
            Insert(instruction, true, first ? retargetMode : InsertionRetargetMode.KeepCurrentTarget);
            first = false;
        }
    }

    /// <summary>
    /// Insert assembly at the current position, advancing the cursor
    /// </summary>
    /// <param name="assembly">The assembly to add</param>
    /// <param name="retargetMode">How should jumps to the current instruction be treated</param>
    public void InsertAssembly(string assembly, InsertionRetargetMode retargetMode=InsertionRetargetMode.KeepCurrentTarget)
    {
        var (insts, locals, tries) = new MethodAssembler(Body.Method, true).Assemble(assembly, Current);
        foreach (var local in locals)
        {
            local.Index = (ushort)body.Method.Variables.Count;
            ((List<MethodVariable>)body.Method.Variables).Add(local);
        }

        body.ErrorTable.TryBlocks.AddRange(tries);
        Insert(insts, retargetMode);
    }
    
    /// <summary>
    /// Advance the cursor
    /// </summary>
    /// <param name="n">The amount of instructions to advance</param>
    public void Advance(ushort n=1)
    {
        if (Index + n <= body.Instructions.Count) Index += n;
        else Index = (ushort)body.Instructions.Count;
    }
    
    /// <summary>
    /// Represents the behaviour of seeking after an instruction is found
    /// </summary>
    public enum SeekMode
    {
        /// <summary>
        /// Point to the found instruction
        /// </summary>
        Before,
        /// <summary>
        /// Point to the instruction after the found instruction
        /// </summary>
        After
    }

    /// <summary>
    /// Represents the behaviour of what to do when a jump needs to be retargeted during removal
    /// </summary>
    [PublicAPI]
    public enum RemovalRetargetMode
    {
        /// <summary>
        /// Throw an error on retarget
        /// </summary>
        Error,
        /// <summary>
        /// Retarget the jump to the next instruction
        /// </summary>
        After
    }

    /// <summary>
    /// Represents the behaviour of what to do when a jump needs to be retargeted during insertion
    /// </summary>
    [PublicAPI]
    public enum InsertionRetargetMode
    {
        KeepCurrentTarget,
        ReplaceWithInserted
    }
    
    /// <summary>
    /// Represents the direction a seek operation should go
    /// </summary>
    public enum SeekDirection
    {
        /// <summary>
        /// Go forwards through the instructions
        /// </summary>
        Forwards,
        /// <summary>
        /// Go backwards through the instruction
        /// </summary>
        Backwards
    }
}
using FantomTools.Fantom.Code.Instructions;
using JetBrains.Annotations;

namespace FantomTools.Fantom.Code;

[PublicAPI]
public class MethodCursor(MethodBody body)
{
    public ushort Index;

    
    // Used for finding an instruction
    public void Seek(Func<Instruction, bool> predicate, SeekMode mode = SeekMode.Before, SeekDirection direction = SeekDirection.Forwards)
    {
        while (!predicate(Current))
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

    public Instruction Current => body.Instructions[Index];

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

        body.Instructions[Index] = newInstruction;
    }

    public void Remove(RetargetMode jumpRetargetMode = RetargetMode.Error)
    {
        foreach (var instruction in body.Instructions.Where(instruction => instruction != Current))
        {
            switch (instruction)
            {
                case JumpInstruction jumpInstruction when jumpInstruction.Target == Current:
                {
                    if (jumpRetargetMode == RetargetMode.Error)
                        throw new Exception("Removing instruction will retarget jump!");
                    jumpInstruction.Target = body.Instructions[Index+1];
                    break;
                }
                case SwitchInstruction switchInstruction:
                {
                    for (var i = 0; i < switchInstruction.JumpTargets.Count; i++)
                    {
                        if (switchInstruction.JumpTargets[i] != Current) continue;
                        if (jumpRetargetMode == RetargetMode.Error)
                            throw new Exception("Removing instruction will retarget jump!");
                        switchInstruction.JumpTargets[i] = body.Instructions[Index + 1];
                    }

                    break;
                }
            }
        }
        body.Instructions.RemoveAt(Index);
    }

    public void Insert(Instruction instruction, bool advanceCursor = true)
    {
        body.Instructions.Insert(Index, instruction);
        if (advanceCursor)
        {
            Index++;
        }
    }

    public void Insert(IEnumerable<Instruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            Insert(instruction);
        }
    }
    
    public enum SeekMode
    {
        Before,
        After
    }

    public enum RetargetMode
    {
        Error,
        After
    }
    
    public enum SeekDirection
    {
        Forwards,
        Backwards
    }
}
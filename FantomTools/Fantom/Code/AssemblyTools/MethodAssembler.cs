using System.Text;
using System.Text.Json.Serialization;
using FantomTools.Fantom.Code.ErrorHandling;
using FantomTools.Fantom.Code.Instructions;
using FantomTools.Fantom.Code.Operations;
using FantomTools.InternalUtilities;
using FantomTools.Utilities;

namespace FantomTools.Fantom.Code.AssemblyTools;


internal class MethodAssembler
{
    private readonly Func<string, MethodVariable?> _variableResolver;
    
    public MethodAssembler(Method method, bool allowLocals=false)
    {
        if (allowLocals) _variableResolver = s => method.Variables.FirstOrDefault(x => x.Name == s);
        else _variableResolver = s => method.Parameters.FirstOrDefault(x => x.Name == s);
    }
    
    private Dictionary<string, List<Action<Instruction>>> _pendingJumps = [];
    private Dictionary<string, Instruction> _labels = [];
    private List<Instruction> _instructions = [];
    private List<string> _lines = [];
    private int _index = 0;
    private string Line => _lines[_index];
    private List<TryBlock> _allTryBlocks = [];
    private TryBlock? _lastResolvedTryBlock = null;
    private List<Action<Instruction>> _onNextAppend = [];

    private enum BlockType
    {
        Try,
        Catch,
        Finally,
    }

    private Stack<(BlockType bt, TryBlock attachedBlock)> _currentBlocks = [];

    private List<MethodVariable> _newLocals = [];


    public (List<Instruction> instructions, List<MethodVariable> newLocals, List<TryBlock> newTryBlocks, Dictionary<Instruction, string> labelsOverride) Assemble(
        string body, Instruction? finish = null) // 
    {
        SetupAssembly(body, finish);
        AssembleAll(finish);
        var reversedLabels = new Dictionary<Instruction, string>();
        foreach (var (k, v) in _labels)
        {
            reversedLabels[v] = k;
        }
        reversedLabels.TryAdd(_instructions[0], "start");
        return (_instructions, _newLocals, _allTryBlocks.ToList(), reversedLabels);
    }

    
    /// <summary>
    /// Slightly janky at the moment
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public (List<Instruction> instructions, List<MethodVariable> newLocals, List<TryBlock> newTryBlocks,
        Dictionary<Instruction, string> labelsOverride) AssembleMethod(string body)
    {
        // Now we need to trim off the first part of the body and copy local declarations into the second part
        var lines = body.Split('\n', StringSplitOptions.TrimEntries).ToList();
        // Remove the first 2 lines as they contain the method header, and maxstack declaration
        // Remove the last line as it is the last brace of the method
        lines = lines[1..^1];
        var index = lines.FindIndex(x => x.StartsWith(".maxstack"));
        if (index >= 0) lines.RemoveAt(index);
        // Remove the closing bracket of the locals
        var closingBracketIndex = lines.IndexOf("]");
        lines.RemoveAt(closingBracketIndex);
        // Remove the opening brace of the locals
        var openingBraceIndex = lines.IndexOf("{");
        lines.RemoveAt(openingBraceIndex);
        SetupAssembly(string.Join('\n',lines),null);
        AssembleAll();
        var reversedLabels = new Dictionary<Instruction, string>();
        foreach (var (k, v) in _labels)
        {
            reversedLabels[v] = k;
        }
        reversedLabels.TryAdd(_instructions[0], "start");
        return (_instructions, _newLocals, _allTryBlocks.ToList(), reversedLabels);
    }

    private void SetupAssembly(string body, Instruction? finish)
    {
        _lines = Preprocess(body).ToList();
        _pendingJumps = [];
        _labels = [];
        _allTryBlocks = [];
        _lastResolvedTryBlock = null;
        _currentBlocks = [];
        _newLocals = [];
        _onNextAppend = [];
        
        if (finish != null) _labels["$FIN"] = finish;
    }

    private void AssembleAll(Instruction? finish = null)
    {
        while (_index < _lines.Count)
        {
            ProcessNext();
        }

        if (_onNextAppend.Count != 0)
        {
            if (finish is not null)
            {
                foreach (var action in _onNextAppend)
                {
                    action(finish);
                }
            }
            else
            {
                throw new Exception(
                    "Information requiring next instruction found without being allowed next instruction, usually this is a terminating label!");
            }
        }

        if (_currentBlocks.Count > 0)
        {
            throw new Exception("Unfinished blocks found!");
        }

        if (_pendingJumps.Count > 0)
        {
            throw new Exception($"Unresolved labels: {string.Join(", ", _pendingJumps.Keys)}");
        }
    }

    // This will be called until we have no more lines left
    private void ProcessNext()
    {
        // First we get all labels that may be affecting this line
        while (_index < _lines.Count && BeginsWithLabel(Line))
        {
            var split = Line.Split(':', 2);
            var label = split[0];
            _onNextAppend.Add(x => ResolveLabel(label, x));
            var rest = split.Length == 1 ? "" : split[1].Trim();
            if (rest != "")
            {
                _lines[_index] = rest;
            }
            else
            {
                _index += 1;
            }
        }

        if (_index >= _lines.Count) return;

        var lineSplit = Line.Split(' ', 2, StringSplitOptions.TrimEntries);
        var op = lineSplit[0];
        var parameters = lineSplit.Length == 2 ? lineSplit[1] : null;

        switch (op)
        {
            case ".local":
                ProcessLocalDeclaration(parameters);
                break;
            case "switch":
                ProcessSwitch(parameters);
                break;
            case "try":
                if (parameters != "{") throw new Exception("expected { after try");
                // This is easy enough to do, we just need to 
                _onNextAppend.Add(x =>
                {
                    var tb = new TryBlock()
                    {
                        Start = x,
                        ErrorHandlers = []
                    };
                    _allTryBlocks.Add(tb);
                    _currentBlocks.Push((BlockType.Try,tb));
                });
                break;
            case "catch":
                ProcessCatch(parameters);
                break;
            case "finally":
                if (_lastResolvedTryBlock == null)
                    throw new Exception("Attempted to start finally block without try block!");
                if (parameters != "{") throw new Exception("expected { after finally");
                var finallyInst = new Instruction { OpCode = OperationType.FinallyStart };
                _lastResolvedTryBlock.Finally = finallyInst;
                _currentBlocks.Push((BlockType.Finally, _lastResolvedTryBlock));
                AddInstruction(finallyInst);
                break;
            case "}":
                var lastBlock = _currentBlocks.Pop();
                _lastResolvedTryBlock = lastBlock.attachedBlock;
                switch (lastBlock.bt)
                {
                    case BlockType.Try:
                        lastBlock.attachedBlock.End = _instructions.Last();
                        break;
                    case BlockType.Catch:
                        AddInstruction(new Instruction {OpCode = OperationType.CatchEnd});
                        break;
                    case BlockType.Finally:
                        AddInstruction(new Instruction {OpCode = OperationType.FinallyEnd});
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                break;
            default:
                try
                {
                    var resultOp = Operations.Operations.OperationsByType.Values.First(x => x.Name == op);
                    switch (resultOp.Signature)
                    {
                        case OperationSignature.None:
                            AddInstruction(new Instruction {OpCode = resultOp.Type});
                            break;
                        case OperationSignature.Integer:
                            if (parameters == null)
                                throw new Exception($"Expected integer value after integer instruction: {op}");
                            AddInstruction(new IntegerInstruction
                                { OpCode = resultOp.Type, Value = long.Parse(parameters.Replace("_","")) });
                            break;
                        case OperationSignature.Float:
                            if (parameters == null)
                                throw new Exception($"Expected float value after float instruction: {op}");
                            AddInstruction(new FloatInstruction
                                { OpCode = resultOp.Type, Value = double.Parse(parameters.Replace("_","")) });
                            break;
                        case OperationSignature.Decimal:
                            if (parameters == null)
                                throw new Exception($"Expected decimal value after decimal instruction: {op}");
                            AddInstruction(new StringInstruction
                                { OpCode = resultOp.Type, Value = parameters.Trim() });
                            break;
                        case OperationSignature.Uri:
                        case OperationSignature.String:
                            if (parameters == null)
                                throw new Exception($"Expected decimal value after decimal instruction: {op}");
                            AddInstruction(new StringInstruction { OpCode = resultOp.Type, Value = parameters[1..^1].Unescape()});
                            break;
                        case OperationSignature.Duration:
                            if (parameters == null)
                                throw new Exception($"Expected duration value after duration instruction: {op}");
                            AddInstruction(new IntegerInstruction
                                { OpCode = resultOp.Type, Value = parameters.ToDurationValue() });
                            break;
                        case OperationSignature.Type:
                            if (parameters == null)
                                throw new Exception($"Expected type value after type instruction: {op}");
                            AddInstruction(new TypeInstruction { OpCode = resultOp.Type, Value = parameters});
                            break;
                        case OperationSignature.Register:
                            if (parameters == null)
                                throw new Exception($"Expected variable name after register instruction: {op}");
                            AddInstruction(new RegisterInstruction
                            {
                                OpCode = resultOp.Type,
                                Value = parameters == "this"
                                    ? null
                                    : ResolveVariableName(parameters) ??
                                      throw new Exception($"Unknown variable {parameters}")
                            });
                            break;
                        case OperationSignature.Field:
                            if (parameters == null)
                                throw new Exception($"Expected field after field instruction: {op}");
                            AddInstruction(new FieldInstruction
                                { OpCode = resultOp.Type, Value = ParseField(parameters) });
                            break;
                        case OperationSignature.Method:
                            if (parameters == null)
                                throw new Exception($"Expected field after field instruction: {op}");
                            AddInstruction(new MethodInstruction
                                { OpCode = resultOp.Type, Value = ParseMethod(parameters) });
                            break;
                        case OperationSignature.Jump:
                            if (parameters == null)
                                throw new Exception($"Expected label after jump instruction: {op}");
                            var ji = new JumpInstruction { OpCode = resultOp.Type };
                            PendLabel(parameters, x => ji.Target = x);
                            AddInstruction(ji);
                            break;
                        case OperationSignature.TypePair:
                            if (parameters == null)
                                throw new Exception($"Expected type pair after type pair instruction: {op}");
                            var pair = parameters.Split(';', 2, StringSplitOptions.TrimEntries);
                            if (pair.Length != 2)
                                throw new Exception(
                                    $"Invalid type pair: {parameters}, expected 2 types separated by a semicolon");
                            AddInstruction(new TypePairInstruction
                                { OpCode = resultOp.Type, FirstType = pair[0], SecondType = pair[1] });
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (InvalidOperationException)
                {
                    throw new Exception($"Unexpected operation {op} on line: {Line}");
                }
                break;
                
        }
        _index += 1;
    }

    private void ProcessCatch(string? parameters)
    {
        
        if (_lastResolvedTryBlock == null)
            throw new Exception("Attempted to start catch block without try block!");
        if (parameters == null || !parameters.EndsWith('{')) throw new Exception($"Invalid catch statement: {Line}");
        if (parameters.Contains('('))
        {
            var statement = parameters[(parameters.IndexOf('(') + 1)..parameters.IndexOf(')')];
            var local = ProcessLocalDeclaration(statement);
            var inst = new TypeInstruction {OpCode = OperationType.CatchErrStart, Value = local.Type};
            AddInstruction(inst);
            AddInstruction(new RegisterInstruction { OpCode = OperationType.StoreVar, Value = local });
            _lastResolvedTryBlock.ErrorHandlers[local.Type] = inst;
        }
        else
        {
            var inst = new Instruction { OpCode = OperationType.CatchAllStart };
            AddInstruction(inst);
            _lastResolvedTryBlock.ErrorHandlers[TypeReference.Err] = inst;
        }

        _currentBlocks.Push((BlockType.Catch, _lastResolvedTryBlock));
    }
    
    private void ProcessSwitch(string? parameters)
    {
        if (parameters == null || parameters.Trim() != "{") throw new Exception($"'{{' must be on same line as switch for switch line: {Line}");
        _index += 1;
        var instruction = new SwitchInstruction { OpCode = OperationType.Switch, JumpTargets =[]};
        while (_index < _lines.Count && Line != "}")
        {
            var split = Line.Split("->", 2, StringSplitOptions.TrimEntries);
            if (split.Length != 2) throw new Exception($"Unexpected switch case line: {Line}");
            var offset = int.Parse(split[0]);
            if (offset != instruction.JumpTargets.Count)
            {
                throw new Exception("Switch cases must be consecutive starting from 0");
            }
            var label = split[1];
            PendLabel(label, x => instruction.JumpTargets[offset] = x);
            _index += 1;
        }
        if (_index == _lines.Count) throw new Exception("unclosed switch!");
        AddInstruction(instruction);
    }

    private static FieldReference ParseField(string fieldReference)
    {
        var firstSplit = fieldReference.Split('.');
        if (firstSplit.Length != 2) throw new Exception($"Invalid field reference {fieldReference}");
        var name = firstSplit[1];
        var secondSplit = firstSplit[0].Split(')');
        if (secondSplit.Length != 2) throw new Exception($"Invalid field reference {fieldReference}");
        TypeReference fieldType = secondSplit[0][1..];
        TypeReference fieldParent = secondSplit[1];
        return new FieldReference(fieldParent, name, fieldType);
    }

    private static MethodReference ParseMethod(string methodReference)
    {
        // This is going to be weird
        TypeReference.TypeParser parser = new TypeReference.TypeParser(methodReference);
        var parentType = parser.Parse(); // We parse our first type
        if (parser.Peek != '.') throw new Exception($"Invalid method reference {methodReference}");
        parser.Consume();
        StringBuilder methodName = new();
        while (parser.Peek != '(')
        {
            if (parser.End) throw new Exception($"Invalid method reference {methodReference}");
            if (!char.IsWhiteSpace(parser.Peek)) methodName.Append(parser.Peek);
            parser.Consume();
        }
        parser.Consume();
        while (char.IsWhiteSpace(parser.Peek)) parser.Consume();
        List<TypeReference> parameters = [];
        while (parser.Peek != ')')
        {
            if (parser.End) throw new Exception($"Invalid method reference {methodReference}");
            parameters.Add(parser.Parse());
            while (char.IsWhiteSpace(parser.Peek)) parser.Consume();
            if (parser.Peek != ',' && parser.Peek != ')')
            {
                throw new Exception($"Invalid method reference {methodReference}");
            }
            if (parser.Peek != ',') continue;
            parser.Consume();
            while (char.IsWhiteSpace(parser.Peek)) parser.Consume();
        }
        parser.Consume();
        while (char.IsWhiteSpace(parser.Peek)) parser.Consume();
        if (parser.End) return new MethodReference(parentType, methodName.ToString(), TypeReference.Void, parameters.ToArray());
        if (!parser.NextIs("->")) throw new Exception($"Invalid method reference {methodReference}");
        parser.Consume();
        parser.Consume();
        while (char.IsWhiteSpace(parser.Peek)) parser.Consume();
        var returnType = parser.Parse();
        if (!parser.End) throw new Exception($"Invalid method reference {methodReference}");
        return new MethodReference(parentType, methodName.ToString(), returnType, parameters.ToArray()); 
    }
    
    private MethodVariable ProcessLocalDeclaration(string? parameters, bool allowMultiple = false)
    {
        if (parameters == null) throw new Exception($"Expected .local <name>, <type> for line: {Line}");
        var nameThenType = parameters.Split(',', 2, StringSplitOptions.TrimEntries);
        if (nameThenType.Length != 2)
        {
            throw new Exception($"Expected .local <name>, <type> for line: {Line}");
        }

        var name = nameThenType[0];
        TypeReference type = nameThenType[1];
        if (ResolveVariableName(name) is {} current)
        {
            if (!allowMultiple)
            {
                throw new Exception($"Cannot have multiple locals with the same name, for example: {name}");
            }
            return current;
        }

        var local = new MethodVariable(false)
        {
            // This will be when adding to the method
            Index = 0,
            Name = name,
            Type = type,
            Attributes = []
        };
        _newLocals.Add(local);
        _index += 1;
        return local;
    }

    
    
    private MethodVariable? ResolveVariableName(string name)
    {
        return _variableResolver(name) ?? _newLocals.FirstOrDefault(x => x.Name == name);
    }

    private static bool BeginsWithLabel(string line)
    {
        var split = line.Split(':', 2);
        return split.Length == 2 && split[0].All(x => char.IsLetterOrDigit(x) || x == '$' || x == '_');
    }
    
    private void PendLabel(string labelName, Action<Instruction> onResolve)
    {
        if (_labels.TryGetValue(labelName, out var inst))
        {
            onResolve(inst);
        }
        else
        {
            if (_pendingJumps.TryGetValue(labelName, out var jumpList))
            {
                jumpList.Add(onResolve);
            }
            else
            {
                _pendingJumps[labelName] = [onResolve];
            }
        }
    }

    private void ResolveLabel(string labelName, Instruction resolution)
    {
        if (_pendingJumps.TryGetValue(labelName, out var toResolve))
        {
            foreach (var resolver in toResolve) resolver(resolution);
            _pendingJumps.Remove(labelName);
        }
        _labels[labelName] = resolution;
    }

    private void AddInstruction(Instruction instruction)
    {
        foreach (var action in _onNextAppend)
        {
            action(instruction);
        }
        _onNextAppend.Clear();
        _instructions.Add(instruction);
    }
    

    private static IEnumerable<string> Preprocess(string body)
    {
        var lines = body.Split('\n');
        foreach (var line in lines)
        {
            var commentsRemoved = RemoveCommentsFrom(line).Trim();
            if (commentsRemoved.Length != 0) yield return commentsRemoved;
        }
    }
    
    private static string RemoveCommentsFrom(string s)
    {
        StringBuilder sb = new();
        var state = CommentParserState.Default;
        foreach (var ch in s)
        {
            switch (state)
            {
                case CommentParserState.Default when ch == '"':
                    state = CommentParserState.InString;
                    goto default;
                case CommentParserState.Default when ch == '/':
                    state = CommentParserState.PossibleCommentBegin;
                    break;
                case CommentParserState.PossibleCommentBegin when ch != '*':
                    throw new Exception($"Expected '*' after '/' in {s}");
                case CommentParserState.PossibleCommentBegin:
                    state = CommentParserState.InComment;
                    break;
                case CommentParserState.InComment when ch == '*':
                    state = CommentParserState.PossibleCommentEnd;
                    break;
                case CommentParserState.InComment:
                    break;
                case CommentParserState.PossibleCommentEnd when ch == '/':
                    state = CommentParserState.Default;
                    break;
                case CommentParserState.PossibleCommentEnd when ch != '*':
                    state = CommentParserState.InComment;
                    break;
                case CommentParserState.PossibleCommentEnd:
                    break;
                case CommentParserState.InString when ch == '\\':
                    state = CommentParserState.StringEscapeCharacter;
                    goto default;
                case CommentParserState.InString when ch == '"':
                    state = CommentParserState.Default;
                    goto default;
                case CommentParserState.StringEscapeCharacter:
                    state = CommentParserState.InString;
                    goto default;
                case CommentParserState.InString:
                default:
                    sb.Append(ch);
                    break;
            }
        }
        return sb.ToString();
    }

    private enum CommentParserState
    {
        Default,
        InString,
        StringEscapeCharacter,
        InComment,
        PossibleCommentBegin,
        PossibleCommentEnd,
    }
}
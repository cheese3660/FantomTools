/*
using System.Text;
using System.Web;
using FantomTools.FantomPod.FantomCode.OperationFantomToolstomEditor.PodReading;

nFantomToolstomEditor.Utilities;

public class CodeWriter(PodReader podReader, FantomBuffer CodeBuffer, FMethod method)
{
    private int _nextLabel = 0;

    private string NextLabel => $"lab_{_nextLabel++}";

    private readonly List<(ushort Address, string Label, string Line)> _lines = [];
    private readonly Dictionary<ushort, string> _labels = [];
    
    private ushort _currentAddress = 0;

    private FantomStreamReader? _reader = null;

    public void Emit(StreamWriter writer)
    {
        // Now we actually have to do this stuff
        _reader = new FantomStreamReader(podReader, new MemoryStream(CodeBuffer.Buffer));
        _currentAddress = 0;
        int op = 0;
        while ((op = _reader.Stream.ReadByte()) >= 0)
        {
            var address = _currentAddress;
            _currentAddress += 1;
            // Do stuff with the op
            var line = GetOperation(op);
            var label = _labels.GetValueOrDefault(address, "");
            _lines.Add((address, label, line));
        }
        _reader.Dispose();
        var labelPadding = Math.Max(_lines.Select(x => x.Label.Length).Max() + 2, 4);
        foreach (var line in _lines)
        {
            if (line.Label.Length > 0)
            {
                writer.Write(line.Label + ":");
                for (var i = line.Label.Length + 1; i < labelPadding; i++)
                {
                    writer.Write(' ');
                }
            }
            else
            {
                for (var i = 0; i < labelPadding; i++)
                {
                    writer.Write(' ');
                }
            }

            writer.WriteLine(line.Line);
        }
    }

    private string GetOperation(int opcode)
    {
        var operationType = (OperationType)opcode;
        if (Operations.OperationsByType.TryGetValue(operationType, out var operation))
        {
            var line = new StringBuilder();
            line.Append(operation.Name);
            if (operationType == OperationType.Switch)
            {
                var count = _reader!.ReadU16();
                _currentAddress += 2;
                for (var i = 0; i < count; i++)
                {
                    if (i != 0)
                    {
                        line.Append(',');
                    }
                    line.Append($" {i} -> {GetLabel(_reader.ReadU16())}");
                    _currentAddress += 2;
                }
            }
            else
            {
                switch (operation.Signature)
                {
                    case OperationSignature.None: break;
                    case OperationSignature.Integer:
                        line.Append($" {podReader.Integers[_reader!.ReadU16()]}");
                        _currentAddress += 2;
                        break;
                    case OperationSignature.Float:
                        line.Append($" {podReader.Floats[_reader!.ReadU16()]}");
                        _currentAddress += 2;
                        break;
                    case OperationSignature.Decimal:
                        throw new NotImplementedException();
                        break;
                    case OperationSignature.String:
                        // TODO: Find a better encoding
                        line.Append($" {HttpUtility.JavaScriptStringEncode(podReader.Strings[_reader!.ReadU16()], true)}");
                        _currentAddress += 2;
                        break;
                    case OperationSignature.DurationExtensions:
                        throw new NotImplementedException();
                        break;
                    case OperationSignature.Type:
                        line.Append($" {podReader.TypeRefs[_reader!.ReadU16()]}");
                        _currentAddress += 2;
                        break;
                    case OperationSignature.Uri:
                        throw new NotImplementedException();
                        break;
                    case OperationSignature.Register:
                    {
                        var register = _reader!.ReadU16();
                        if (register == 0)
                        {
                            line.Append(" this");
                        }
                        else
                        {
                            line.Append($" {method.Variables[register - 1].Name}");
                        }
                        _currentAddress += 2;
                    }
                        break;
                    case OperationSignature.Field:
                        line.Append($" {podReader.FieldRefs[_reader!.ReadU16()]}");
                        _currentAddress += 2;
                        break;
                    case OperationSignature.Method:
                        line.Append($" {podReader.MethodRefs[_reader!.ReadU16()]}");
                        _currentAddress += 2;
                        break;
                    case OperationSignature.Jump:
                        line.Append($" {GetLabel(_reader!.ReadU16())}");
                        _currentAddress += 2;
                        break;
                    case OperationSignature.TypePair:
                        line.Append($" {podReader.TypeRefs[_reader!.ReadU16()]}, {podReader.TypeRefs[_reader.ReadU16()]}");
                        _currentAddress += 4;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }


            return line.ToString().TrimEnd();
        }
        else
        {
            throw new Exception($"Unknown opcode: {opcode}");
        }
    }
    
    private void UpdateLabel(ushort address, string newLabel)
    {
        _labels[address] = newLabel;
        if (address <= _currentAddress)
        {
            for (var i = 0; i < _lines.Count; i++)
            {
                var (addr, label, line) = _lines[i];
                if (addr == address)
                {
                    _lines[i] = (addr, newLabel, line);
                }
            }
        }
    }

    private string GetLabel(ushort address)
    {
        if (_labels.TryGetValue(address, out var label))
        {
            return label;
        }
        var lab = NextLabel;
        UpdateLabel(address, lab);
        return lab;
    }
}
*/
using FantomTools.Utilities;

namespace FantomTools.PodWriting;

public abstract class FantomTable<T> where T : IEquatable<T>
{
    private readonly List<T> _data = [];
    private readonly Dictionary<T, ushort> _indices = [];

    public T this[ushort idx] => _data[idx];
    public bool Empty => _data.Count == 0;
    
    public void WriteToStream(FantomStreamWriter writer)
    {
        writer.WriteU16((ushort)_data.Count);
        foreach (var d in _data)
        {
            WriteSingle(writer, d);
        }
    }

    protected abstract void WriteSingle(FantomStreamWriter writer, T value);

    public ushort Intern(T value)
    {
        if (_indices.TryGetValue(value, out var index))
        {
            return index;
        }

        if (_data.Count >= 65536) throw new Exception("Fantom Table is too large!");
        var idx = (ushort)_data.Count;
        _indices[value] = idx;
        _data.Add(value);
        return idx;
    }
}
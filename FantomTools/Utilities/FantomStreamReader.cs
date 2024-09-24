using System.Text;
using FantomTools.PodReading;
using Myitian.Text;

namespace FantomTools.Utilities;

internal sealed class FantomStreamReader(PodReader podReader, Stream stream) : BigEndianReader(stream, false)
{
    
    public PodReader PodReader => podReader;
    
    public List<T> ReadTable<T>(Func<FantomStreamReader, T> readValue)
    {
        var len = ReadU16();
        var result = new List<T>((int)len);
        for (var i = 0; i < len; i++)
        {
            result.Add(readValue(this));
        }
        return result;
    }
    
    public List<T> ReadTable<T>(Func<FantomStreamReader, int, T> readValue)
    {
        var len = ReadU16();
        var result = new List<T>((int)len);
        for (var i = 0; i < len; i++)
        {
            result.Add(readValue(this, i));
        }
        return result;
    }

    public string ReadName()
    {
        return podReader.Names[ReadU16()];
    }
}
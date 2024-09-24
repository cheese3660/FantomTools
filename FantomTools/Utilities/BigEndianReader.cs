using System.Text;
using Myitian.Text;

namespace FantomTools.Utilities;

public class BigEndianReader(Stream stream, bool leaveOpen=true) : IDisposable, IAsyncDisposable
{
    private static Encoding _mutf8 = new ModifiedUTF8Encoding();
    public Stream Stream => stream;

    public byte ReadU8()
    {
        var b = stream.ReadByte();
        if (b == -1) throw new EndOfStreamException();
        return (byte)b;
    }

    public sbyte ReadI8() => (sbyte)ReadU8();

    private byte[] ReadBigEndianBytes(int n)
    {
        byte[] buffer = new byte[n];
        stream.ReadExactly(buffer);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(buffer);
        }
        return buffer;
    }
    
    public ushort ReadU16()
    {
        var buffer = ReadBigEndianBytes(2);
        return BitConverter.ToUInt16(buffer);
    }
    
    public short ReadI16()
    {
        var buffer = ReadBigEndianBytes(2);
        return BitConverter.ToInt16(buffer);
    }
    
    public uint ReadU32()
    {
        var buffer = ReadBigEndianBytes(4);
        return BitConverter.ToUInt32(buffer);
    }
    
    public int ReadI32()
    {
        var buffer = ReadBigEndianBytes(4);
        return BitConverter.ToInt32(buffer);
    }
    
    public float ReadF32()
    {
        var buffer = ReadBigEndianBytes(4);
        return BitConverter.ToSingle(buffer);
    }
    
    public ulong ReadU64()
    {
        var buffer = ReadBigEndianBytes(8);
        return BitConverter.ToUInt64(buffer);
    }
    
    public long ReadI64()
    {
        var buffer = ReadBigEndianBytes(8);
        return BitConverter.ToInt64(buffer);
    }

    public double ReadF64()
    {
        var buffer = ReadBigEndianBytes(8);
        return BitConverter.ToDouble(buffer);
    }
    
    public string ReadUtf8()
    {
        var len = ReadU16();
        var buffer = new byte[len];
        stream.ReadExactly(buffer);
        return _mutf8.GetString(buffer);
    }

    public void Dispose()
    {
        if (!leaveOpen)
            stream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (!leaveOpen)
            await stream.DisposeAsync();
    }
}
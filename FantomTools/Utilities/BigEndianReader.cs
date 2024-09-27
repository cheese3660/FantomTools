using System.Text;
using JetBrains.Annotations;
using Myitian.Text;

namespace FantomTools.Utilities;

/// <summary>
/// A stream wrapper for reading big endian values from the stream, as well as java modified utf-8
/// </summary>
/// <param name="stream">The stream to wrap</param>
/// <param name="leaveOpen">Should the stream remain open after this is disposed?</param>
[PublicAPI]
public class BigEndianReader(Stream stream, bool leaveOpen=true) : IDisposable, IAsyncDisposable
{
    private static readonly Encoding ModifiedUtf8 = new ModifiedUTF8Encoding();
    /// <summary>
    /// The stream that this is wrapping
    /// </summary>
    public Stream Stream => stream;

    /// <summary>
    /// Read an unsigned byte from the stream
    /// </summary>
    /// <returns>The byte</returns>
    /// <exception cref="EndOfStreamException">Thrown if there is no more to read</exception>
    public byte ReadU8()
    {
        var b = stream.ReadByte();
        if (b == -1) throw new EndOfStreamException();
        return (byte)b;
    }

    /// <summary>
    /// Read a signed byte from the stream
    /// </summary>
    /// <returns>The byte</returns>
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
    
    /// <summary>
    /// Read an unsigned short from the stream
    /// </summary>
    /// <returns>The short</returns>
    public ushort ReadU16()
    {
        var buffer = ReadBigEndianBytes(2);
        return BitConverter.ToUInt16(buffer);
    }
    
    /// <summary>
    /// Read a signed short from the stream
    /// </summary>
    /// <returns>The short</returns>
    public short ReadI16()
    {
        var buffer = ReadBigEndianBytes(2);
        return BitConverter.ToInt16(buffer);
    }
    
    /// <summary>
    /// Read an unsigned integer from the stream
    /// </summary>
    /// <returns>The integer</returns>
    public uint ReadU32()
    {
        var buffer = ReadBigEndianBytes(4);
        return BitConverter.ToUInt32(buffer);
    }
    
    /// <summary>
    /// Read a signed integer from the stream
    /// </summary>
    /// <returns>The integer</returns>
    public int ReadI32()
    {
        var buffer = ReadBigEndianBytes(4);
        return BitConverter.ToInt32(buffer);
    }
    
    /// <summary>
    /// Read a float from the stream
    /// </summary>
    /// <returns>The float</returns>
    public float ReadF32()
    {
        var buffer = ReadBigEndianBytes(4);
        return BitConverter.ToSingle(buffer);
    }
    
    /// <summary>
    /// Read an unsigned long from the stream
    /// </summary>
    /// <returns>The long</returns>
    public ulong ReadU64()
    {
        var buffer = ReadBigEndianBytes(8);
        return BitConverter.ToUInt64(buffer);
    }
    
    /// <summary>
    /// Read a signed long from the stream
    /// </summary>
    /// <returns>The long</returns>
    public long ReadI64()
    {
        var buffer = ReadBigEndianBytes(8);
        return BitConverter.ToInt64(buffer);
    }

    /// <summary>
    /// Read a double from the stream
    /// </summary>
    /// <returns>The double</returns>
    public double ReadF64()
    {
        var buffer = ReadBigEndianBytes(8);
        return BitConverter.ToDouble(buffer);
    }
    
    /// <summary>
    /// Read a java-modified utf-8 string from the stream
    /// </summary>
    /// <returns>The string</returns>
    public string ReadUtf8()
    {
        var len = ReadU16();
        var buffer = new byte[len];
        stream.ReadExactly(buffer);
        return ModifiedUtf8.GetString(buffer);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!leaveOpen)
            stream.Dispose();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (!leaveOpen)
            await stream.DisposeAsync();
    }
}
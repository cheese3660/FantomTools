using System.Text;
using JetBrains.Annotations;
using Myitian.Text;

namespace FantomTools.Utilities;

/// <summary>
/// A stream wrapper for writing big endian values to the stream, as well as java modified utf-8
/// </summary>
/// <param name="stream">The stream to wrap</param>
/// <param name="leaveOpen">Should the stream remain open after this is disposed?</param>
[PublicAPI]
public class BigEndianWriter(Stream stream, bool leaveOpen = true) : IDisposable, IAsyncDisposable
{
    private static readonly Encoding ModifiedUtf8 = new ModifiedUTF8Encoding();
    
    /// <summary>
    /// The stream that this is wrapping
    /// </summary>
    public Stream Stream => stream;
    
    private void WriteBigEndianBytes(byte[] buffer)
    {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(buffer);
        }

        stream.Write(buffer);
    }

    /// <summary>
    /// Write an unsigned byte to the stream
    /// </summary>
    /// <param name="b">The byte</param>
    public void WriteU8(byte b)
    {
         stream.WriteByte(b);
    }

    /// <summary>
    /// Write a signed byte to the stream
    /// </summary>
    /// <param name="b">The byte</param>
    public void WriteI8(sbyte b)
    {
        stream.WriteByte((byte)b);
    }

    /// <summary>
    /// Write an unsigned short to the stream
    /// </summary>
    /// <param name="v">The short</param>
    public void WriteU16(ushort v)
    {
        WriteBigEndianBytes(BitConverter.GetBytes(v));
    }
    
    /// <summary>
    /// Write a signed short to the stream
    /// </summary>
    /// <param name="v">The short</param>
    public void WriteI16(short v)
    {
        WriteBigEndianBytes(BitConverter.GetBytes(v));
    }
    
    /// <summary>
    /// Write an unsigned integer to the stream
    /// </summary>
    /// <param name="v">The integer</param>
    public void WriteU32(uint v)
    {
        WriteBigEndianBytes(BitConverter.GetBytes(v));
    }
    
    /// <summary>
    /// Write a signed integer to the stream
    /// </summary>
    /// <param name="v">The integer</param>
    public void WriteI32(int v)
    {
        WriteBigEndianBytes(BitConverter.GetBytes(v));
    }
    /// <summary>
    /// Write a float to the stream
    /// </summary>
    /// <param name="v">The float</param>
    public void WriteF32(float v)
    {
        WriteBigEndianBytes(BitConverter.GetBytes(v));
    }
    /// <summary>
    /// Write an unsigned long to the stream
    /// </summary>
    /// <param name="v">The long</param>
    public void WriteU64(ulong v)
    {
        WriteBigEndianBytes(BitConverter.GetBytes(v));
    }
    
    /// <summary>
    /// Write a signed long to the stream
    /// </summary>
    /// <param name="v">The long</param>
    public void WriteI64(long v)
    {
        WriteBigEndianBytes(BitConverter.GetBytes(v));
    }
    
    /// <summary>
    /// Write a double to the stream
    /// </summary>
    /// <param name="v">The double</param>
    public void WriteF64(double v)
    {
        WriteBigEndianBytes(BitConverter.GetBytes(v));
    }

    /// <summary>
    /// Write a string to the stream in java modified utf-8 encoding
    /// </summary>
    /// <param name="s">The string</param>
    public void WriteUtf8(string s)
    {
        var bytes = ModifiedUtf8.GetBytes(s);
        WriteU16((ushort)bytes.Length);
        stream.Write(bytes);
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

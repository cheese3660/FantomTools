using System.Text;
using Myitian.Text;

namespace FantomTools.Utilities;

public class FantomStreamWriter(Stream stream)
{
    private static Encoding _mutf8 = new ModifiedUTF8Encoding();
    
    public Stream Stream => stream;
    
    private void WriteBigEndianBytes(byte[] buffer)
    {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(buffer);
        }

        stream.Write(buffer);
    }

    public void WriteU8(byte b)
    {
         stream.WriteByte(b);
    }

    public void WriteI8(sbyte b)
    {
        stream.WriteByte((byte)b);
    }

    public void WriteU16(ushort v)
    {
        WriteBigEndianBytes(BitConverter.GetBytes(v));
    }
    
    public void WriteI16(short v)
    {
        WriteBigEndianBytes(BitConverter.GetBytes(v));
    }
    
    public void WriteU32(uint v)
    {
        WriteBigEndianBytes(BitConverter.GetBytes(v));
    }
    
    public void WriteI32(int v)
    {
        WriteBigEndianBytes(BitConverter.GetBytes(v));
    }
    public void WriteF32(float v)
    {
        WriteBigEndianBytes(BitConverter.GetBytes(v));
    }
    public void WriteU64(ulong v)
    {
        WriteBigEndianBytes(BitConverter.GetBytes(v));
    }
    
    public void WriteI64(long v)
    {
        WriteBigEndianBytes(BitConverter.GetBytes(v));
    }
    
    public void WriteF64(double v)
    {
        WriteBigEndianBytes(BitConverter.GetBytes(v));
    }

    public void WriteUtf8(string s)
    {
        var bytes = _mutf8.GetBytes(s);
        WriteU16((ushort)bytes.Length);
        stream.Write(bytes);
    }
}

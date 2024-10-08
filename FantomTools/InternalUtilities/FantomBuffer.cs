﻿using System.Text;
using FantomTools.Utilities;
using Myitian.Text;

namespace FantomTools.InternalUtilities;

internal class FantomBuffer(byte[] buffer, ushort len)
{
    private static readonly Encoding MUtf8 = new ModifiedUTF8Encoding();
    public byte[] Buffer => buffer;
    private ushort Length => len;

    public static FantomBuffer? Read(FantomStreamReader reader)
    {
        var length = reader.ReadU16();
        if (length == 0) return null;
        var buffer = new byte[length];
        reader.Stream.ReadExactly(buffer);
        return new FantomBuffer(buffer, length);
    }

    public void WriteBuffer(BigEndianWriter writer)
    {
        writer.WriteU16(Length);
        writer.Stream.Write(Buffer);
    }
}
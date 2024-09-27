using FantomTools.Utilities;

namespace FantomTools.InternalUtilities;

internal class FantomBufferStream(Stream baseStream) : BigEndianWriter(new MemoryStream(), false), IDisposable, IAsyncDisposable
{
    public void Dispose()
    {
        var baseStreamWriter = new BigEndianWriter(baseStream);
        Stream.Seek(0, SeekOrigin.Begin);
        baseStreamWriter.WriteU16((ushort)Stream.Length);
        Stream.CopyTo(baseStream);
        base.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        var baseStreamWriter = new BigEndianWriter(baseStream);
        Stream.Seek(0, SeekOrigin.Begin);
        baseStreamWriter.WriteU16((ushort)Stream.Length);
        await Stream.CopyToAsync(baseStream);
        await base.DisposeAsync();
    }
}
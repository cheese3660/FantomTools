using System.IO.Compression;
using FantomTools.PodReading;

namespace FantomTools.InternalUtilities;

internal sealed class ZipPod(ZipArchive archive, PodReader podReader) : IDisposable
{
    public ZipArchive Archive => archive;
    
    public List<string> MetaFiles => archive.Entries.Where(x =>
            !x.FullName.StartsWith("fcode/") && !x.FullName.StartsWith("fcode\\") && !x.FullName.EndsWith(".class") && x.FullName != "meta.props")
        .Select(x => x.FullName).ToList();
    
    public FantomStreamReader? ReadFile(string path, bool required=false)
    {
        var entry = archive.GetEntry(path);
        if (entry == null)
        {
            if (required)
            {
                throw new Exception($"Missing required file \"{path}\" in pod");
            }
            else
            {
                return null;
            }
        }

        return new FantomStreamReader(podReader, entry.Open());
    }

    public Stream CreateFile(string path)
    {
        var entry = archive.CreateEntry(path);
        return entry.Open();
    }

    public void Dispose()
    {
        archive.Dispose();
    }
}
using System.IO.Compression;
using FantomTools.Fantom;
using FantomTools.InternalUtilities;
using FantomTools.Utilities;

namespace FantomTools.PodReading;

internal sealed class PodReader : IDisposable
{
    public readonly ZipPod Archive;
    
    
    internal readonly List<string> Names;
    internal readonly List<TypeReferenceReader> TypeRefs;
    internal readonly List<FieldReferenceReader> FieldRefs;
    internal readonly List<MethodReferenceReader> MethodRefs;
    private readonly List<TypeReader> _types = [];
    internal readonly List<long> Integers;
    internal readonly List<double> Floats;
    internal readonly List<string> Strings;
    internal readonly List<string> Decimals;
    internal readonly List<string> Uris;
    internal readonly List<long> Durations;
    private string _fCodeVersion = "";
    
    public PodReader(FileInfo podFile) : this(ZipFile.OpenRead(podFile.FullName))
    {
    }

    public PodReader(ZipArchive podFile)
    {
        Archive = new ZipPod(podFile, this);
        ReadMeta(Archive.ReadFile("meta.props")!);
        Names = ReadTable("fcode/names.def", reader => reader.ReadUtf8());
        TypeRefs = ReadTable("fcode/typeRefs.def", TypeReferenceReader.Read);
        FieldRefs = ReadTable("fcode/fieldRefs.def", FieldReferenceReader.Read);
        MethodRefs = ReadTable("fcode/methodRefs.def", MethodReferenceReader.Read);
        Integers = ReadTable("fcode/ints.def", reader => reader.ReadI64());
        Floats = ReadTable("fcode/floats.def", reader => reader.ReadF64());
        Decimals = ReadTable("fcode/decimals.def", reader => reader.ReadUtf8());
        Strings = ReadTable("fcode/strs.def", reader => reader.ReadUtf8());
        Durations = ReadTable("fcode/durations.def", reader => reader.ReadI64());
        Uris = ReadTable("fcode/uris.def", reader => reader.ReadUtf8());
        
        var typeMetaStream = Archive.ReadFile("fcode/types.def");
        if (typeMetaStream != null)
        {
            ReadTypeMeta(typeMetaStream);
        }

        foreach (var type in _types)
        {
            type.Read();
        }
    }

    private List<T> ReadTable<T>(string path, Func<FantomStreamReader, T> readFunc)
    {
        using var stream = Archive.ReadFile(path);
        return stream?.ReadTable(readFunc) ?? [];
    }

    private void ReadMeta(FantomStreamReader streamReader)
    {
        var props = new Properties();
        props.Load(streamReader.Stream);
        streamReader.Dispose();
        _fCodeVersion = props.GetProperty("fcode.version");
        if (_fCodeVersion != "1.0.51") Console.Error.WriteLine($"Warning: Possibly invalid fcode version: {_fCodeVersion}, this tool was designed for v1.0.51");
    }

    private void ReadTypeMeta(FantomStreamReader streamReader)
    {
        var count = streamReader.ReadU16();
        _types.EnsureCapacity(count);
        for (var i = 0; i < count; i++)
        {
            _types.Add(TypeReader.ReadMeta(streamReader));
        }

        streamReader.Dispose();
    }

    public Pod ToMemoryPod()
    {
        var resultPod = new Pod(Archive);
        using var metaStream = Archive.Archive.GetEntry("meta.props")!.Open();
        resultPod.LoadMeta(metaStream);
        foreach (var type in _types)
        {
            resultPod.AddType(type.ToMemoryType(resultPod));
        }
        return resultPod;
    }

    public void Dispose()
    {
        Archive.Dispose();
    }
}
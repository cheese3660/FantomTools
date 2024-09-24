using System.IO.Compression;
using FantomTools.PodReading;
using FantomTools.PodWriting;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom;

[PublicAPI]
public sealed class Pod : IDisposable
{
    private readonly MemoryStream _backingStream;
    private readonly ZipArchive _nonPodFiles;
    public PodMeta MetaData = new();
    public List<Type> Types = [];
    
    public Pod()
    {
        _backingStream = new MemoryStream();
        _nonPodFiles = new ZipArchive(_backingStream, ZipArchiveMode.Update, true);
    }

    public void AddType(Type t)
    {
        if (t.TypePod != this) throw new Exception("Type does not come from this pod!");
        Types.Add(t);
    }

    public Type CreateType(string name)
    {
        var result = new Type(this, name);
        Types.Add(result);
        return result;
    }

    public Type GetType(string name)
    {
        return Types.First(x => x.Name == name);
    }

    public Type CreateType(string name, TypeReference baseType, Flags flags = 0, params TypeReference[] mixins)
    {
        var result = new Type(this, name, baseType, flags, mixins);
        Types.Add(result);
        return result;
    }
    
    internal Pod(ZipPod backingPod) : this()
    {
        foreach (var metaName in backingPod.MetaFiles)
        {
            using var readStream = backingPod.Archive.GetEntry(metaName)!.Open();
            using var writeStream = _nonPodFiles.CreateEntry(metaName).Open();
            readStream.CopyTo(writeStream);
        }
    }

    public static Pod FromFile(string podFile)
    {
        using var readPod = new PodReader(new FileInfo(podFile));
        return readPod.ToMemoryPod();
    }

    public void LoadMeta(Stream stream)
    {
        MetaData.Read(stream);
    }

    public Stream OpenDataFile(string path, bool createIfNotFound = false)
    {
        if (_nonPodFiles.GetEntry(path) is { } entry)
        {
            return entry.Open();
        } 
        if (createIfNotFound)
        {
            return _nonPodFiles.CreateEntry(path).Open();
        }
        throw new Exception($"Path not found in data: {path}");
    }
    
    public void WritePod(ZipArchive outArchive)
    {
        // First let's copy the non pod data over
        foreach (var file in _nonPodFiles.Entries)
        {
            using var readEntry = file.Open();
            using var writeEntry = outArchive.CreateEntry(file.FullName).Open();
            readEntry.CopyTo(writeEntry);
        }
        // Then let's copy meta.props over
        using var propsStream = outArchive.CreateEntry("meta.props").Open();
        MetaData.Write(propsStream);
        
        // Now we have to recreate every single table before we emit everything :3
        FantomTables tables = new();
        foreach (var type in Types)
        {
            // We intern the type
            tables.Types.Intern(type);
            var path = $"fcode/{type.Name}.fcode";
            using var typeStream = outArchive.CreateEntry(path).Open();
            var writer = new FantomStreamWriter(typeStream);
            type.EmitBody(writer, tables);
        }
        // And after we've written every type
        tables.WriteTableDefinitions(outArchive);
    }


    public void Dispose()
    {
        _nonPodFiles.Dispose();
        _backingStream.Dispose();
    }
}
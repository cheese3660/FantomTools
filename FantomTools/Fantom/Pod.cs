using System.IO.Compression;
using FantomTools.InternalUtilities;
using FantomTools.PodReading;
using FantomTools.PodWriting;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom;


/// <summary>
/// This is an in memory representation of a Fantom Pod
/// </summary>
[PublicAPI]
public sealed class Pod : IDisposable
{
    private readonly MemoryStream _backingStream;
    private readonly ZipArchive _nonPodFiles;
    
    /// <summary>
    /// The pod's metadata
    /// </summary>
    public PodMeta MetaData = new();
    
    /// <summary>
    /// All the types contained in the pod
    /// </summary>
    public List<Type> Types = [];
    
    /// <summary>
    /// The paths of all the data files (i.e. non-code/meta.props files) that this pod currently has
    /// </summary>
    public IEnumerable<string> DataFiles => _nonPodFiles.Entries.Select(x => x.FullName);
    
    
    /// <summary>
    /// Create an empty Pod
    /// Make sure to set up the metadata before writing this out!
    /// </summary>
    public Pod()
    {
        _backingStream = new MemoryStream();
        _nonPodFiles = new ZipArchive(_backingStream, ZipArchiveMode.Update, true);
    }

    /// <summary>
    /// Add a type to this pod
    /// </summary>
    /// <param name="typeToAdd">The type to add to this pod</param>
    /// <exception cref="ArgumentException">Thrown if the types pod is not the current Pod instance</exception>
    public void AddType(Type typeToAdd)
    {
        if (typeToAdd.TypePod != this) throw new ArgumentException("Type does not come from this pod!");
        Types.Add(typeToAdd);
    }
    
    /// <summary>
    /// Creates a type, and adds it to this pod
    /// </summary>
    /// <param name="name">The name of the new type</param>
    /// <param name="baseType">The type this type inherits from</param>
    /// <param name="flags">The flags that this type has, including visibility, abstractness, etc...</param>
    /// <param name="mixins">The mixins this type has</param>
    /// <returns>The newly created type</returns>
    public Type CreateType(string name, TypeReference? baseType = null, Flags flags = Flags.Public, params TypeReference[] mixins)
    {
        var result = new Type(this, name, baseType ?? TypeReference.Object, flags, mixins);
        Types.Add(result);
        return result;
    }

    /// <summary>
    /// Gets a type in the pod by name
    /// </summary>
    /// <param name="name">The name of the type</param>
    /// <returns>The type if it is found, otherwise null</returns>
    public Type? GetType(string name)
    {
        return Types.FirstOrDefault(x => x.Name == name);
    }

    /// <summary>
    /// Attempts to get a type in the pod by name
    /// </summary>
    /// <param name="name">The name of the type</param>
    /// <param name="type">Set to the type if it is found, otherwise null</param>
    /// <returns>True if the type is found</returns>
    public bool TryGetType(string name, out Type? type)
    {
        if (GetType(name) is { } t)
        {
            type = t;
            return true;
        }
        type = null;
        return false;
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

    /// <summary>
    /// Reads a Pod from a file
    /// </summary>
    /// <param name="podFile">The pod file</param>
    /// <returns>A memory representation of the pod read from the file</returns>
    public static Pod FromFile(string podFile)
    {
        using var readPod = new PodReader(new FileInfo(podFile));
        return readPod.ToMemoryPod();
    }

    internal void LoadMeta(Stream stream)
    {
        MetaData.Read(stream);
    }

    /// <summary>
    /// Opens a data file for reading or writing
    /// </summary>
    /// <param name="path">The path to the data file in the pod</param>
    /// <param name="createIfNotFound">Should the file be created if it does not exist</param>
    /// <returns>The stream of the data file that was opened</returns>
    /// <exception cref="Exception">Thrown if createIfNotFound is false and the path does not exist</exception>
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

    /// <summary>
    /// Attempts to open a data file for reading/writing
    /// </summary>
    /// <param name="path">The path to the data file in the pod</param>
    /// <param name="result">Set to the stream of the data file if it exists, otherwise null</param>
    /// <returns>True if the stream was opened</returns>
    public bool TryOpenDataFile(string path, out Stream? result)
    {
        if (_nonPodFiles.GetEntry(path) is { } entry)
        {
            result = entry.Open();
            return true;
        }
        result = null;
        return false;
    }

    /// <summary>
    /// Checks if a path exists in the pod's data files
    /// </summary>
    /// <param name="path">The path to check</param>
    /// <returns>True if the path exists</returns>
    public bool HasDataFile(string path) => _nonPodFiles.GetEntry(path) is not null;

    /// <summary>
    /// Trys to delete a data file from the pod
    /// </summary>
    /// <param name="path">The path in the pod to delete</param>
    public void RemoveDataFile(string path)
    {
        if (_nonPodFiles.GetEntry(path) is not { } entry) return;
        entry.Delete();
    }
    
    /// <summary>
    /// Saves a pod to a zip archive
    /// </summary>
    /// <param name="outArchive">The archive to save the pod to</param>
    public void Save(ZipArchive outArchive)
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
            var writer = new BigEndianWriter(typeStream);
            type.EmitBody(writer, tables);
        }
        // And after we've written every type
        tables.WriteTableDefinitions(outArchive);
    }

    /// <summary>
    /// Writes a pod to a stream
    /// </summary>
    /// <param name="stream">The stream to write the pod to</param>
    public void Save(Stream stream) => Save(new ZipArchive(stream, ZipArchiveMode.Create, true));
    
    /// <summary>
    /// Disposes of the resources the pod holds
    /// </summary>
    public void Dispose()
    {
        _nonPodFiles.Dispose();
        _backingStream.Dispose();
    }
}
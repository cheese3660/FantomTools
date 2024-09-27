using System.IO.Compression;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.PodWriting;

[PublicAPI]
internal class FantomTables
{
    public StringTable Names = new();
    public TypeReferenceTable TypeReferences;
    public FieldReferenceTable FieldReferences;
    public MethodReferenceTable MethodReferences;
    public TypeMetaTable Types;
    public StringTable Strings = new();
    public FloatTable Floats = new();
    public IntegerTable Integers = new();
    public StringTable Decimals = new();
    public IntegerTable Durations = new();
    public StringTable Uris = new();
    // Now we need an integer/float tables from now

    public FantomTables()
    {
        TypeReferences = new TypeReferenceTable(this);
        MethodReferences = new MethodReferenceTable(this);
        FieldReferences = new FieldReferenceTable(this);
        Types = new TypeMetaTable(this);
    }

    // Since this can modify the table, we go in reverse dependency order
    // We also want to have output all type fcode before this as well
    public void WriteTableDefinitions(ZipArchive outArchive)
    {
        WriteTable(Uris, "fcode/uris.def", outArchive);
        WriteTable(Durations, "fcode/durations.def", outArchive);
        WriteTable(Decimals, "fcode/decimals.def", outArchive);
        WriteTable(Integers, "fcode/ints.def", outArchive);
        WriteTable(Strings, "fcode/strs.def", outArchive);
        WriteTable(Floats, "fcode/floats.def", outArchive);
        WriteTable(Types, "fcode/types.def", outArchive);
        WriteTable(MethodReferences, "fcode/methodRefs.def", outArchive);
        WriteTable(FieldReferences, "fcode/fieldRefs.def", outArchive);
        WriteTable(TypeReferences, "fcode/typeRefs.def", outArchive);
        WriteTable(Names, "fcode/names.def", outArchive);
    }

    private static void WriteTable<T>(FantomTable<T> table, string path, ZipArchive archive) where T : IEquatable<T>
    {
        if (table.Empty) return;
        using var stream = archive.CreateEntry(path).Open();
        var writer = new BigEndianWriter(stream);
        table.WriteToStream(writer);
    }
}
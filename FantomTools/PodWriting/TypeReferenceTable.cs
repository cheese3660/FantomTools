﻿using FantomTools.Fantom;
using FantomTools.Utilities;

namespace FantomTools.PodWriting;

public class TypeReferenceTable(FantomTables tables) : FantomTable<TypeReference>
{
    protected override void WriteSingle(FantomStreamWriter writer, TypeReference value)
    {
        writer.WriteU16(tables.Names.Intern(value.Pod));
        writer.WriteU16(tables.Names.Intern(value.Name));
        writer.WriteUtf8(value.SerializedSignature);
    }
}
// See https://aka.ms/new-console-template for more information

using System.IO.Compression;
using System.Text;
using FantomTools.Fantom;
using FantomTools.Fantom.Code.Operations;

switch (args[0])
{
    case "edit":
    {
        var pod = Pod.FromFile(args[1]);
        var cursor = pod.GetType("Main").GetMethod("main").Body.Cursor;
        cursor.Seek(x => x.OpCode == OperationType.JumpFalse);
        cursor.Current.OpCode = OperationType.JumpTrue;

        if (File.Exists(args[2]))
        {
            File.Delete(args[2]);
        }
        using var outPod = ZipFile.Open(args[2], ZipArchiveMode.Create);
        pod.Save(outPod);
        break;
    }
    case "dump":
    {
        var pod = Pod.FromFile(args[1]);
        var sb = new StringBuilder();
        foreach (var type in pod.Types)
        {
            sb.AppendLine(type.Dump(true));
        }
        File.WriteAllText(args[2], sb.ToString());
        break;
    }
    case "roundtrip":
    {
        var pod = Pod.FromFile(args[1]);
        using var outPod = ZipFile.Open(args[2], ZipArchiveMode.Create);
        pod.Save(outPod);
        break;
    }
}
// See https://aka.ms/new-console-template for more information

using System.IO.Compression;
using System.Text;
using FantomTools.Fantom;
using FantomTools.Fantom.Code;
using FantomTools.Fantom.Code.AssemblyTools;
using FantomTools.Fantom.Code.Operations;

// Console.WriteLine(MethodAssembler.ParseMethod("skyarcd::LicRuntime.isNearCapacity(sys::Int,sys::Int) -> sys::Bool"));

// sys::Obj.echo(sys::Obj?) -> sys::Void

const string toInsert = """
                        /* Load 7, but under a try catch */
                        try {
                          ld.var instance
                          ld.int 7
                          st.instance (sys::Int)fantom_test::Main.x
                          ld.var instance
                          call.virtual fantom_test::Main.assembleMe() -> sys::Void
                          leave $FIN
                        } 
                        catch {
                          call.static fantom_test::Main.eliminate() -> sys::Void
                          leave $FIN
                        }
                        """;

const string toAssemble = """
                              ld.var this
                              ld.instance (sys::Int)fantom_test::Main.x
                              ld.int 7
                              cmp.ne sys::Int; sys::Int
                              jmp.true dont_throw
                              ld.str ""
                              new sys::Err.make(sys::Str)
                              throw
                          dont_throw:
                              ld.var this
                              ld.instance (sys::Int)fantom_test::Main.x
                              coerce sys::Int; sys::Obj?
                              call.static sys::Obj.echo(sys::Obj?) -> sys::Void
                              ret
                          """;

const string toCreate = """
                            ld.str "Ohio will be eliminated!"
                            call.static sys::Obj.echo(sys::Obj?) -> sys::Void
                            ret
                        """;

switch (args[0])
{
    case "edit":
    {
        var pod = Pod.FromFile(args[1]);
        var main = pod.GetType("Main")!;
        var cursor = main.GetMethod("main").Body.Cursor;
        cursor.Seek(x => x.OpCode == OperationType.CallVirtual, MethodCursor.SeekMode.After);
        cursor.InsertAssembly(toInsert);
        var assembleMe = main.GetMethod("assembleMe");
        assembleMe.AssembleFrom(toAssemble);
        var eliminate = main.AddMethod("eliminate", TypeReference.Void);
        eliminate.Flags |= Flags.Static;
        eliminate.AssembleFrom(toCreate);
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
            sb.AppendLine(type.Dump(true,true));
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
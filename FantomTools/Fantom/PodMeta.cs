using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom;

[PublicAPI]
public class PodMeta
{
    // pod.name
    public string PodName = "";
    // pod.version
    public Version PodVersion = new("0.0.0.0");
    // pod.depends
    public List<PodDependency> Dependencies = [];
    // pod.summary
    public string Summary = "";
    // pod.isScript
    public bool IsScript;
    // fcode.version
    public Version FCodeVersion = new("1.0.5.1");
    // build.host
    public string BuildHost = "";
    // build.user
    public string BuildUser = "";
    // build.ts
    public string BuildTimeStamp = "";
    // build.tsKey
    public string BuildTimeStampKey = "";
    // build.compiler
    public Version CompilerVersion = new("1.0.80");
    // build.platform
    public string Platform = "";
    // pod.docApi
    public bool DocumentsApi;
    // pod.docSrc
    public bool DocumentsSource;
    // pod.native.java
    public bool NativeJava;
    // pod.native.jni
    public bool NativeJni;
    // pod.native.dotnet
    public bool NativeDotnet;
    // pod.native.js
    public bool NativeJavaScript;
    // pod.fcode
    public bool FCode = true;
    // pod.js
    public bool JavaScript;

    public void Read(Stream propsFile)
    {
        var props = new Properties();
        props.Load(propsFile);
        PodName = props.GetPropertyOrDefault("pod.name") ?? PodName;
        PodVersion = new Version(props.GetPropertyOrDefault("pod.version") ?? PodVersion.ToString());
        var dependsString = props.GetPropertyOrDefault("pod.depends") ?? "";
        foreach (var depends in dependsString.Split(';'))
        {
            Dependencies.Add(PodDependency.Parse(depends));
        }

        Summary = props.GetPropertyOrDefault("pod.summary") ?? Summary;
        IsScript = StringToBool(props.GetPropertyOrDefault("pod.isScript"), IsScript);
        FCodeVersion = new Version(props.GetPropertyOrDefault("fcode.version") ?? FCodeVersion.ToString());
        BuildHost = props.GetPropertyOrDefault("build.host") ?? BuildHost;
        BuildUser = props.GetPropertyOrDefault("build.user") ?? BuildUser;
        BuildTimeStamp = props.GetPropertyOrDefault("build.ts") ?? BuildTimeStamp;
        BuildTimeStampKey = props.GetPropertyOrDefault("build.tsKey") ?? BuildTimeStampKey;
        CompilerVersion = new Version(props.GetPropertyOrDefault("build.compiler") ?? CompilerVersion.ToString());
        Platform = props.GetPropertyOrDefault("build.platform") ?? Platform;
        DocumentsApi = StringToBool(props.GetPropertyOrDefault("pod.docApi"), DocumentsApi);
        DocumentsSource = StringToBool(props.GetPropertyOrDefault("pod.docSrc"), DocumentsSource);
        NativeJava = StringToBool(props.GetPropertyOrDefault("pod.native.java"), NativeJava);
        NativeJni = StringToBool(props.GetPropertyOrDefault("pod.native.jni"), NativeJni);
        NativeDotnet = StringToBool(props.GetPropertyOrDefault("pod.native.dotnet"), NativeDotnet);
        NativeJavaScript = StringToBool(props.GetPropertyOrDefault("pod.native.javascript"), NativeJavaScript);
        FCode = StringToBool(props.GetPropertyOrDefault("pod.fcode"), FCode);
        JavaScript = StringToBool(props.GetPropertyOrDefault("pod.js"), JavaScript);
    }

    public void Write(Stream propsFile)
    {
        var props = new Properties();
        WriteOptionalString(props, "pod.name", PodName);
        props.SetProperty("pod.version", PodVersion.ToString());
        WriteOptionalString(props, "pod.depends", string.Join("; ", Dependencies.Select(x => x.ToString())));
        WriteOptionalString(props, "pod.summary", Summary);
        WriteBoolean(props, "pod.isScript", IsScript);
        props.SetProperty("fcode.version", FCodeVersion.ToString());
        WriteOptionalString(props, "build.host", BuildHost);
        WriteOptionalString(props, "build.user", BuildUser);
        WriteOptionalString(props, "build.ts", BuildTimeStamp);
        WriteOptionalString(props, "build.tsKey", BuildTimeStampKey);
        props.SetProperty("build.compiler", CompilerVersion.ToString());
        WriteOptionalString(props, "build.platform", Platform);
        WriteBoolean(props, "pod.docApi", DocumentsApi);
        WriteBoolean(props, "pod.docSrc", DocumentsSource);
        WriteBoolean(props, "pod.native.java", NativeJava);
        WriteBoolean(props, "pod.native.jni", NativeJni);
        WriteBoolean(props, "pod.native.dotnet", NativeDotnet);
        WriteBoolean(props, "pod.native.javascript", NativeJavaScript);
        WriteBoolean(props, "pod.fcode", FCode);
        WriteBoolean(props, "pod.js", JavaScript);
        props.Store(propsFile);
    }

    private static void WriteBoolean(Properties props, string key, bool property)
    {
        props.SetProperty(key, property ? "true" : "false");
    }
    private static void WriteOptionalString(Properties props, string key, string property)
    {
        if (!string.IsNullOrEmpty(property)) props.SetProperty(key,property);
    }
    
    private static bool StringToBool(string? property, bool onNull)
    {
        return property switch
        {
            "true" => true,
            "false" => false,
            _ => onNull
        };
    }
}
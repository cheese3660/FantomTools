using FantomTools.InternalUtilities;
using FantomTools.Utilities;
using JetBrains.Annotations;

namespace FantomTools.Fantom;

/// <summary>
/// Represents the metadata about a pod
/// </summary>
[PublicAPI]
public class PodMeta
{
    /// <summary>
    /// The name of the pod
    /// </summary>
    public string PodName = "";
    /// <summary>
    /// The version of the pod
    /// </summary>
    public Version PodVersion = new("0.0.0.0");
    /// <summary>
    /// The dependencies of the pod
    /// </summary>
    public List<PodDependency> Dependencies = [];
    /// <summary>
    /// A description of the pod
    /// </summary>
    public string Summary = "";
    /// <summary>
    /// Is the pod a script
    /// </summary>
    public bool IsScript;
    /// <summary>
    /// The version of the FCode contained within the pod
    /// </summary>
    public Version FCodeVersion = new("1.0.5.1");
    /// <summary>
    /// The host that built the pod
    /// </summary>
    public string BuildHost = "";
    /// <summary>
    /// The user that built the pod
    /// </summary>
    public string BuildUser = "";
    /// <summary>
    /// A timestamp of when the pod was built
    /// </summary>
    public string BuildTimeStamp = "";
    /// <summary>
    /// Unknown?
    /// </summary>
    public string BuildTimeStampKey = "";
    /// <summary>
    /// The version of the compiler that built the pod
    /// </summary>
    public Version CompilerVersion = new("1.0.80");
    /// <summary>
    /// The platform the pod was built on
    /// </summary>
    public string Platform = "";
    /// <summary>
    /// Does the pod have API documentation?
    /// </summary>
    public bool DocumentsApi;
    /// <summary>
    /// Does the pod have source code documentation?
    /// </summary>
    public bool DocumentsSource;
    /// <summary>
    /// Does the pod have java code?
    /// </summary>
    public bool NativeJava;
    /// <summary>
    /// Does the pod have jni code?
    /// </summary>
    public bool NativeJni;
    /// <summary>
    /// Does the pod have dotnet code?
    /// </summary>
    public bool NativeDotnet;
    /// <summary>
    /// Does the pod have native js code?
    /// </summary>
    public bool NativeJavaScript;
    /// <summary>
    /// Does the pod have fantom bytecode?
    /// </summary>
    public bool FCode = true;
    /// <summary>
    /// Does the pod have javascript code?
    /// </summary>
    public bool JavaScript;

    internal void Read(Stream propsFile)
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

    internal void Write(Stream propsFile)
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
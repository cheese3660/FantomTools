using System.Text;
using Myitian.Text;

namespace FantomTools.Utilities;

public class Properties
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);
    public Dictionary<string, string> Props = new();

    public string GetProperty(string key) => Props[key];
    public void SetProperty(string key, string value) => Props[key] = value;
    public string? GetPropertyOrDefault(string key) => Props.GetValueOrDefault(key);
    
    public void Load(Stream input)
    {
        using var reader = new StreamReader(input, Utf8NoBom);
        var result = reader.ReadToEnd();
        var lines = result.Split((char[])['\r', '\n'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var pair = line.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
            Props[pair[0]] = pair[1];
        }
    }

    public void Store(Stream output)
    {
        using var writer = new StreamWriter(output, Utf8NoBom);
        foreach (var (key, value) in Props)
        {
            writer.WriteLine($"{key}={value}");
        }
    }
}
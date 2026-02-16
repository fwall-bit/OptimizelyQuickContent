using System.IO;
using System.Text.Json;

namespace OptimizelyQuickContent;

public class ConnectionInfo {
    public string DisplayName { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;

    public override string ToString() => DisplayName;
}

public static class ConnectionStore {
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "OptimizelyQuickContent",
        "connections.json");

    private static readonly JsonSerializerOptions JsonOptions = new() {
        WriteIndented = true
    };

    public static List<ConnectionInfo> Load() {
        if (!File.Exists(FilePath))
            return [];

        var json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<List<ConnectionInfo>>(json, JsonOptions) ?? [];
    }

    public static void Save(IEnumerable<ConnectionInfo> connections) {
        var dir = Path.GetDirectoryName(FilePath)!;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(connections, JsonOptions);
        File.WriteAllText(FilePath, json);
    }
}
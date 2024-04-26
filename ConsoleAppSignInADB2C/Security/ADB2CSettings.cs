public class ADB2CSettings
{
    public string? ConsoleAppId { get; set; }
    public string? ConsoleAppSecret { get; set; }
    public GraphApiSettings? GraphApi { get; set; } = new();
    public string? TenantId { get; set; }
    public string? TenantName { get; set; }
    public List<string>? UserFlows { get; set; }
    public string? RedirectUri { get; set; }
}

public class GraphApiSettings
{
    public string? Scopes { get; set; }
}
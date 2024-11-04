using Microsoft.Extensions.Caching.Memory;

public interface IVersionResolver
{
    VersionConfig GetVersionsConfig();
    (string version, string path) ResolveVersion(string appName, HttpContext context);
}

public class VersionResolver : IVersionResolver
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<VersionResolver> _logger;
    private readonly IMemoryCache _cache;

    public VersionResolver(IConfiguration configuration, ILogger<VersionResolver> logger, IMemoryCache cache)
    {
        _configuration = configuration;
        _logger = logger;
        _cache = cache;
    }

    public VersionConfig GetVersionsConfig()
    {
        return _configuration.Get<VersionConfig>() 
            ?? throw new InvalidOperationException("Version configuration is missing");
    }

    public (string version, string path) ResolveVersion(string appName, HttpContext context)
    {
        var config = GetVersionsConfig();
        
        if (!config.Apps.TryGetValue(appName, out var appConfig))
        {
            throw new KeyNotFoundException($"App {appName} not found in configuration");
        }

        // Check variants
        foreach (var variant in appConfig.Versions.Variants)
        {
            var criteria = variant.Value.Criteria;

            // Check query parameter
            if (!string.IsNullOrEmpty(criteria.QueryParam))
            {
                var parts = criteria.QueryParam.Split('=');
                if (parts.Length == 2 && 
                    context.Request.Query[parts[0]] == parts[1])
                {
                    return (variant.Value.Version, appConfig.Paths[variant.Value.Version]);
                }
            }

            // Check cookie
            if (!string.IsNullOrEmpty(criteria.Cookie))
            {
                var parts = criteria.Cookie.Split('=');
                if (parts.Length == 2 && 
                    context.Request.Cookies[parts[0]] == parts[1])
                {
                    return (variant.Value.Version, appConfig.Paths[variant.Value.Version]);
                }
            }

            // Check user groups (if implemented)
            if (criteria.UserGroups?.Length > 0 && IsUserInGroups(context, criteria.UserGroups))
            {
                return (variant.Value.Version, appConfig.Paths[variant.Value.Version]);
            }

            // Apply weighted distribution if no specific criteria matched
            if (ShouldApplyVariant(variant.Value.Weight))
            {
                return (variant.Value.Version, appConfig.Paths[variant.Value.Version]);
            }
        }

        // Default version
        return (appConfig.Versions.Default, appConfig.Paths[appConfig.Versions.Default]);
    }

    private bool IsUserInGroups(HttpContext context, string[] groups)
    {
        // Implement your user group checking logic here
        // This could involve checking JWT claims, calling an auth service, etc.
        return false;
    }

    private bool ShouldApplyVariant(int weight)
    {
        return Random.Shared.Next(100) < weight;
    }
}
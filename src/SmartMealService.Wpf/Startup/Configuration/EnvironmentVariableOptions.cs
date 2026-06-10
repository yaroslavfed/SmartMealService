using Microsoft.Extensions.Configuration;

namespace SmartMealService.Wpf.Startup.Configuration;

public sealed class EnvironmentVariableOptions
{
    public IReadOnlyList<string> Names { get; init; } = [];

    public IReadOnlyDictionary<string, string> Comments { get; init; } = new Dictionary<string, string>();

    public static EnvironmentVariableOptions FromConfiguration(IConfiguration configuration)
    {
        var names = configuration.GetSection("EnvironmentVariables:Names")
            .GetChildren()
            .Select(section => section.Value)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!.Trim())
            .ToArray();

        if (names.Length == 0)
            throw new InvalidOperationException("Параметр 'EnvironmentVariables:Names' не настроен.");

        var comments = configuration.GetSection("EnvironmentVariables:Comments")
            .GetChildren()
            .Where(section => !string.IsNullOrWhiteSpace(section.Key) && !string.IsNullOrWhiteSpace(section.Value))
            .ToDictionary(section => section.Key, section => section.Value!.Trim());

        return new EnvironmentVariableOptions
        {
            Names = names,
            Comments = comments
        };
    }
}

namespace MovieLibrary.Api.Data;

public class DatabaseOptions
{
    public bool InitializeSchemaOnStartup { get; init; } = true;

    public bool SeedOnStartup { get; init; } = false;

    public int MinimumSeedRecordCount { get; init; } = 10_000;
}

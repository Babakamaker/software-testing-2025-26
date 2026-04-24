namespace MovieLibrary.IntegrationTests;

[CollectionDefinition(nameof(PostgresCollection), DisableParallelization = true)]
public class PostgresCollection : ICollectionFixture<PostgresFixture>;

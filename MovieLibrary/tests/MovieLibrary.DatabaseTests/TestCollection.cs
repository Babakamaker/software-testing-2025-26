namespace MovieLibrary.DatabaseTests;

[CollectionDefinition(nameof(DatabaseCollection), DisableParallelization = true)]
public class DatabaseCollection : ICollectionFixture<PostgresDatabaseFixture>;

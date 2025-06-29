namespace PoultryFarmManager.Tests.Integration;

[CollectionDefinition(Name)]
public class InfrastructureContextCollection : ICollectionFixture<InfrastructureContextFixture>
{
    public const string Name = nameof(InfrastructureContextCollection);

    // This class is used to group the InfrastructureContextFixture for integration tests.
    // It allows us to share the same context across multiple test classes.
    // No additional code is needed here, as the fixture will be automatically used by the test framework.
}
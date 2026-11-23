namespace AspireRag.ArchitectureTests;

internal static class TestResultExtensions
{
    public static void ShouldBeSuccessful(this NetArchTest.Rules.TestResult result) =>
        result.IsSuccessful.ShouldBeTrue("Violating types: " + string.Join(", ", result.FailingTypeNames ?? []));
}

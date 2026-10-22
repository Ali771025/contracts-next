namespace Contracts.Application;

/// <summary>
/// Identifies the Application assembly for composition and architecture verification.
/// </summary>
public static class ApplicationAssembly
{
    public static Assembly Reference => typeof(ApplicationAssembly).Assembly;
}

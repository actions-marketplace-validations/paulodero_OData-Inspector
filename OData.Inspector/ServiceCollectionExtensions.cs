namespace OData.Inspector;

using Microsoft.Build.Locator;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDotNetCodeAnalysisServices(
        this IServiceCollection services)
    {
        MSBuildLocator.RegisterDefaults();

        return services;
    }
}

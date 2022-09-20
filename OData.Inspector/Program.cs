namespace OData.Inspector;
public class Program
{
    public static async Task Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>{})
            .Build();
        var parser = Default.ParseArguments<ActionInputs>(() => new(), args);
            
        // If the no parameters/invalid parameters are passed.
        parser.WithNotParsed(
            errors =>
            {
                Get<ILoggerFactory>(host)
                    .CreateLogger("OData.Inspector")
                    .LogError(
                        string.Join(Environment.NewLine, errors.Select(error => error.ToString())));

                Environment.Exit(2);
            });

        await parser.WithParsedAsync(options => StartSchemaAnalysisAsync(options, host));
        await host.RunAsync();
    }
    private static TService Get<TService>(IHost host)
            where TService : notnull =>
            host.Services.GetRequiredService<TService>();

    static async Task StartSchemaAnalysisAsync(ActionInputs inputs, IHost host)
    {
        await Task.Delay(1);

        Get<ILoggerFactory>(host)
                .CreateLogger("OData.Inspector")
        .LogError("An error has been detected in the flow.");

        Environment.Exit(-1);
    }
}
namespace OData.Inspector;
using OData.Schema.Validation.Utils;

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
                    .LogError(string.Join(Environment.NewLine, errors.Select(error => error.ToString())));

                Environment.Exit(2);
            });

        await parser.WithParsedAsync(options => StartSchemaAnalysisAsync(options, host));
        await host.RunAsync();
    }
    private static TService Get<TService>(IHost host)
            where TService : notnull =>
            host.Services.GetRequiredService<TService>();
    
    /// <summary>
    /// Schema analysis.
    /// </summary>
    /// <param name="inputs">Input parameters. An instance of <see cref="ActionInputs"/>.</param>
    /// <param name="host">Host.</param>
    /// <returns>async task.</returns>
    private static async Task StartSchemaAnalysisAsync(ActionInputs inputs, IHost host)
    {
        var sourceSchemas = await GitUtilities.GetSchemasFromBranch(inputs.Username, inputs.SourceBranch);
        var validator = new SchemaValidator(sourceSchemas);
        
        validator.RunValidation();

        var logger = Get<ILoggerFactory>(host).CreateLogger("OData.Inspector");
        foreach (var error in validator.validationErrors)
        {
            logger.LogError(error.ErrorMessage);
        }

        if (validator.validationErrors.Any())
        {
            Environment.Exit(-1);
        }
        else
        {
            Environment.Exit(0);
        }
    }
}
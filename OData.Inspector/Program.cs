using CommandLine;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
    })
    .Build();

static TService Get<TService>(IHost host)
    where TService : notnull =>
    host.Services.GetRequiredService<TService>();

static async Task StartAnalysisAsync(ActionInputs inputs, IHost host)
{
    var updatedMetrics = "100%";
    var title = "OData Inspector";
    var summary = "The summary";

    await Task.Delay(1);

    Get<ILoggerFactory>(host)
           .CreateLogger("OData.Inspector")
    .LogError("An error has been detected in the flow.");

    // https://docs.github.com/actions/reference/workflow-commands-for-github-actions#setting-an-output-parameter
    Console.WriteLine($"::set-output name=updated-metrics::{updatedMetrics}");
    Console.WriteLine($"::set-output name=summary-title::{title}");
    Console.WriteLine($"::set-output name=summary-details::{summary}");

    Environment.Exit(0);
}

var parser = Default.ParseArguments<ActionInputs>(() => new(), args);
parser.WithNotParsed(
    errors =>
    {
        Get<ILoggerFactory>(host)
            .CreateLogger("OData.Inspector")
            .LogError(
                string.Join(Environment.NewLine, errors.Select(error => error.ToString())));
        
        Environment.Exit(2);
    });

await parser.WithParsedAsync(options => StartAnalysisAsync(options, host));
await host.RunAsync();

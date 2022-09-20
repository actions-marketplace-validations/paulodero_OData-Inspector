namespace OData.Inspector;

public class ActionInputs
{
    public ActionInputs()
    {
        if (Environment.GetEnvironmentVariable(
            "GREETINGS") is { Length: > 0 } greetings)
        {
            Console.WriteLine(greetings);
        }
    }

    [Option('b', "branch",
        Required = true,
        HelpText = "The source branch.")]
    public string SourceBranch { get; set; } = null!;


    [Option('t', "targetBranch",
        Required = true,
        HelpText = "The target branch.")]
    public string TargetBranch { get; set; } = null!;
}

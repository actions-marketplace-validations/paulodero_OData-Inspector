namespace OData.Inspector;

public class ActionInputs
{
    [Option('b', "branch",
        Required = true,
        HelpText = "The source branch.")]
    public string SourceBranch { get; set; } = null!;


    [Option('t', "targetBranch",
        Required = true,
        HelpText = "The target branch.")]
    public string TargetBranch { get; set; } = null!;


    [Option('o', "repoOwner",
        Required = true,
        HelpText = "Repository owner.")]
    public string RepoOwner { get; set; } = null!;

    [Option('n', "repoName",
    Required = true,
    HelpText = "Name of the repository.")]
    public string RepoName { get; set; } = null!;
}

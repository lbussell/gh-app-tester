using System.CommandLine;
using GitHubAppTester;
using Octokit;
using static System.Console;


var rootCommand = new RootCommand("Inspects a GitHub repository authenticated as a GitHub App");

var pemKeyFileOption = new Option<string>("--pem-key-file", "The private key file for the app") { IsRequired = true };
var clientIdOption = new Option<string>("--client-id", "Client ID of the app") { IsRequired = true };

rootCommand.AddGlobalOption(pemKeyFileOption);
rootCommand.AddGlobalOption(clientIdOption);

var infoCommand = new Command("info", "Gets basic info about a GitHub App");
infoCommand.SetHandler(GetAppInfoAsync, pemKeyFileOption, clientIdOption);
rootCommand.Add(infoCommand);

var ownerOption = new Option<string>("--owner", "The owner of the repository") { IsRequired = true };
var repoOption = new Option<string>("--repo", "The name of the repository") { IsRequired = true };
var contentsCommand = new Command("contents", "Gets the contents of a repository") { ownerOption, repoOption };
contentsCommand.SetHandler(GetContentsAsync, pemKeyFileOption, clientIdOption, ownerOption, repoOption);
rootCommand.Add(contentsCommand);

WriteLine();
await rootCommand.InvokeAsync(args);


static async Task GetAppInfoAsync(string pemKeyFileValue, string clientIdValue)
{
    WriteLine($"Using private key: {pemKeyFileValue}");
    WriteLine($"Client ID: {clientIdValue}");

    var credentials = GitHubHelper.GetAppCredentials(
        clientId: clientIdValue,
        pemKeyFilePath: pemKeyFileValue);

    var appInfo = await GitHubHelper.GetBasicAppInfoAsync(credentials);

    WriteLine(appInfo);
}

static async Task GetContentsAsync(
    string pemKeyFileValue,
    string clientIdValue,
    string ownerValue,
    string repoValue)
{
    WriteLine($"Using private key: {pemKeyFileValue}");
    WriteLine($"Client ID: {clientIdValue}");

    var credentials = GitHubHelper.GetAppCredentials(
        clientId: clientIdValue,
        pemKeyFilePath: pemKeyFileValue);

    GitHubAppInfoResult appInfo = await GitHubHelper.GetBasicAppInfoAsync(credentials);
    Installation installation = appInfo.Installations[0];
    GitHubClient appInstallationClient = await GitHubHelper.GetAppInstallationClient(credentials, installation.Id);

    var contents = await appInstallationClient.Repository.Content.GetAllContents(ownerValue, repoValue, "/");

    WriteLine();
    WriteLine($"Contents of {ownerValue}/{repoValue}:");
    foreach (var content in contents)
    {
        WriteLine($"{content.Type,-5} {content.Size,6} {content.Path}");
    }
    WriteLine();
}

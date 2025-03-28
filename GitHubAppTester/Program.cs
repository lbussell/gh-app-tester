using System.CommandLine;
using GitHubAppTester;
using static System.Console;


var rootCommand = new RootCommand("Inspects a GitHub repository authenticated as a GitHub App");

var pemKeyFileOption = new Option<string>("--pem-key-file", "The private key file for the app") { IsRequired = true };
var clientIdOption = new Option<string>("--client-id", "Client ID of the app") { IsRequired = true };

var infoCommand = new Command("info", "Gets basic info about a GitHub App")
{
    pemKeyFileOption,
    clientIdOption
};

infoCommand.SetHandler(GetAppInfoAsync, pemKeyFileOption, clientIdOption);
rootCommand.Add(infoCommand);
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

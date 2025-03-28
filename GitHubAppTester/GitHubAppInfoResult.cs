using System.Text.Json;
using Octokit;

namespace GitHubAppTester;

public record GitHubAppInfoResult(
    GitHubApp CurrentApp,
    IReadOnlyList<Installation> Installations)
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };

    public override string ToString()
    {
        string appInfoText = JsonSerializer.Serialize(CurrentApp, s_jsonOptions);
        string installationsText = JsonSerializer.Serialize(Installations, s_jsonOptions);

        return $"""

        App: {CurrentApp.Name}
        ---

        App Info: {appInfoText}

        App Installations: {installationsText}

        """;
    }
};

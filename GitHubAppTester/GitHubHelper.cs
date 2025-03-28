using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json.Serialization.Metadata;
using Microsoft.IdentityModel.Tokens;
using Octokit;

namespace GitHubAppTester;

// Roughly based on https://github.com/dotnet/crank/blob/main/src/Microsoft.Crank.PullRequestBot/CredentialsHelper.cs.
// Octokit.Net documentation: https://octokitnet.readthedocs.io/en/latest/github-apps/
public class GitHubHelper
{
    private static readonly TimeSpan s_gitHubJwtTimeout = TimeSpan.FromMinutes(10);

    public static readonly ProductHeaderValue ClientHeader = new("GitHubAppTester");

    public static Credentials GetAppCredentials(string clientId, string pemKeyFilePath)
    {
        string token = CreateJwt(clientId, pemKeyFilePath);
        return new Credentials(token, AuthenticationType.Bearer);
    }

    public static async Task<GitHubAppInfoResult> GetBasicAppInfoAsync(Credentials credentials)
    {
        var client = CreateGitHubClient(credentials);

        // Get the current authenticated GitHub App
        var currentApp = await client.GitHubApps.GetCurrent();

        // Get a list of installations for the authenticated GitHub App
        var installations = await client.GitHubApps.GetAllInstallationsForCurrent();

        // Get a specific installation of the authenticated GitHubApp by it's installation Id
        // var installation = await client.GitHubApps.GetInstallationForCurrent();

        return new GitHubAppInfoResult(currentApp, installations);
    }

    private static string CreateJwt(string clientId, string pemKeyFilePath)
    {
        string keyText = File.ReadAllText(pemKeyFilePath);
        // Console.WriteLine("Key text: " + keyText);

        RsaSecurityKey rsaSecurityKey;
        using (var rsa = RSA.Create())
        {
            rsa.ImportFromPem(keyText);
            rsaSecurityKey = new RsaSecurityKey(rsa.ExportParameters(true));
        };

        var creds = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

        var jwt = new JwtSecurityToken(
            new JwtHeader(creds),
            new JwtPayload(
                issuer: clientId,
                issuedAt: DateTime.Now,
                expires: DateTime.Now.Add(s_gitHubJwtTimeout),
                audience: null,
                claims: null,
                notBefore: null));

        string token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return token;
    }

    private static GitHubClient CreateGitHubClient(Credentials credentials)
    {
        return new GitHubClient(ClientHeader)
        {
            Credentials = credentials
        };
    }
}

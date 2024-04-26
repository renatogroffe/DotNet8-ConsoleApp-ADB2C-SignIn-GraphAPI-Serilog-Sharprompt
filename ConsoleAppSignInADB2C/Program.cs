using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Serilog;
using ConsoleAppSignInADB2C.Inputs;
using ConsoleAppSignInADB2C.Security;

Console.WriteLine("***** Autenticando usuarios com Azure AD B2C *****");
Console.WriteLine();

var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("azureadb2c-logs.tmp")
    .CreateLogger();

var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile($"appsettings.json");
var configuration = builder.Build();

var adb2cSettings = new ADB2CSettings();
configuration.GetSection("ADB2C").Bind(adb2cSettings);

string clientId = adb2cSettings.ConsoleAppId!;
string tenantName = adb2cSettings.TenantName!;
string tenant = $"{tenantName}.onmicrosoft.com";
string azureAdB2CHostname = $"{tenantName}.b2clogin.com";
string redirectUri = adb2cSettings.RedirectUri!;
string authorityBase = $"https://{azureAdB2CHostname}/tfp/{tenant}/";

logger.Information("Configurando credencial de acesso ao Graph API...");
var tenantId = adb2cSettings.TenantId!;
var clientSecret = adb2cSettings.ConsoleAppSecret!;
var scopesGraphApi = new[] { adb2cSettings.GraphApi!.Scopes! };
var options = new ClientSecretCredentialOptions
{
    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
};
var clientSecretCredential = new ClientSecretCredential(
    tenantId, clientId, clientSecret, options);
using var graphClient = new GraphServiceClient(clientSecretCredential, scopesGraphApi);

var applicationData = await graphClient.ApplicationsWithAppId(clientId).GetAsync();
logger.Information("Certifique-se de que no manifest da aplicacao foram configurados os atributos");
logger.Information("   \"signInAudience\": \"AzureADandPersonalMicrosoftAccount\"");
logger.Information("   \"accessTokenAcceptedVersion\": 2");
logger.Information("***** Informacoes da App Registration *****");
logger.Information($"      {nameof(applicationData.Id)}: {applicationData!.Id}");
logger.Information($"      {nameof(applicationData.DisplayName)}: {applicationData.DisplayName}");
logger.Information($"      {nameof(applicationData.SignInAudience)}: {applicationData.SignInAudience}");

string continuar;
do
{
    string userFlow = InputHelper.GetUserFlow(adb2cSettings.UserFlows!);
    string authoritySignUpSignIn = $"{authorityBase}{userFlow}";

    logger.Information($"Exibindo a tela de login com {nameof(PublicClientApplicationBuilder)} | User Flow: {userFlow}");
    var app = PublicClientApplicationBuilder
        .Create(clientId)
        .WithB2CAuthority(authoritySignUpSignIn)
        .WithRedirectUri(redirectUri)
        .Build(); 
    string[] scopes = { "openid offline_access" };
    var result = await app.AcquireTokenInteractive(scopes).ExecuteAsync();

    if (result.IdToken is not null)
    {
        logger.Information($"Id Token: {result.IdToken}");
        logger.Information($"*** Algumas Claims do Usuario ***");
        var infoIdToken = TokenHelper.ParseIdToken(result.IdToken);
        logger.Information($"Nome = {infoIdToken["given_name"]}");
        logger.Information($"Sobrenome = {infoIdToken["family_name"]}");
    }
    else
    {
        logger.Error($"O Id Token nao foi gerado!");
    }

    continuar = InputHelper.GetAnswerContinue();
} while (continuar == "Sim");
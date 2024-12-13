using HitachiQA.Helpers;
using Microsoft.Extensions.Configuration;
namespace HitachiQA.Source.HttpClients.Authorization;

public abstract class InteractiveAuthBase
{
    protected IConfiguration Config { get; init; }
    public InteractiveAuthBase(IConfiguration config)
    {
        Config = config;
    }

    public abstract bool IsBrowserRunning { get;}

    public abstract Task InvokeBrowserAsync();
    public abstract Task AttemptAutoSigninAsync(string? emailIdentifierKey);
    public abstract Task<BrowserCredential> GetAccessTokenCredsAsync(string identifierKey);
    public abstract Task NavigateToHostIfNeededAsync();

    public async Task<Credentials> AuthenticateUsingBrowserAsync()
    {
        var keyIdentifier = Config.GetVariable("AUTH_INTERACTIVE_KEY_IDENTIFIER");
        if (string.IsNullOrWhiteSpace(keyIdentifier))
        {
            throw new KeyNotFoundException("AUTH_INTERACTIVE_KEY_IDENTIFIER for interactive auth is not found in the configuration");
        }
        var disposeDriverAfterTokenAcquisition = false;
        //if no driver has been ever invoked, then we invoke and attempt to auto sign in
        if (!IsBrowserRunning)
        {  
            disposeDriverAfterTokenAcquisition = true;
            await InvokeBrowserAsync();
            await NavigateToHostIfNeededAsync();
            var emailIdentifier = Config.GetVariable("AUTH_INTERACTIVE_EMAIL_IDENTIFIER", true);
            if (string.IsNullOrWhiteSpace(emailIdentifier))
                await AttemptAutoSigninAsync(emailIdentifier);
        }
        //else
        //if a driver has already been invoked (we'll assume sign must've happened)


        //the below waits for 2 minutes for a token to exist (from auto or manual sign in)
        var accessCreds = await GetAccessTokenCredsAsync(keyIdentifier);
        accessCreds.Secret.NullGuard("localStorage's secret");
        var expTimestamp = long.Parse(accessCreds.ExpiresOn);

        if (disposeDriverAfterTokenAcquisition)
        {
            await DisposeAsync();
        }

        return new Credentials()
        {
            AccessToken = accessCreds.Secret,
            ExpiryDateTime = (expTimestamp - 30).UnixTimeStampToDateTime(),
            ClientId = accessCreds.ClientId,
            Target = accessCreds.Target,
        };
    }

    public abstract Task DisposeAsync();
}

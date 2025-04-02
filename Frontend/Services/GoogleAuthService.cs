using Microsoft.Extensions.Configuration;

namespace Frontend.Services;

public class GoogleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly PkceHelper _pkceHelper;
    private readonly string _clientId;
    private readonly string _redirectUri;

    public GoogleAuthService(
        IConfiguration configuration,
        PkceHelper pkceHelper)
    {
        _configuration = configuration;
        _pkceHelper = pkceHelper;
        _clientId = _configuration["GoogleAuth:ClientId"]!;
        _redirectUri = _configuration["GoogleAuth:RedirectUri"]!;
    }

    public async Task<string> GetAuthorizationUrlAsync()
    {
        var (_, codeChallenge) = await _pkceHelper.GeneratePkcePairAsync();
        
        var scope = "openid email profile";
        return $"https://accounts.google.com/o/oauth2/v2/auth?" +
               $"client_id={_clientId}&" +
               $"redirect_uri={Uri.EscapeDataString(_redirectUri)}&" +
               $"response_type=code&" +
               $"scope={Uri.EscapeDataString(scope)}&" +
               $"code_challenge={codeChallenge}&" +
               $"code_challenge_method=S256";
    }
} 
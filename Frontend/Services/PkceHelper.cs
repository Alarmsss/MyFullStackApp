using System.Security.Cryptography;
using System.Text;
using Microsoft.JSInterop;

namespace Frontend.Services;

public class PkceHelper
{
    private readonly IJSRuntime _jsRuntime;
    private const string CODE_VERIFIER_KEY = "pkce_code_verifier";

    public PkceHelper(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<(string codeVerifier, string codeChallenge)> GeneratePkcePairAsync()
    {
        // 生成 code_verifier (43-128 字元的隨機字串)
        var codeVerifier = GenerateCodeVerifier();
        
        // 計算 code_challenge
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        
        // 儲存 code_verifier 到 sessionStorage
        await StoreCodeVerifierAsync(codeVerifier);
        
        return (codeVerifier, codeChallenge);
    }

    public async Task<string> RetrieveCodeVerifierAsync()
    {
        return await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", CODE_VERIFIER_KEY);
    }

    private async Task StoreCodeVerifierAsync(string codeVerifier)
    {
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", CODE_VERIFIER_KEY, codeVerifier);
    }

    public async Task ClearCodeVerifierAsync()
    {
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", CODE_VERIFIER_KEY);
    }

    private string GenerateCodeVerifier()
    {
        var bytes = new byte[32]; // 256 bits
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        
        return Convert.ToBase64String(challengeBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
} 
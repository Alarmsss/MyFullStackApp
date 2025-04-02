using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MyFullStackApp.Backend.Models;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly TodoDbContext _context;
    private readonly JwtService _jwtService;
    private readonly IConfiguration _config;

    //IConfiguration是自動註冊的DI 不用手動註冊也能用
    public AuthController(TodoDbContext context, IConfiguration config, JwtService jwtService)
    {
        _context = context;
        _config = config;
        _jwtService = jwtService;
    }
    //傳入Code回傳JWT token
    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        try
        {
            var clientId = _config["Google:ClientId"];
            var clientSecret = _config["Google:ClientSecret"];
            var redirectUri = _config["Google:RedirectUri"]; // 通常為前端提供的 redirect URI

            using var httpClient = new HttpClient();
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token");
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "code", request.Code },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" }
            });

            var tokenResponse = await httpClient.SendAsync(tokenRequest);
            if (!tokenResponse.IsSuccessStatusCode)
            {
                return Unauthorized("Failed to exchange code for token");
            }

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(tokenJson);
            var idToken = doc.RootElement.GetProperty("id_token").GetString();

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
            if (payload == null)
            {
                return Unauthorized("Invalid Google Token");
            }

            // 從 Google 取得唯一的使用者 ID（sub）
            var googleUserId = payload.Subject;
            var email = payload.Email;
            var name = payload.Name;

            // 查詢資料庫是否已經有此使用者
            var user = await _context.Users.FindAsync(googleUserId);
            if (user == null)
            {
                user = new User
                {
                    Id = googleUserId, // 使用 Google 提供的 ID
                    Email = email,
                    Name = name
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            var token = _jwtService.GenerateToken(user);
            return Ok(token);

        }
        catch (InvalidJwtException)
        {
            return Unauthorized("Invalid Google Token");
        }
    }
}

// 用來接收前端傳來的 JSON 請求
public class GoogleLoginRequest
{
    public required string Code { get; set; }
}
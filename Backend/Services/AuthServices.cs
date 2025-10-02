using AuthApp.Config;
using AuthApp.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AuthApp.Services;

public class AuthServices
{
    private readonly IMongoCollection<Signup> _signup;
    private readonly IMongoCollection<RefreshToken> _refreshTokens;
    private readonly IConfiguration _config;

    public AuthServices(IOptions<AuthDataBaseSettings> settings, IConfiguration config)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);

        _signup = database.GetCollection<Signup>(settings.Value.AuthCollection);
        _refreshTokens = database.GetCollection<RefreshToken>(settings.Value.RefreshTokenCollection);

        _config = config;
    }


    // Add new user
    public async Task AddUser(Signup user) => await _signup.InsertOneAsync(user);

    // Get user by email
    public async Task<Signup?> GetByEmailAsync(string email) =>
        await _signup.Find(u => u.email == email).FirstOrDefaultAsync();

    // Get user by Id
    public async Task<Signup?> GetByIdAsync(string id) =>
        await _signup.Find(u => u.Id == id).FirstOrDefaultAsync();

    // Generate JWT access token
    public string GenerateToken(Signup user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, user.email),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.username),
            new Claim("role_id", ((int)user.role).ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiryInMinutes"]!)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Generate cryptographically secure refresh token
    public async Task<RefreshToken> GenerateRefreshToken(string userId)
    {
        byte[] randomBytes = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        var token = Convert.ToBase64String(randomBytes);

        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = userId,
            Expires = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        await _refreshTokens.InsertOneAsync(refreshToken);
        return refreshToken;
    }

    // Validate old refresh token and issue new JWT + refresh token
    public async Task<(string newJwt, RefreshToken newRefreshToken)> RefreshJwtTokenAsync(string oldRefreshToken)
    {
        var stored = await _refreshTokens.Find(t => t.Token == oldRefreshToken).FirstOrDefaultAsync();

        if (stored == null || stored.IsUsed || stored.IsRevoked || stored.Expires < DateTime.UtcNow)
            throw new Exception("Invalid refresh token");

        // Mark old refresh token as used
        stored.IsUsed = true;
        await _refreshTokens.ReplaceOneAsync(t => t.Token == oldRefreshToken, stored);

        // Get user
        var user = await GetByIdAsync(stored.UserId);
        if (user == null) throw new Exception("User not found");

        var newJwt = GenerateToken(user);
        var newRefreshToken = await GenerateRefreshToken(user.Id);

        return (newJwt, newRefreshToken);
    }
}

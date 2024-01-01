using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;

namespace ModularMonolith.CategoryManagement.Api.IntegrationTests.Fixtures;

public class CategoryManagementFixture : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = default!;
    
    public string AuthAudience => "localhost";
    
    public string AuthIssuer => "localhost";

    public string AuthSigningKey => "12345678123456781234567812345678";
    
    public Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
    }

    public HttpClient CreateClient(string connectionString, Action<IWebHostBuilder>? action = null)
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Database", connectionString);
            builder.UseSetting("Modules:CategoryManagement:Enabled", "true");
            builder.UseSetting("Authentication:Type", "Bearer");
            builder.UseSetting("Authentication:Audience", AuthAudience);
            builder.UseSetting("Authentication:Issuer", AuthIssuer);
            builder.UseSetting("Authentication:SigningKey", AuthSigningKey);

            action?.Invoke(builder);
        }).CreateClient();
        
        client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;v=1.0;q=1.0"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateToken());
        
        return client;
    }

    private string GenerateToken()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthSigningKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim("userId", Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, "Test-User"),
        };

        var expirationTime = DateTimeOffset.UtcNow.AddMinutes(15);

        var token = new JwtSecurityToken(AuthIssuer,
            AuthAudience,
            claims,
            expires: expirationTime.UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);        
    }
}

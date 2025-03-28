using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

#pragma warning disable ConstructorNotDi
#pragma warning disable CA1822
#pragma warning disable ClassMethodMissingInterface

namespace vc.Ifx.Web;

public class JwtHandler(string secretKey, string issuer, string audience)
{

    // Generate a JWT
    public string GenerateToken(Dictionary<string, string> claims, TimeSpan expiresIn)
    {

        try
        {
            var key = Encoding.UTF8.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(CreateClaims(claims)),
                Expires = DateTime.UtcNow.Add(expiresIn),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return string.Empty;
        }

    }

    // Validate a JWT
    public ClaimsPrincipal? ValidateToken(string token)
    {

        try
        {
            var key = Encoding.UTF8.GetBytes(secretKey);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // Adjust if necessary
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            return null; // Token is invalid
        }

    }

    // Decode a JWT without validation
    public IDictionary<string, object>? DecodeToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
        {
            return null; // Invalid token format
        }
        var jwtToken = handler.ReadJwtToken(token);
        return new Dictionary<string, object>
        {
            { "Header", jwtToken.Header },
            { "Payload", jwtToken.Payload }
        };
    }

    private static IEnumerable<Claim> CreateClaims(Dictionary<string, string> claims) => claims.Select(claim => new Claim(claim.Key, claim.Value));
}

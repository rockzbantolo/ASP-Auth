using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Base.Api.Helpers
{
    public class JwtHelper
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly double _expiryMinutes;

        public JwtHelper(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            var secret = _config["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException("JWT key is missing in configuration.");

            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            _issuer = _config["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT issuer missing");
            _audience = _config["Jwt:Audience"] ?? throw new InvalidOperationException("JWT audience missing");

            if (!double.TryParse(_config["Jwt:ExpireMinutes"], out _expiryMinutes))
                _expiryMinutes = 60; // default 1 hour
        }

        /// <summary>
        /// Generate JWT token with optional role and permissions
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="username">Username</param>
        /// <param name="roles">List of role names (optional)</param>
        /// <param name="permissions">List of permission names (optional)</param>
        /// <returns>JWT token string</returns>
        public string GenerateToken(
            int userId,
            string username,
            IEnumerable<string>? roles = null,
            IEnumerable<string>? permissions = null)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, username),
                new(ClaimTypes.NameIdentifier, userId.ToString())
            };

            // Add roles as claims
            if (roles != null)
            {
                foreach (var role in roles)
                {
                    if (!string.IsNullOrWhiteSpace(role))
                        claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            // Add permissions as claims
            if (permissions != null)
            {
                foreach (var perm in permissions)
                {
                    if (!string.IsNullOrWhiteSpace(perm))
                        claims.Add(new Claim("permission", perm));
                }
            }

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Optional: validate a token manually and return claims principal
        /// </summary>
        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ClockSkew = TimeSpan.Zero,
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };

            var principal = tokenHandler.ValidateToken(token, parameters, out _);
            return principal;
        }
    }
}

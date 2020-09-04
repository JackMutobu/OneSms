using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OneSms.Constants;
using OneSms.Data;
using OneSms.Domain;
using OneSms.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OneSms.Services
{
    public interface IAthenticationService
    {
        AuthenticationResult Authenticate(Guid appId, Guid secret);
        AuthenticationResult Authenticate(Guid serverKey, string secret);
        Task<AuthenticationResult> Authenticate(string userName, string password);
    }

    public class AthenticationService : IAthenticationService
    {
        private readonly DataContext _dbContext;
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AthenticationService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, DataContext dbContext, JwtSettings jwtSettings)
        {
            _dbContext = dbContext;
            _jwtSettings = jwtSettings;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public AuthenticationResult Authenticate(Guid appId, Guid secret)
        {
            var app = _dbContext.Apps.SingleOrDefault(app => app.Id == appId);

            if (app?.Secret == secret)
                return GenerateAuthenticationResult(app);

            return new AuthenticationResult
            {
                Errors = new List<string>
                    {
                        "Authentication failed, check your credentials"
                    }
            };
        }

        public AuthenticationResult Authenticate(Guid serverKey, string secret)
        {
            var app = _dbContext.MobileServers.SingleOrDefault(app => app.Id == serverKey);

            if (app?.Secret == secret)
                return GenerateAuthenticationResult(app);

            return new AuthenticationResult
            {
                Errors = new List<string>
                    {
                        "Authentication failed, check your credentials"
                    }
            };
        }

        public async Task<AuthenticationResult> Authenticate(string userName, string password)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if(user != null)
            {
                var passwordVerificationResult = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
                if(passwordVerificationResult == PasswordVerificationResult.Success)
                    return await GenerateAuthenticationResult(user);
            }

            return new AuthenticationResult
            {
                Errors = new List<string>
                    {
                        "Authentication failed, check your credentials"
                    }
            };
        }

        private AuthenticationResult GenerateAuthenticationResult(Application app)
        {

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, app.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("appId", app.Id.ToString()),
                new Claim(ClaimTypes.Role,Roles.ApiUser),
                new Claim("userId",app.UserId),
            };

            return GenerateToken(claims);
        }

        private AuthenticationResult GenerateAuthenticationResult(MobileServer server)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, server.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("mobileServerId", server.Id.ToString()),
                new Claim(ClaimTypes.Role,Roles.MobileServer)
            };

            return GenerateToken(claims);
        }

        private AuthenticationResult GenerateToken(List<Claim> claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(_jwtSettings.TokenLifetime),
                SigningCredentials =
                  new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthenticationResult
            {
                Success = true,
                Token = tokenHandler.WriteToken(token)
            };
        }

        private bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
        {
            return (validatedToken is JwtSecurityToken jwtSecurityToken) &&
                   jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                       StringComparison.InvariantCultureIgnoreCase);
        }

        public async Task<AuthenticationResult> GenerateAuthenticationResult(AppUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("id", user.Id),
                new Claim(ClaimTypes.Role,user.Role)
            };

            var userClaims = await _userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
                var role = await _roleManager.FindByNameAsync(userRole);
                if (role == null) continue;
                var roleClaims = await _roleManager.GetClaimsAsync(role);

                foreach (var roleClaim in roleClaims)
                {
                    if (claims.Contains(roleClaim))
                        continue;

                    claims.Add(roleClaim);
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(_jwtSettings.TokenLifetime),
                SigningCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthenticationResult
            {
                Success = true,
                Token = tokenHandler.WriteToken(token)
            };
        }
    }
}

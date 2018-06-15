using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using ETLCommon;

using Microsoft.IdentityModel.Tokens;

namespace ETLService.Manager
{
    public class JWTUser
    {
        public string Name;
        public string Role;
    }

    public class JWTControl
    {
        #region Поля

        private Database db;
        private SecurityKey key;

        #endregion Поля

        #region Основные функции

        public JWTUser IsValidUser(string userName, string password)
        {
            return new JWTUser { Name = "Admin", Role = "Admin" };
        }

        public JwtSecurityToken GenerateToken(string userName, string password)
        {
            if (key == null)
                return null;

            JWTUser user = IsValidUser(userName, password);
            if (user == null)
                return null;

            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Exp, $"{new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds()}"),
                new Claim(JwtRegisteredClaimNames.Nbf, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}")
            };

            return new JwtSecurityToken(new JwtHeader(new SigningCredentials(key, SecurityAlgorithms.HmacSha256)), new JwtPayload(claims));
        }

        public JWTControl(Database database, string secretKey)
        {
            if (database != null)
                db = database;

            // Секретный ключ шифрования
            key = string.IsNullOrEmpty(secretKey) || secretKey.Length < 16
                ? null 
                : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        }

        #endregion Основные функции
    }
}

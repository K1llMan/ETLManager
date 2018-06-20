using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace ETLService.Manager
{
    // Делегат проверки пользователя и 
    public delegate Claim[] CheckUser(Dictionary<string, string> userData);

    public class JwtControl
    {
        #region Поля

        private SecurityKey key;
        private CheckUser checkUser;

        #endregion Поля

        #region Основные функции

        public JwtSecurityToken GenerateToken(Dictionary<string, string> userData)
        {
            if (key == null || checkUser == null)
                return null;

            Claim[] userClaims = checkUser(userData);
            if (userClaims == null || userClaims.Length == 0)
                return null;

            Claim[] claims = {
                new Claim(JwtRegisteredClaimNames.Exp, $"{new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds()}"),
                new Claim(JwtRegisteredClaimNames.Nbf, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}")
            };

            return new JwtSecurityToken(new JwtHeader(new SigningCredentials(key, SecurityAlgorithms.HmacSha256)), new JwtPayload(claims.Union(userClaims)));
        }

        public JwtControl(CheckUser checkUserDelegate, string secretKey)
        {
            checkUser = checkUserDelegate;

            // Секретный ключ шифрования
            key = string.IsNullOrEmpty(secretKey) || secretKey.Length < 16
                ? null 
                : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        }

        #endregion Основные функции
    }
}

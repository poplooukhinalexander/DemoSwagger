using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Demo.WebApi.Model
{
    /// <summary>
    /// Опции для аутентификации.
    /// </summary>
    public class AuthOptions
    {
        /// <summary>
        /// Издатель.
        /// </summary>
        public const string ISSUER = "MyAuthServer";

        /// <summary>
        /// Потребитель токена.
        /// </summary>
        public const string AUDIENCE = "Demo Web API/";

        /// <summary>
        /// Соль для ключа шифрования.
        /// </summary>
        const string KEY = "mysupersecret_secretkey!123";

        /// <summary>
        /// Время жизни окена в минутах.
        /// </summary>
        public const int LIFETIME = 20;

        /// <summary>
        /// Возвращает симметричный ключ.
        /// </summary>
        /// <returns></returns>
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}

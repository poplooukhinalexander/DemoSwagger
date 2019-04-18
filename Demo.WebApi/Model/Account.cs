namespace Demo.WebApi.Model
{
    /// <summary>
    /// Учетная запись.
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Пароль.
        /// </summary>
        public string Password { get; set; }
    }
}

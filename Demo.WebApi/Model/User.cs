namespace Demo.WebApi.Model
{
    /// <summary>
    /// Пользователь системы.
    /// </summary>
    public class User : Account
    {        
        /// <summary>
        /// Роль.
        /// </summary>
        public string Role { get; set; }
    }
}

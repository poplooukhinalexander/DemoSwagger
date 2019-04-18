namespace Demo.WebApi.Model
{
    /// <summary>
    /// Пользователь системы.
    /// </summary>
    public class User : Account
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Роль.
        /// </summary>
        public string Role { get; set; }
    }
}

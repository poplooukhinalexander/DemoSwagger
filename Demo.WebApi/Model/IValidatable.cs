namespace Demo.WebApi.Model
{
    /// <summary>
    /// Интерфейс для валидируемых моделей.
    /// </summary>
    public interface IValidatable
    {
        /// <summary>
        /// Валидирует модель.
        /// </summary>
        void Validate();
    }
}

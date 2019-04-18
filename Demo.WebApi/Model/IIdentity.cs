namespace Demo.WebApi.Model
{
    /// <summary>
    /// Итнрфейс для идентифицируемых сущностей.
    /// </summary>
    public interface IIdentity
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        long Id { get; set; }
    }
}

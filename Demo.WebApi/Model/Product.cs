namespace Demo.WebApi.Model
{
    /// <summary>
    /// Модель для товара.
    /// </summary>
    public class Product : IIdentity
    {
        /// <summary>
        /// Идентификатор вендора.
        /// </summary>
        public long VendorId { get; set; }

        /// <summary>
        /// Идентификатор.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Стоимость.
        /// </summary>
        public decimal Price { get; set; }

    }
}

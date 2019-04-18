namespace Demo.WebApi.Model
{
    /// <summary>
    /// Модель для фото.
    /// </summary>
    public class Photo : IIdentity
    {
        /// <summary>
        /// Иденнтификатор.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Описание.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Соержимое как набор байт.
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// Тип картинки (расширение файла).
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Идентифкатор товара
        /// </summary>
        public long ProductId { get; set; }        
    }
}

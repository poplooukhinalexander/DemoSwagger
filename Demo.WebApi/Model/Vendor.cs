namespace Demo.WebApi.Model
{
    /// <summary>
    /// Модель для вендора.
    /// </summary>    
    public class Vendor : IIdentity
    {
        /// <summary>
        /// Идентифкатор.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Ссылка на лого.
        /// </summary>
        public string Logo { get; set; }        
    }
}

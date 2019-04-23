using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Demo.WebApi.Model
{
    /// <summary>
    /// Модель для вендора.
    /// </summary>    
    public class Vendor : IIdentity, IValidatable
    {
        /// <summary>
        /// Идентифкатор.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Наименование.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Описание.
        /// </summary>
        [MaxLength(200)]
        public string Description { get; set; }

        /// <summary>
        /// Ссылка на лого.
        /// </summary>
        public string Logo { get; set; }

        /// <summary>
        /// Тип партнерства.        
        /// </summary>
        public PartnerType Partner { get; set; } = PartnerType.Default;

        /// <summary>
        /// Валидирует модель.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new InvalidDataException($"'{nameof(Name)}' cannot be empty.");
            }
        }

        /// <summary>
        /// Тип партнерства.
        /// </summary>
        public enum PartnerType
        {
            /// <summary>
            /// Обычный партнер.
            /// </summary>
            Default,

            /// <summary>
            /// Серебряный патнер.
            /// </summary>
            Silver,

            /// <summary>
            /// Золотой партнер.
            /// </summary>
            Golden,

            /// <summary>
            /// Платиновый партнер.
            /// </summary>
            Platinum
        }
    }
}

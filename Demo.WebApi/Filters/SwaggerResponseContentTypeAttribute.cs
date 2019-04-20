using System;

namespace Demo.WebApi.Filters
{
    /// <summary>
    /// Атрибут, который задает ContentType.
    /// Изменяя "produces":
    /// ["application/json",
    /// "text/json"
    /// ]
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SwaggerResponseContentTypeAttribute : Attribute
    {
        /// <summary>
        /// Создает и инциализирует объек типа <see cref="SwaggerResponseContentTypeAttribute"/>.
        /// </summary>
        /// <param name="contentType"></param>
        public SwaggerResponseContentTypeAttribute(string contentType)
        {
            ContentType = contentType;
        }
        
        /// <summary>
        /// Тип содержимого в ответе.
        /// </summary>
        public string ContentType { get; private set; }

        /// <summary>
        /// True - только один тип содержимого, иначе может быть несколько.
        /// </summary>
        public bool Exclusive { get; set; }
    }
}

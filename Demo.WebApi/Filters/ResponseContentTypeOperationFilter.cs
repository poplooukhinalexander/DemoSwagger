using System.Linq;
using System.Reflection;

using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Demo.WebApi.Filters
{
    /// <summary>
    /// Фильтр для сваггера, который задает ContentType.
    /// Изменяя "produces":
    /// ["application/json",
    /// "text/json"
    /// ]
    /// </summary>
    public class ResponseContentTypeOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Применяем фильтр.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="context"></param>
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var requestAttributes = context.MethodInfo.GetCustomAttributes<SwaggerResponseContentTypeAttribute>()
                .FirstOrDefault();

            if (requestAttributes != null)
            {
                if (requestAttributes.Exclusive)
                    operation.Produces.Clear();

                operation.Produces.Add(requestAttributes.ContentType);
            }
        }
    }
}

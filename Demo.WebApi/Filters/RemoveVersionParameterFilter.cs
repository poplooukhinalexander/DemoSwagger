using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Demo.WebApi.Filters
{
    /// <summary>
    /// Фильтр для удаления параметра "version".
    /// </summary>
    public class RemoveVersionParameterFilter : IOperationFilter
    {
        /// <summary>
        /// Применение фильтра.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="context"></param>
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var versionParam = operation.Parameters.FirstOrDefault(p => p.Name == "version");
            if (versionParam != null)
                operation.Parameters.Remove(versionParam);
        }
    }
}

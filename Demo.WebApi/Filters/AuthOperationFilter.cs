using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;

using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Demo.WebApi.Filters
{
    /// <summary>
    /// Добавляем заголовок для аутентификации.
    /// </summary>
    public class AuthOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Применяет фильтр для аутентификации.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="context"></param>
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var authAttribute = context.MethodInfo.GetCustomAttributes<AuthorizeAttribute>()
                .FirstOrDefault();

            if (authAttribute != null)
            {               
                operation.Parameters = operation.Parameters ?? new List<IParameter>();
                var param = new BodyParameter
                {
                    Name = "Authorization",
                    In = "header",
                    Description = "access token",
                    Required = true,
                }; 
                param.Extensions.Add("default", "Bearer ");
                operation.Parameters.Add(param);               
            }
        }
    }
}

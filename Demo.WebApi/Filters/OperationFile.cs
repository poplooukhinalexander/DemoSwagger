using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Demo.WebApi.Filters
{
    /// <summary>
    /// Добавляет параметр для загрузки файла.
    /// </summary>
    public class OperationFile : IOperationFilter
    {
        /// <summary>
        /// Применяет фильтр.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="context"></param>
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.OperationId == "AddPhoto")
            {
                var oldParam = operation.Parameters.FirstOrDefault(p => p.Name == "file");
                operation.Parameters.Remove(oldParam);
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "file",
                    In = "formData",
                    Description = "Upload Image",
                    Required = true,
                    Type = "file"
                });
                operation.Consumes.Clear();
                operation.Consumes.Add("application/form-data");
            }
        }
    }
}

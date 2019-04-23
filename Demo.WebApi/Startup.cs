using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Demo.WebApi
{
    using Filters;  
    using Model;  

    /// <summary>
    /// Инициализация приложения.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Создаеи и инициализирует объект типа <see cref="Startup"/>.
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Настройка приложения.
        /// </summary>
        public IConfiguration Configuration { get; }

       /// <summary>
       /// Подключаем сервисы.
       /// </summary>
       /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CatalogContext>(opts => opts.UseInMemoryDatabase("MyCatalog"));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1.0", new Info { Title = "Swagger Demo Web API", Version = "v1.0" });
                c.SwaggerDoc("v2.0", new Info { Title = "Swagger Demo Web API", Version = "v2.0" });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.OperationFilter<ResponseContentTypeOperationFilter>();
                c.OperationFilter<AuthOperationFilter>();
                c.OperationFilter<RemoveVersionParameterFilter>();
                c.OperationFilter<OperationFile>();
                c.DocumentFilter<UseVersionValueFilter>();
                c.SchemaFilter<SchemaFilter>();                

                c.DescribeAllEnumsAsStrings();
                c.DescribeStringEnumsInCamelCase();

                c.DocInclusionPredicate((docName, desc) =>
                {
                    if (!desc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;

                    var versions = methodInfo.DeclaringType
                        .GetCustomAttributes(true)
                        .OfType<ApiVersionAttribute>()
                        .SelectMany(attr => attr.Versions);

                    var maps = methodInfo.GetCustomAttributes(true)
                        .OfType<MapToApiVersionAttribute>()
                        .SelectMany(map => map.Versions)
                        .ToArray();

                    var res = versions.Any(v => $"v{v.ToString()}" == docName) 
                        && (maps.Length == 0 || maps.Any(v => $"v{v.ToString()}" == docName));
                    return res;
                });
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opts =>
                {
                    opts.RequireHttpsMetadata = false;
                    opts.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = AuthOptions.ISSUER,

                        ValidateAudience = true,
                        ValidAudience = AuthOptions.AUDIENCE,
                        ValidateLifetime = true,

                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                        ValidateIssuerSigningKey = true
                    };
                });

            services.AddApiVersioning(opts =>
            {
                opts.AssumeDefaultVersionWhenUnspecified = true;
                opts.DefaultApiVersion = new ApiVersion(1, 0);
            });

            services.AddMvc().AddJsonOptions(opts => opts.SerializerSettings.Converters.Add(new StringEnumConverter()));
        }

        /// <summary>
        /// Подключаем middlewares для обработки http-запроса.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            

            //app.Use(async (ctx, next) =>
            //{
            //    if (ctx.Request.HttpContext.Request.Path.ToString().Contains("/swagger/") &&
            //        ctx.Request.Host.Host == "localhost")               
            //        await next();                    
            //    else                    
            //        ctx.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;           
            //});

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "v1.0");
                c.SwaggerEndpoint("/swagger/v2.0/swagger.json", "v2.0");                                   
            });

            app.UseAuthentication();

            app.UseExceptionHandler(c => c.Run(async ctx =>
            {
                var exceptionHandlerPathFeature = ctx.Features.Get<IExceptionHandlerPathFeature>();
                var ex = exceptionHandlerPathFeature.Error;

                var result = JsonConvert.SerializeObject(new { error = ex.Message });
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsync(result);
            }));

            app.UseMvc();
        }
    }
}

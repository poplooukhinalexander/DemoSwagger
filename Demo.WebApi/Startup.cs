﻿using System;
using System.IO;
using System.Reflection;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

using Swashbuckle.AspNetCore.Swagger;

namespace Demo.WebApi
{
    using Filters;
    using Model;    

    /// <summary>
    /// Точка входа в приложение.
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
                c.SwaggerDoc("v1", new Info { Title = "Swagger Demo Web API", Version = "v1" });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.OperationFilter<ResponseContentTypeOperationFilter>();
                c.OperationFilter<AuthOperationFilter>();
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

            services.AddMvc();
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
        
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");                
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

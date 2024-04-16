using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;


using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PractihubAPI.Persistance.Authenticate;
using PractihubAPI.Persistance.Opinion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace PractihubAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IOpinionHandler, OpinionHandler>();
            services.AddTransient<IUserHandler, UserHandler>();

            //Configuración del cors
            services.AddCors(options =>
            {
                var angularProjectUrl = Configuration.GetValue<string>("AngularProjectUrl");

                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(angularProjectUrl)
                        .WithMethods("GET", "POST")
                        .AllowAnyHeader()
                        .WithExposedHeaders("CountTotal");
                });
            });

            //Configuración de autenticación
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("veryveryveryveryveryveryverysecret.....")),
                ClockSkew = TimeSpan.Zero
            });


            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            //Middleware para servir archivos estaticos
            app.UseStaticFiles();

            app.UseCors();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

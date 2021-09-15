using FastTunnel.Core;
using FastTunnel.Core.Config;
using FastTunnel.Core.Extensions;
using FastTunnel.Core.Handlers.Server;
using FastTunnel.Server.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SuiDao.Server.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuiDao.Server
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
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {
                   var serverOptions = Configuration.GetSection("FastTunnel").Get<DefaultServerConfig>();

                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = false,
                       ValidateAudience = false,
                       ValidateLifetime = true,
                       ClockSkew = TimeSpan.FromSeconds(serverOptions.Api.JWT.ClockSkew),
                       ValidateIssuerSigningKey = true,
                       ValidAudience = serverOptions.Api.JWT.ValidAudience,
                       ValidIssuer = serverOptions.Api.JWT.ValidIssuer,
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(serverOptions.Api.JWT.IssuerSigningKey))
                   };

                   options.Events = new JwtBearerEvents
                   {
                       OnChallenge = async context =>
                       {
                           context.HandleResponse();

                           context.Response.ContentType = "application/json;charset=utf-8";
                           context.Response.StatusCode = StatusCodes.Status200OK;

                           await context.Response.WriteAsync(new ApiResponse
                           {
                               errorCode = ErrorCodeEnum.AuthError,
                               errorMessage = context.Error ?? "Token is Required"
                           }.ToJson());
                       },
                   };
               });

            services.AddAuthorization();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "SuiDao.Api", Version = "v2" });
            });

            // -------------------FastTunnel START------------------
            services.AddFastTunnelServer(Configuration.GetSection("FastTunnel"));
            // -------------------FastTunnel END--------------------

            services.AddSingleton<ILoginHandler, SuiDaoLoginHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v2/swagger.json", "SuiDao.Api"));
            }

            // -------------------FastTunnel START------------------
            app.UseFastTunnelServer();
            // -------------------FastTunnel END--------------------

            app.UseRouting();

            // --------------------- Custom UI ----------------
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();
            // --------------------- Custom UI ----------------

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapReverseProxy();
                endpoints.MapControllers();
            });
        }
    }
}

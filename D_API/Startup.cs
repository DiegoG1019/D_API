using AspNetCoreRateLimit;
using D_API.Authentication;
using DiegoG.Utilities.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D_API.Types;

namespace D_API
{
    public static partial class Program
    {
        public class Startup
        {
            public static IAuth Auth { get; private set; }
            
            public Startup(IConfiguration configuration)
            {
                Configuration = configuration;
            }

            public IConfiguration Configuration { get; }

            // This method gets called by the runtime. Use this method to add services to the container.
            public void ConfigureServices(IServiceCollection services)
            {
                string key;
                {
                    var args = Environment.GetCommandLineArgs();
                    if (args.Length is > 1)
                        key = args[1];
                    else
                    {
                        Log.Error("Could not obtain a security key from command line arguments. Using default key instead.");
                        key = "qgBVG:Qv7W?ns7Rf_eRdA2J,~NayIrYKr?X%PX@YcOi!IgHV@5Ln:jeGYVb1Smc+";
                    }
                }

                services.AddOptions();
                services.AddMemoryCache();
                services.Configure<ClientRateLimitOptions>(Configuration.GetSection("ClientRateLimiting"));
                services.Configure<ClientRateLimitOptions>(Configuration.GetSection("ClientRateLimitPolicies"));

                services.AddInMemoryRateLimiting();

                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

                services.AddControllers();
                services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(x =>
                {
#if DEBUG
                    x.RequireHttpsMetadata = false;
#endif
                    x.Audience = Settings<APISettings>.Current.Security.Audience;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = false,
                        ValidateAudience = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key))
                    };
                });

                Auth = new JwtAuth(key);
                services.AddSingleton<IAuth>(Auth);
                services.AddSingleton<IAppDataAccessKeeper>(new AppDataAccessFileKeeper("main"));
                services.AddControllers();
                services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "D_API", Version = "v1" }));
            }

            // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    app.UseSwagger();
                    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "D_API v1"));
                }

                app.UseClientRateLimiting();

                app.UseHttpsRedirection();

                app.UseRouting();

                app.UseAuthentication();

                app.UseAuthorization();

                app.UseEndpoints(endpoints => endpoints.MapControllers());

                startwatch.Stop();
                StartupTime = startwatch.Elapsed;
                stopwatch.Start();
            }
        }
    }
}

using AspNetCoreRateLimit;
using D_API.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using D_API.Types;
using Microsoft.AspNetCore.HttpOverrides;
using D_API.Interfaces;

namespace D_API;

public static partial class Program
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string key = GetKey();

            services.AddOptions();
            services.AddMemoryCache();
            services.Configure<ClientRateLimitOptions>(Configuration.GetSection("ClientRateLimiting"));
            services.Configure<ClientRateLimitOptions>(Configuration.GetSection("ClientRateLimitPolicies"));

            services.AddInMemoryRateLimiting();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            //services.Configure<ForwardedHeadersOptions>(options =>
            //{
            //    options.KnownProxies.Add(IPAddress.Parse("10.0.0.100"));
            //}); //Not necessary as of now

            var jwtIssuer = Settings<APISettings>.Current.Security.Issuer;
            var jwtAudience = Settings<APISettings>.Current.Security.Audience;
            var jwtSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));

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
                x.Audience = jwtAudience;
                x.ClaimsIssuer = jwtIssuer;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = jwtIssuer is not null,
                    ValidateAudience = jwtAudience is not null,
                    RequireExpirationTime = true,
                    RequireAudience = jwtAudience is not null,
                    RequireSignedTokens = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = jwtSigningKey,
                };
            });

            services.AddSingleton<IJwtProvider>(new JwtAuth(jwtSigningKey, jwtAudience, jwtIssuer));
            services.AddSingleton<IAppDataAccessKeeper>(new AppDataAccessFileKeeper("main"));
            {
                var (hashkey, enkey, eniv) = GetMCVPData();
                services.AddSingleton<IAuthCredentialsVerificationProvider>(new MemoryCredentialsVerificationProvider(hashkey, enkey, eniv));
            }
            services.AddControllers();
            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "D_API", Version = "v1" }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

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

        private static (string, string?, string?) GetMCVPData()
        {
            return (GetHashKey(), GetEncryptionKey(), GetEncryptionIV());

            static string? GetEncryptionIV()
            {
                if (Settings<APISettings>.Current.EncryptionIV is not null)
                {
                    Log.Information($"Obtained Encryption IV from configurations file");
                    return Settings<APISettings>.Current.EncryptionIV;
                }

                string[] args = Environment.GetCommandLineArgs();
                if (args.Length is >= 4)
                {
                    Log.Information($"Obtained Encryption IV from Command Line Arguments file");
                    return args[3];
                }

                string? envkey;
                if ((envkey = Environment.GetEnvironmentVariable("EncryptionIV")) is not null)
                {
                    Log.Information("Obtained Encryption IV from an environment variable");
                    return envkey;
                }

                Log.Error("Could not obtain a Encryption IV from configurations file, command line arguments or an environment varible. No IV available.");
                return null;
            }

            static string? GetEncryptionKey()
            {
                if (Settings<APISettings>.Current.EncryptionKey is not null)
                {
                    Log.Information($"Obtained Encryption Key from configurations file");
                    return Settings<APISettings>.Current.EncryptionKey;
                }

                string[] args = Environment.GetCommandLineArgs();
                if (args.Length is >= 3)
                {
                    Log.Information($"Obtained Encryption Key from Command Line Arguments file");
                    return args[2];
                }

                string? envkey;
                if ((envkey = Environment.GetEnvironmentVariable("EncryptionKey")) is not null)
                {
                    Log.Information("Obtained Encryption Key from an environment variable");
                    return envkey;
                }

                Log.Error("Could not obtain a Encryption Key from configurations file, command line arguments or an environment varible. No Encryption Key available");
                return null;
            }

            static string GetHashKey()
            {
                if (Settings<APISettings>.Current.ClientSecretHashKey is not null)
                {
                    Log.Information($"Obtained Client Secret HashKey from configurations file");
                    return Settings<APISettings>.Current.ClientSecretHashKey;
                }

                string[] args = Environment.GetCommandLineArgs();
                if(args.Length is >= 2)
                {
                    Log.Information($"Obtained Client Secret HashKey from Command Line Arguments file");
                    return args[1];
                }

                string? envkey;
                if ((envkey = Environment.GetEnvironmentVariable("SecretHashKey")) is not null)
                {
                    Log.Information("Obtained Client Secret HashKey from an environment variable");
                    return envkey;
                }

                Log.Error("Could not obtain a Client Secret HashKey from configurations file, command line arguments or an environment varible. Using default key instead.");
                return "&fCd>2Cpxz=@SK>^sQkt5zV0aE]8IKNsqazFOPb:m-RBq0VsBSN?Ebn&^aiO6wE@";
            }
        }

        private static string GetKey()
        {
            if (Settings<APISettings>.Current.JWTSecurityKey is not null)
            {
                Log.Information("Obtained JWT Security key from configurations file");
                return Settings<APISettings>.Current.JWTSecurityKey;
            }

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length is >= 1)
            {
                Log.Information("Obtained JWT Security key from Command Line arguments");
                return args[0];
            }

            string? envkey;
            if ((envkey = Environment.GetEnvironmentVariable("JsonWebTokenKey")) is not null)
            {
                Log.Information("Obtained JWT Security key from an environment variable");
                return envkey;
            } 
                
            Log.Error("Could not obtain a JWT Security key from configurations file, command line arguments or an environment varible. Using default key instead.");
            return "qgBVG:Qv7W?ns7Rf_eRdA2J,~NayIrYKr?X%PX@YcOi!IgHV@5Ln:jeGYVb1Smc+";
        }
    }
}

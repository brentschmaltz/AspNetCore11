using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace FederatedUsers
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            builder.AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Logging
            services.AddLogging();

            // Add framework services.
            services.AddMvc();

            services.AddAuthentication(
                SharedOptions => SharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            var encryptionKey = new JsonWebKey
            {
                K = "WIVds2iwJPwNhgUgwZXmn/46Ql1EkiL+M+QqDRdQURE=",
                KeyId = "EncryptionKey",
                Kty = JsonWebAlgorithmsKeyTypes.Octet
            };

            var signingKey = new JsonWebKey
            {
                K = "fpdOQGL9hFCg2d3cLNDnPK9qbHA25zYPcfLpk3gKFzU=",
                KeyId = "SigningKey",
                Kty = JsonWebAlgorithmsKeyTypes.Octet
            };

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                ClientId = "26498a7b-a9b4-47e6-a783-ef968979eeba",
                MetadataAddress = "http://localhost:8081/.well-known/openid-configuration",
                RequireHttpsMetadata = false,
                ResponseType = OpenIdConnectResponseType.IdToken,
                TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = signingKey,
                    TokenDecryptionKey = encryptionKey
                },
                Events = new OpenIdConnectEvents()
                {
                    OnRedirectToIdentityProvider = context =>
                    {
                        var encodedEncryptionKey = Base64UrlEncoder.Encode(JsonConvert.SerializeObject(encryptionKey));
                        var encodedSigningKey = Base64UrlEncoder.Encode(JsonConvert.SerializeObject(signingKey));
                        var json = $"{{\"encryptionkey\":\"{encodedEncryptionKey}\", \"signingkey\":\"{encodedSigningKey}\"}}";
                        context.ProtocolMessage.SetParameter("tParam", Base64UrlEncoder.Encode(json));
                        return Task.FromResult(0);
                    },
                    OnMessageReceived = context =>
                    {
                        return Task.FromResult(0);
                    },
                    OnTokenValidated = context =>
                    {
                        return Task.FromResult(0);
                    }
                }
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

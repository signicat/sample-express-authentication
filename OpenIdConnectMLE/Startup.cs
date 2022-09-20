using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace SampleApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(opts =>
                {
                    opts.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    opts.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    opts.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddOpenIdConnect("signicat", opts =>
                {
                    opts.Authority = Configuration["Signicat:Authority"];
                    opts.ClientId = Configuration["Signicat:ClientId"];
                    opts.ClientSecret = Configuration["Signicat:ClientSecret"];

                    opts.Scope.Clear();
                    opts.Scope.Add("openid");
                    opts.Scope.Add("profile");
                    
                    opts.ResponseType = "code";

                    opts.CallbackPath = new PathString("/callback");
                    opts.SignedOutCallbackPath = new PathString("/signout-callback");
                    opts.ClaimsIssuer = "Signicat";
                    
                    // Saves the ID and access tokens to the AuthenticationProperties.
                    // The ID token is required when signing out to allow bypassing the logout confirmation screen and redirect back to our application. 
                    opts.SaveTokens = true;

                    opts.TokenValidationParameters = new TokenValidationParameters()
                    {
                        NameClaimType = "name"
                    };
                    
                    // Retrieves the JWK that will be used for decryption.
                    opts.TokenValidationParameters.TokenDecryptionKeyResolver =
                        (token, securityToken, kid, parameters) =>
                        {
                            var keys = new List<SecurityKey>();

                            var jwk = Configuration.GetValue<string>($"Signicat:Jwks:{kid}");
                            if (jwk != null)
                            {
                                keys.Add(new JsonWebKey(jwk));
                            }

                            return keys;
                        };
                        
                    opts.Events.OnTokenValidated = ctx =>
                    {
                        ctx.SecurityToken = ctx.SecurityToken.InnerToken ?? ctx.SecurityToken;
                        return Task.CompletedTask;
                    };
                });
            
            // Disables .NET Core claims mapping in order to use the exact claims that are returned from Signicat. 
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            
            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
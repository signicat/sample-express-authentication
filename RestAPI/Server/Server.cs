using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Idfy;
using Idfy.IdentificationV2;

namespace Server 
{
    class Server 
    {
        public static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://0.0.0.0:4242")
                .UseWebRoot(".")
                .UseStartup<Startup>()
                .Build()
                .Run();
        }
    }   

    public class Startup
    {        
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddNewtonsoftJson(); 
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage(); 
            app.UseRouting();
            app.UseStaticFiles(); 
            app.UseEndpoints(endpoints => endpoints.MapControllers()); 

        }
    }


    [Route("authentication-session")] 
    [ApiController] 
    public class AuthenticationApiController : Controller
    {
        private readonly IIdentificationV2Service _identificationV2Service;

        public AuthenticationApiController()
        {
            // Todo: Fill in your API Credential
            _identificationV2Service = new IdentificationV2Service(
                "<INSERT CLIENT_ID>",
                "<INSERT CLIENT_SECRET>",
                new List<OAuthScope>
                {
                    OAuthScope.Identify
                });
        }

        [HttpPost] 
        public async Task<ActionResult> Create() 
        {
            var domain = "http://localhost:3000/";
            var backendDomain = "http://localhost:4242/";

            var session = await _identificationV2Service.CreateSessionAsync(new IdSessionCreateOptions()
            {
                Flow = IdSessionFlow.Redirect,
                RedirectSettings = new RedirectSettings()
                {
                    ErrorUrl = domain + "?error=true",
                    AbortUrl = domain + "?canceled=true",
                    SuccessUrl = backendDomain + "authentication-session"
                },
                AllowedProviders = new List<IdProviderType>
                {
                    IdProviderType.Mitid,
                    IdProviderType.NoBankidNetcentric,
                    IdProviderType.NoBankidMobile,
                    IdProviderType.SeBankid,
                },
                Include = new List<Include>()
                {
                   Include.Name,
                   Include.Nin
                }
            });

            Response.Headers.Add("Location", session.Url); 
            return new StatusCodeResult(303); 
        }
        
        [HttpGet]
        public async Task<ActionResult> Retrieve([FromQuery(Name = "sessionId")] string sessionId)
        {
            var domain = "http://localhost:3000/";

            var result = await _identificationV2Service.GetSessionAsync(sessionId);
            string name = result.Identity.FullName;
            string nin = result.Identity.Nin;

            Response.Headers.Add("Location", domain + "?success=true&name=" + name + "&nin=" + nin);
            return new StatusCodeResult(303);
        }
    }
}

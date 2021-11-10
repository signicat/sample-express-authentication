using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Signicat.Express;
using Signicat.Express.IdentificationV2;

namespace Server
{
    class Server
    {
        public static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://localhost:4242")
                .UseWebRoot(".")
                .UseStartup<Startup>()
                .Build()
                .Run();
        }
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddNewtonsoftJson();

            services.AddSingleton<IIdentificationV2Service>(c => new IdentificationV2Service(
                Configuration["Signicat:ClientId"],
                Configuration["Signicat:ClientSecret"],
                new List<OAuthScope>() { OAuthScope.Identify }
            ));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseStaticFiles();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }


    [Route("authentication-session")]
    [ApiController]
    public class AuthenticationApiController : Controller
    {
        private readonly IIdentificationV2Service _identificationService;
        private readonly string _frontendAppUrl;
        private readonly string _backendUrl;

        public AuthenticationApiController(IIdentificationV2Service identificationService, IConfiguration configuration)
        {
            _identificationService = identificationService;
            _frontendAppUrl = configuration["FrontendAppUrl"];
            _backendUrl = configuration["BackendUrl"];
        }

        [HttpPost]
        public async Task<ActionResult> Create()
        {
            var session = await _identificationService.CreateSessionAsync(new IdSessionCreateOptions()
            {
                Flow = IdSessionFlow.Redirect,
                RedirectSettings = new RedirectSettings()
                {
                    ErrorUrl = _frontendAppUrl + "?error=true",
                    AbortUrl = _frontendAppUrl + "?canceled=true",
                    SuccessUrl = _backendUrl + "authentication-session"
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
            var result = await _identificationService.GetSessionAsync(sessionId);

            var name = result.Identity.FullName;
            var nin = result.Identity.Nin;

            Response.Headers.Add("Location", _frontendAppUrl + "?success=true&name=" + name + "&nin=" + nin);
            return new StatusCodeResult(303);
        }
    }
}
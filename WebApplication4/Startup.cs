using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace EmployeeManagement
{

    public class Startup
    {
        private IConfiguration _config;
        public Startup(IConfiguration config)
        {
            _config = config;
        }
        

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<AppDbContext>(
                options => options.UseSqlServer(_config.GetConnectionString("EmployeeDBConnection")));
            services.AddIdentity<ApplicationUser, IdentityRole>(options => //zamiast application user bylo IdentityUser
            {
                options.Password.RequiredLength = 3;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireNonAlphanumeric = false;
            }).AddEntityFrameworkStores<AppDbContext>();//logowanie info



            /*
            services.Configure<IdentityOptions>(options => //tak tez mozna 
            {
                options.Password.RequiredLength = 3;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireNonAlphanumeric = false;
            });
            */

            //services.AddMvc().AddXmlDataContractSerializerFormatters(); //wszystkie [czyli standard plus Core] .addxxx teraz w formacie xml

            services.AddMvc(config => {
                var policy = new AuthorizationPolicyBuilder()
                                .RequireAuthenticatedUser()
                                .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            }).AddXmlDataContractSerializerFormatters(); //dla wszystkich kontrolerow


            // services.AddMvcCore(); //tylko core
            //services.AddSingleton<IEmployeeRepository, MockEmployeeRepository>();// dependency injection ułatwia tesotwanie i przy duzych projektach wystarczy zmeinic tylko tutaj
            //Scope Transient, Signleton
            //w tym samym http: zostaje, nowa, zostaje
            //w innym http: nowa, nowa, zostaje
            services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();//teraz zamiast mocka w pamieci uzywamy sqla
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                DeveloperExceptionPageOptions devExp = new DeveloperExceptionPageOptions
                {
                    SourceCodeLineCount = 10//ilosc linii
                };
                app.UseDeveloperExceptionPage(devExp);//zawsze pierwsze dla view exception
            }
            else if (env.IsStaging() || env.IsProduction() || env.IsEnvironment("UAT"))
            {
                //  app.UseStatusCodePages();//jak nie ma takiej strony to standarodowe brak strony
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
                //UseStatusCodePagesWithRedirects vs UseStatusCodePagesWithReExecute
                //rexecute zapamietuje adres, mozna wiecej danych wyciagnac
                //redirect napisuje error code na sukces, zmienia url
                app.UseExceptionHandler("/Error");
            }

            ////// Mozliwosc uzycia wwwroota 1 metoda
            /*
            DefaultFilesOptions defFiles = new DefaultFilesOptions();
            defFiles.DefaultFileNames.Clear();
            defFiles.DefaultFileNames.Add("foo.html");
            //  app.UseDefaultFiles(defFiles); //must be first, żeby default.html otworzyć
            // app.UseStaticFiles();//wwwroot usage
            */
            /////


            /////usedefaultfiels i usestaticfiles w jednym, 2 metoda
            /*
            FileServerOptions defFiles2 = new FileServerOptions();
            defFiles2.DefaultFilesOptions.DefaultFileNames.Clear();
            defFiles2.DefaultFilesOptions.DefaultFileNames.Add("foo.html");
            app.UseFileServer(defFiles2);
            */
            ///////

            app.UseStaticFiles();
            app.UseAuthentication();//kolejnosc wazna bo autoryzacjia przed routowaniem
            //  app.UseMvcWithDefaultRoute();//po staticfiles zeby nie czekac, jak znajdzie homecontroller to nie przejdzie dalej
            //app.UseMvc(); //mvc bez default route 
            app.UseMvc(routes =>
{
                routes.MapRoute("default", "{controller=home}/{action=Index}/{id?}");//? optional
                // routes.MapRoute("default", "CompanyName/{controller=home}/{action=Index}/{id?}");//z company name jak działamy na tag helperach to wszystkie reflinki automatycznie zmieniają się
});

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
                //throw new Exception("Some error");
                /*
                await context.Response.WriteAsync("Hosting Enviroment: "+env.EnvironmentName);
                if (env.IsEnvironment("nazwa"))
                {
                    await context.Response.WriteAsync("Enviroment to nazwa!");
                }
                */
            });
        }
    }
}

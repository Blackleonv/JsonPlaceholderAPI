using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JsonPlaceholderAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // Bu yöntem, çalışma zamanı tarafından çağrılır. Servisleri kapsayıcıya eklemek için bu yöntemi kullanın.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(); // MVC kontrolcülerini ekliyoruz
            services.AddHttpClient();  // HttpClient servisini ekliyoruz
        }

        // Bu yöntem, çalışma zamanı tarafından çağrılır. HTTP istek boru hattını yapılandırmak için bu yöntemi kullanın.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

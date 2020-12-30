using eShop.Lib;
using eShop.Shop.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace eShop.Shop
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
            CargoChainConfiguration cargoChainConfiguration = new CargoChainConfiguration();
            Configuration.GetSection("CargoChain").Bind(cargoChainConfiguration);
            services.AddSingleton(cargoChainConfiguration);

            services.AddSingleton<ShopContext>();
            services.AddSingleton<CargoChainService>();

            services.AddControllersWithViews();

            services.AddCors(options =>
            {
                //options.AddPolicy("CargoChainSpecificOrigin", 
                //    builder => builder
                //        .WithOrigins(cargoChainConfiguration.ApiUrl.ToString())
                //        .AllowAnyHeader()
                //        .AllowAnyMethod());

                options.AddPolicy("AnyOriginPolicy",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseCors();
        }
    }
}

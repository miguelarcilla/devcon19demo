using Blazored.LocalStorage;
using DevCon19.Web.Services;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GeolocationService = AspNetMonsters.Blazor.Geolocation.LocationService;

namespace DevCon19.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IWeatherForecastService, HttpWeatherForecastService>();
            services.AddScoped<GeolocationService>();
            services.AddBlazoredLocalStorage();
            services.AddScoped<PinnedLocationsService>();
            services.AddScoped<IConfiguration, LocalStorageConfiguration>();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}

﻿using DevCon19.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DevCon19.Web.Services
{
    public interface IWeatherForecastService
    {
        Task<Location> GetLocationByGeolocation(decimal latitude, decimal longitude);

        Task<Location[]> GetLocationsByText(string search, CancellationToken cancellationToken);

        IAsyncEnumerable<WeatherResponse> GetStreamingWeather(string locationKey, CancellationToken token);

        Task<WeatherResponse> GetWeather(string locationKey);
    }
}
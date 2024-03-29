﻿using DevCon19.Common.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DevCon19.Web.Services
{
    public class HttpWeatherForecastService : IWeatherForecastService
    {
        private readonly HttpClient _httpClient;
        private string apiKey;

        public HttpWeatherForecastService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://dataservice.accuweather.com/");

            apiKey = "2tVmykQlBx5ffu91mVxWRGvxa1PDZxEA";
        }

        public async Task<Location[]> GetLocationsByText(string search, CancellationToken cancellationToken)
        {
            // Query AccuWeather Straight
            using var response = await _httpClient.GetAsync($"locations/v1/cities/US/autocomplete?{GetApiKey()}&q={search}", cancellationToken);

            // Query Azure Functions (Dev)
            //using var response = await _httpClient.GetAsync($"http://localhost:7071/api/locations?query={search}", cancellationToken);

            // Query Azure Functions (Prod)
            // Ensure that CORS have been enabled for your website in the Azure Function App Settings
            //using var response = await _httpClient.GetAsync($"https://anarcilldevconfunctions.azurewebsites.net/api/locations?query={search}", cancellationToken);

            using var responseStream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<Location[]>(responseStream, cancellationToken: cancellationToken);
        }

        public async Task<Location> GetLocationByGeolocation(decimal latitude, decimal longitude)
        {
            return await JsonSerializer.DeserializeAsync<Location>(await _httpClient.GetStreamAsync($"locations/v1/cities/geoposition/search?{GetApiKey()}&q={latitude},{longitude}"));
        }

        public async IAsyncEnumerable<WeatherResponse> GetStreamingWeather(string locationKey, [EnumeratorCancellation] CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var weather = await GetWeatherCore(locationKey);
                yield return weather.WeatherResponse;
                await Task.Delay(weather.Expires);
            }
        }

        public async Task<WeatherResponse> GetWeather(string locationKey)
        {
            var weather = await GetWeatherCore(locationKey);
            return weather.WeatherResponse;
        }

        async Task<(WeatherResponse WeatherResponse, TimeSpan Expires)> GetWeatherCore(string locationKey)
        {
            var response = await _httpClient.GetAsync($"currentconditions/v1/{locationKey}?{GetApiKey()}&details=true");
            var model = await JsonSerializer.DeserializeAsync<Forecast[]>(await response.Content.ReadAsStreamAsync());
            var weather = new WeatherResponse(model.First());
            var expiresHeader = response.Content.Headers.Expires;
            var expires = expiresHeader.HasValue ? expiresHeader.Value.UtcDateTime.Subtract(DateTime.UtcNow) : TimeSpan.FromSeconds(2);
            return (weather, expires);
        }

        string GetApiKey() => $"apikey={apiKey}";
    }
}

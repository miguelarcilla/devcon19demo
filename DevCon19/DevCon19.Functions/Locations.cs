using DevCon19.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DevCon19.Functions
{
    public static class Locations
    {
        [FunctionName("Locations")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log, ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Obtain reference to App Configuration to secure AccuWeather API Key
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            var apiKey = config["AccuWeatherToken"];

            // Obtain query parameters from request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string query = req.Query["query"];
            query ??= data?.locationKey;

            // If no locationKey provided in request, terminate function early
            if (query == null) return new BadRequestObjectResult("Please provide a location query in the query string or in the request body");

            // Initialize an HttpClient to query AccuWeather
            using HttpClient httpClient = new HttpClient { BaseAddress = new Uri("https://dataservice.accuweather.com/") };
            var response = await httpClient.GetAsync($"locations/v1/cities/US/autocomplete?apikey={apiKey}&q={query}");

            // Return API response
            return new OkObjectResult(await response.Content.ReadAsStringAsync());
        }
    }
}

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using Serilog;
using Serilog.Core;

namespace Staż.Controllers;

[ApiController]
[Route("[controller]")]
public class AviationDataController : ControllerBase
{
    readonly IConfiguration configuration;
    readonly Logger logger;

    public AviationDataController(IConfiguration configuration)
    {
        this.configuration = configuration;
        logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/AviationData.txt", rollingInterval: RollingInterval.Minute)
            .CreateLogger();
    }

    [HttpGet]
    public string GetFlightsInTheAir(string departureCode, string arrivalCode)
    {
        var request = GetRequest(departureCode, arrivalCode);
        var data = JsonSerializer.Deserialize<JsonNode>(request);
        if (data == null){
            logger.Error("Response: Data is null");
            throw new Exception("Data is null");
        }
        var output = "";
        foreach (var flight in data["data"]!.AsArray()){
            output += Output(flight!);
        }
        logger.Information("Returning output: \n{Output}", output);
        return output;
    }

    static string Output(JsonNode flight)
    {
        var estimated = flight["arrival"]!["estimated"];
        var airline = flight["airline"]!["name"];
        var flightNr = flight["flight"]!["number"];
        var flightText = $"Flight: nr: {flightNr}; Airline: {airline}; Estimated time of arrival: {estimated}\n";
        return flightText;
    }

    string GetRequest(string departure, string arrival)
    {
        var requestUri = $"/v1/flights?access_key={configuration["ApiKey"]}&dep_iata={departure}&arr_iata{arrival}&flight_status=active";
        logger.Information("Request: {RequestUri}", requestUri);
        var client = new RestClient("http://api.aviationstack.com");
        var request = new RestRequest(requestUri);
        var response = client.Get(request);
        logger.Information("Response: {StatusCode}", response.StatusCode);
        if (response.StatusCode != System.Net.HttpStatusCode.OK){
            logger.Error("Response: {ErrorMessage}", response.ErrorMessage);
            throw new Exception(response.ErrorMessage);
        }
        return response.Content!;
    }
}
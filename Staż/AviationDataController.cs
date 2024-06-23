using System.Diagnostics;
using RestSharp;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace Staż.Controllers;

[ApiController]
[Route("[controller]")]
public class AviationDataController : ControllerBase
{
    HttpClient sharedClient = new();

    [HttpGet]
    public async Task<string> GetFlightsInTheAir(string departureCode, string arrivalCode)
    {
        var output = "";
        var request = await GetRequest(departureCode, arrivalCode);
        // Use restsharp
        var data = JsonSerializer.Deserialize<JsonNode>(request);
        Debug.Assert(data != null, nameof(data) + " != null");
        foreach (var flight in data["data"]!.AsArray()){
            var flightText = $"Flight: nr: {flight!["flight"]!["number"]} airline: {flight["airline"]!["name"]} estimated time of arrival: {flight["arrival"]!["estimated"]}\n";
            output += flightText;
        }
        return output;
    }

    async Task<string> GetRequest(string departure, string arrival)
    {
        var requestUri = $"http://api.aviationstack.com/v1/flights?access_key={YOUR_ACCESS_KEY}&dep_iata={departure}&arr_iata{arrival}&flight_status=active";
        Console.WriteLine("Request URI: " + requestUri); //replace with serilog
        Console.WriteLine("Request sent");
        var response = await sharedClient.GetStringAsync(requestUri);
        Console.WriteLine("Response received");
        return response;
    }

    const string YOUR_ACCESS_KEY = "656d03ab8936ca69c67727f8cfc8a299"; //Move to appsettings.json
}
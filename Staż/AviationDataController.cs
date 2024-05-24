using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Staż.Controllers;

[ApiController]
[Route("[controller]")]
public class AviationDataController : ControllerBase
{
    static HttpClient sharedClient = new();
    [HttpGet]
    public async Task<string> GetFlightsInTheAir(string departureCode, string arrivalCode)
    {
        var output = "";
        var request = await GetRequest(departureCode, arrivalCode);
        var document = JsonDocument.Parse(request);
        var data = document.RootElement.GetProperty("data");
        if (data.GetArrayLength() == 0){
            output = "No flights in the air";
        }
        else{
            var arrayEnumerator = data.EnumerateArray();
            foreach (var flight in arrayEnumerator){
                var flightText = "\nFlight: nr: " + flight.GetProperty("flight").GetProperty("number").GetString() + " airline: " + flight.GetProperty("airline").GetProperty("name").GetString() + " estimated time of arrival: " + flight.GetProperty("arrival").GetProperty("estimated").GetString() + "\n";
                output += flightText;
            }
        }
        return output;
    }

    static async Task<string> GetRequest(string departure, string arrival)
    {
        var requestUri = "http://api.aviationstack.com/v1/flights?access_key=" + YOUR_ACCESS_KEY + "&dep_iata=" + departure + "&arr_iata=" + arrival + "&flight_status=active";
        Console.WriteLine("Request URI: " + requestUri);
        Console.WriteLine("Request sent");
        var response = await sharedClient.GetStringAsync(requestUri);
        Console.WriteLine("Response received");
        return response;
    }

    const string YOUR_ACCESS_KEY = "656d03ab8936ca69c67727f8cfc8a299";
}
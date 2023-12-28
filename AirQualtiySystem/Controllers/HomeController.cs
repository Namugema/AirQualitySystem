using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AirQualtiySystem.Models;
using System.Text;
using Microsoft.Extensions.Configuration;
using AirQualtiySystem.Services;
using Amazon.DynamoDBv2;
using Amazon;

namespace AirQualtiySystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DynamoDBService _dynamoDbService;

    public HomeController(ILogger<HomeController> logger, DynamoDBService dynamoDbService)
    {
        _logger = logger;
        _dynamoDbService = dynamoDbService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public async Task<IActionResult> DownloadCsv(AirQualityQueryModel queryModel)
    {
        try
        {
            var items = await _dynamoDbService.GetItemsByDateRangeAndLocationAsync(queryModel.StartDate, queryModel.EndDate, queryModel.Country);

            if (items == null || !items.Any())
            {
                ViewData["ErrorMessage"] = "Data not found.";
                return View("Index");
            }

            // Convert the data to CSV format
            var csv = ConvertToCsv(items);

            // Return the CSV file
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "AirQualityData.csv");
        }
        catch (Exception ex)
        {
            ViewData["ErrorMessage"] = ex.Message;
            return View("Index");
        }
    }

    private string ConvertToCsv(List<DailyAirQualityModel> data)
    {
        var csvBuilder = new StringBuilder();

        // Add header
        csvBuilder.AppendLine("Id,Timestamp,Country,Altitude,ExactLocation,Indoor,Latitude,Longitude,SensorId,SensorPin,SensorTypeName,SensorManufacturer,SensorDataValues");

        foreach (var item in data)
        {
            // Extract properties from JSON objects and handle nulls
            var locationCountry = item.location?.country ?? "";
            var altitude = item.location?.altitude ?? "";
            var exactLocation = item.location?.exact_location?.ToString() ?? "";
            var indoor = item.location?.indoor?.ToString() ?? "";
            var latitude = item.location?.latitude ?? "";
            var longitude = item.location?.longitude ?? "";

            var sensorId = item.sensor?.id?.ToString() ?? "";
            var sensorPin = item.sensor?.pin ?? "";
            var sensorTypeName = item.sensor?.sensor_type?.name ?? "";
            var sensorManufacturer = item.sensor?.sensor_type?.manufacturer ?? "";

            var sensorDataValues = item.sensordatavalues != null
                ? string.Join(";", item.sensordatavalues.Select(sdv => $"[{sdv.id}, {sdv.value}, {sdv.value_type}]"))
                : "";

            // Convert each item to a CSV row and append
            csvBuilder.AppendLine($"{item.id},{item.timestamp},{locationCountry},{altitude},{exactLocation},{indoor},{latitude},{longitude},{sensorId},{sensorPin},{sensorTypeName},{sensorManufacturer},{sensorDataValues}");
        }

        return csvBuilder.ToString();
    }
}


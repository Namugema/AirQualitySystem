using System;
namespace AirQualtiySystem.Models
{
    public class DailyAirQualityModel
    {
        public string timestamp { get; set; }
        public long id { get; set; }
        public string? locationJson { get; set; }
        public double? sampling_rate { get; set; }
        public string? sensorJson { get; set; }
        public string? sensorDataValuesJson { get; set; }

        // Optional: Properties for parsed JSON (if needed)
        public Location? location { get; set; }
        public Sensor? sensor { get; set; }
        public List<SensorDataValue>? sensordatavalues { get; set; }
    }

    //additional classes if needed for parsing JSON
    public class Location
    {
        public long? id { get; set; }
        public string? altitude { get; set; }
        public string? country { get; set; }
        public int? exactlocation { get; set; }
        public int? indoor { get; set; }
        public string? latitude { get; set; }
        public string? longitude { get; set; }
    }

    public class Sensor
    {
        public long? id { get; set; }
        public string? pin { get; set; }
        public string? SensorTypeJson { get; set; }

        // Optional: Properties for parsed JSON (if needed)
        public SensorType? sensor_type { get; set; }
    }

    public class SensorDataValue
    {
        public long? id { get; set; }
        public string? value { get; set; }
        public string? value_type { get; set; }
    }

    public class SensorType
    {
        public long? id { get; set; }
        public string? manufacturer { get; set; }
        public string? name { get; set; }
    }

    public class AirQualityQueryModel
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Country { get; set; }
    }

}
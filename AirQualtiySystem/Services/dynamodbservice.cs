using System;
using AirQualtiySystem.Models;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AirQualtiySystem.Services
{
	public class DynamoDBService
    {
        private readonly AmazonDynamoDBClient _dynamoDbClient;

        public DynamoDBService(IConfiguration configuration)
		{
            var region = RegionEndpoint.GetBySystemName(configuration["AWS:Region"]);
            _dynamoDbClient = new AmazonDynamoDBClient(region);
        }

        public async Task<List<DailyAirQualityModel>?> GetItemsAsync(string timestamp, long id)
        {
            try
            {
                var request = new QueryRequest
                {
                    TableName = "DailyAirQuality",
                    KeyConditionExpression = "#pk = :pkval AND #sk = :skval",
                    ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#pk", "timestamp" },  // partition key name
                { "#sk", "id" }        // sort key name
            },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":pkval", new AttributeValue { S = timestamp } },
                { ":skval", new AttributeValue { N = id.ToString() } }
            }
                };

                var response = await _dynamoDbClient.QueryAsync(request);

                if (response.Items == null || response.Items.Count == 0)
                {
                    return null;
                }

                var items = new List<DailyAirQualityModel>();

                foreach (var respItem in response.Items)
                {
                    var item = new DailyAirQualityModel
                    {
                        // Assign properties from respItem to item
                        id = Convert.ToInt64(respItem["id"].N),
                        timestamp = respItem["timestamp"].S,
                        locationJson = respItem.ContainsKey("location") ? Document.FromAttributeMap(respItem["location"].M).ToJson() : null,
                        sampling_rate = null,
                        sensorJson = respItem.ContainsKey("sensor") ? Document.FromAttributeMap(respItem["sensor"].M).ToJson() : null

                    };


                    // Optional: Parsing JSON into structured objects
                    if (item.locationJson != null && !string.IsNullOrEmpty(item.locationJson))
                    {
                        item.location = JsonConvert.DeserializeObject<Location>(item.locationJson);
                    }

                    if (item.sensorJson != null && !string.IsNullOrEmpty(item.sensorJson))
                    {
                        item.sensor = JsonConvert.DeserializeObject<Sensor>(item.sensorJson);
                    }

                    item.sensordatavalues = respItem.ContainsKey("sensordatavalues") ? respItem["sensordatavalues"].L.Select(av => new SensorDataValue
                    {
                        id = av.M.ContainsKey("id") ? long.Parse(av.M["id"].N): null,
                        value = av.M.ContainsKey("value") ? av.M["value"].S: null,
                        value_type = av.M.ContainsKey("value_type") ? av.M["value_type"].S: null
                    }).ToList()
                        : null;

                    if (item.sensordatavalues != null)
                    {
                        item.sensorDataValuesJson = JsonConvert.SerializeObject(item.sensordatavalues);
                    }

                    if (item.sensor != null && !string.IsNullOrEmpty(item.sensor.SensorTypeJson))
                    {
                        item.sensor.sensor_type = JsonConvert.DeserializeObject<SensorType>(item.sensor.SensorTypeJson);
                    }

                    items.Add(item);
                }

                return items;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<DailyAirQualityModel>?> GetItemsByDateRangeAndLocationAsync(string startDate, string endDate, string? country = null)
        {
            try
            {
                var request = new ScanRequest
                {
                    TableName = "DailyAirQuality",
                    FilterExpression = "#pk BETWEEN :start_date AND :end_date",
                    ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#pk", "timestamp" }
            },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":start_date", new AttributeValue { S = startDate } },
                { ":end_date", new AttributeValue { S = endDate } }
            }
                };

                if (!string.IsNullOrEmpty(country))
                {
                    // Add the #location key to ExpressionAttributeNames
                    request.ExpressionAttributeNames.Add("#location", "location");

                    // Append the country filter to the existing filter expression with an AND operator
                    request.FilterExpression += " AND contains(#location.country, :country)";
                    request.ExpressionAttributeValues.Add(":country", new AttributeValue { S = country });
                }

                var response = await _dynamoDbClient.ScanAsync(request);

                if (response.Items == null || response.Items.Count == 0)
                {
                    return null;
                }

                var items = new List<DailyAirQualityModel>();

                foreach (var respItem in response.Items)
                {
                    var item = new DailyAirQualityModel
                    {
                        // Assign properties from respItem to item
                        id = Convert.ToInt64(respItem["id"].N),
                        timestamp = respItem["timestamp"].S,
                        locationJson = respItem.ContainsKey("location") ? Document.FromAttributeMap(respItem["location"].M).ToJson() : null,
                        sampling_rate = null,
                        sensorJson = respItem.ContainsKey("sensor") ? Document.FromAttributeMap(respItem["sensor"].M).ToJson() : null

                    };


                    // Optional: Parsing JSON into structured objects
                    if (item.locationJson != null && !string.IsNullOrEmpty(item.locationJson))
                    {
                        item.location = JsonConvert.DeserializeObject<Location>(item.locationJson);
                    }

                    if (item.sensorJson != null && !string.IsNullOrEmpty(item.sensorJson))
                    {
                        item.sensor = JsonConvert.DeserializeObject<Sensor>(item.sensorJson);
                    }

                    item.sensordatavalues = respItem.ContainsKey("sensordatavalues") ? respItem["sensordatavalues"].L.Select(av => new SensorDataValue
                    {
                        id = av.M.ContainsKey("id") ? long.Parse(av.M["id"].N) : null,
                        value = av.M.ContainsKey("value") ? av.M["value"].S : null,
                        value_type = av.M.ContainsKey("value_type") ? av.M["value_type"].S : null
                    }).ToList()
                        : null;

                    if (item.sensordatavalues != null)
                    {
                        item.sensorDataValuesJson = JsonConvert.SerializeObject(item.sensordatavalues);
                    }

                    if (item.sensor != null && !string.IsNullOrEmpty(item.sensor.SensorTypeJson))
                    {
                        item.sensor.sensor_type = JsonConvert.DeserializeObject<SensorType>(item.sensor.SensorTypeJson);
                    }

                    items.Add(item);
                }

                return items;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}


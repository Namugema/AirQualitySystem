import json
import boto3
import requests
from decimal import Decimal

#Ranges for calculating Air Quality Index
p1_ranges = [(0, 11, 1), (12, 23, 2), (24, 35, 3), (36, 41, 4), (42, 47, 5), (48, 53, 6), (54, 58, 7), (59, 64, 8), (65, 70, 9), (71, float('inf'), 10)]
p2_ranges = [(0, 16, 1), (17, 33, 2), (34, 50, 3), (51, 58, 4), (59, 66, 5), (67, 75, 6), (76, 83, 7), (84, 91, 8), (92, 100, 9), (101, float('inf'), 10)]

# Convert all float values to Decimal
def convert_floats_to_decimals(obj):
    if isinstance(obj, float):
        return Decimal(str(obj))
    elif isinstance(obj, dict):
        return {k: convert_floats_to_decimals(v) for k, v in obj.items()}
    elif isinstance(obj, list):
        return [convert_floats_to_decimals(v) for v in obj]
    return obj

def get_aqi_for_value(value, ranges):
    for (rangeMinValue, rangeMaxValue, aqi) in ranges:
        if rangeMinValue <= value <= rangeMaxValue:
            return aqi
    return -1

def calculateAQI(p1, p2):
    p1_aqi = get_aqi_for_value(p1, p1_ranges)
    p2_aqi = get_aqi_for_value(p2, p2_ranges)

    return max(p1_aqi, p2_aqi)

def fetch_and_insert_data():
    # URL to fetch data from
    #24 hours
    url = 'https://data.sensor.community/static/v2/data.24h.json'

    # Fetch data from URL
    response = requests.get(url)
    data_list = response.json()  # Assuming the data is in JSON format

    # DynamoDB resource
    dynamodb = boto3.resource('dynamodb', region_name='us-east-1')

    table = dynamodb.Table('DailyAirQuality')

    # Update DynamoDB table
    for data in data_list:
        # Convert float to Decimal in data
        converted_data = convert_floats_to_decimals(data)

        # Attempt to extract P1 and P2 values from the sensor data
        p2_value = next((item['value'] for item in converted_data['sensordatavalues'] if item['value_type'] == 'P2'), None)
        p1_value = next((item['value'] for item in converted_data['sensordatavalues'] if item['value_type'] == 'P1'), None)

        # Check if both P1 and P2 values are present before calculating AQI and updating the database
        if p1_value is not None and p2_value is not None:
            p2 = float(p2_value)
            p1 = float(p1_value)
            aqi = calculateAQI(p1, p2)
            converted_data['AQI'] = Decimal(str(aqi))

        # Update DynamoDB table
        table.put_item(Item=converted_data)

    return {
        'statusCode': 200,
        'body': json.dumps(f'{len(data_list)} items updated in DynamoDB')
    }

# Call the function
response = fetch_and_insert_data()
print(response)

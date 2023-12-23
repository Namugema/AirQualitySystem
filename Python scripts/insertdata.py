import json
import boto3
import requests
from decimal import Decimal

# Convert all float values to Decimal
def convert_floats_to_decimals(obj):
    if isinstance(obj, float):
        return Decimal(str(obj))
    elif isinstance(obj, dict):
        return {k: convert_floats_to_decimals(v) for k, v in obj.items()}
    elif isinstance(obj, list):
        return [convert_floats_to_decimals(v) for v in obj]
    return obj

def fetch_and_update_data():
    # URL to fetch data from
    #5minutes
    #url = 'https://data.sensor.community/static/v2/data.json'
    #24 hours
    url = 'https://data.sensor.community/static/v2/data.24h.json'

    # Fetch data from URL
    response = requests.get(url)
    data_list = response.json()  # Assuming the data is in JSON format

    # DynamoDB resource
    dynamodb = boto3.resource('dynamodb', region_name='us-east-1')
    #5minutes
    #table = dynamodb.Table('AirQuality')

    #24 hours
    table = dynamodb.Table('DailyAirQuality')

    # Update DynamoDB table
    for data in data_list:
        # Convert float to Decimal in data
        converted_data = convert_floats_to_decimals(data)

        # Update DynamoDB table
        table.put_item(Item=converted_data)

    return {
        'statusCode': 200,
        'body': json.dumps(f'{len(data_list)} items updated in DynamoDB')
    }

# Call the function
response = fetch_and_update_data()
print(response)

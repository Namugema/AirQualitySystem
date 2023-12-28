import json
import requests
import boto3
from boto3.dynamodb.conditions import Key
from decimal import Decimal
import schedule
import time

# Initialize the DynamoDB resource
dynamodb = boto3.resource('dynamodb', region_name='us-east-1')
#5minutes
#table = dynamodb.Table('AirQuality')

#24 hours
table = dynamodb.Table('DailyAirQuality')

# Convert all float values to Decimal
def convert_floats_to_decimals(obj):
    if isinstance(obj, float):
        return Decimal(str(obj))
    elif isinstance(obj, dict):
        return {k: convert_floats_to_decimals(v) for k, v in obj.items()}
    elif isinstance(obj, list):
        return [convert_floats_to_decimals(v) for v in obj]
    return obj

def fetch_and_update_db():

    # Fetch the data from the URL
    #5 minutes
    #response = requests.get("https://data.sensor.community/static/v2/data.json")
    #24hours
    response = requests.get("https://data.sensor.community/static/v2/data.24h.json")
    
    data_list = response.json()

    # Process and update DynamoDB
    counter = 0

    for data in data_list:

        item = convert_floats_to_decimals(data)

        unique_id = item['id']
        unique_timestamp = item['timestamp']

        # Check if the item already exists in DynamoDB
        existing_item = table.query(
            KeyConditionExpression=Key('timestamp').eq(unique_timestamp) & Key('id').eq(unique_id)
            )

        if not existing_item['Items']:
            # If item does not exist, update the database
            table.put_item(Item=item)
            counter = counter + 1

    return {
            'statusCode': 200,
            'body': json.dumps(f'{counter} new items created in DynamoDB')
        }

response = fetch_and_update_db()
print(response['body'])

'''
def job():
    response = fetch_and_update_db()
    print(response['body'])

# Schedule the job every 5 minutes
schedule.every(5).minutes.do(job)

# Run the scheduler
while True:
    schedule.run_pending()
    time.sleep(1)
'''
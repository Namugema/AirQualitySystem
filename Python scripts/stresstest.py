from locust import HttpUser, TaskSet, task, between
import datetime
import random

# Define a list of country codes with an empty string to represent an empty/null value
COUNTRY_CODES = ["US", "CA", "GB", "DE", "FR", "JP", "AU", "IN", "CN", "BR", ""]

class UserBehavior(TaskSet):
    
    @task
    def submit_form(self):
        # This is the URL for the form POST data.
        submit_url = "http://whyme-env.eba-h9z3xdpk.us-east-1.elasticbeanstalk.com/Home/DownloadCsv"

        # Generate dynamic dates for testing
        start_date = (datetime.datetime.now() - datetime.timedelta(days=1)).strftime('%d/%m/%Y')
        end_date = datetime.datetime.now().strftime('%d/%m/%Y')

        # Randomly select a country code from the list or an empty string
        country_code = random.choice(COUNTRY_CODES)

        # Simulation data for form fields.
        form_data = {
            "StartDate": start_date,
            "EndDate": end_date,
            "Country": country_code
        }

        # Submit a POST request to the form submission URL with the form data.
        self.client.post(submit_url, data=form_data)

class WebsiteUser(HttpUser):
    tasks = [UserBehavior]
    wait_time = between(5, 15)  # Simulate users waiting between 5 and 15 seconds before performing a task

    # Elastic Beanstalk environment
    host = "http://whyme-env.eba-h9z3xdpk.us-east-1.elasticbeanstalk.com/"

# Run the test by executing this script using the locust command in your terminal.
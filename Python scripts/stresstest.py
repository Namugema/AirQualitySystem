from locust import HttpUser, TaskSet, task, between

class UserBehavior(TaskSet):
    
    @task
    def submit_form(self):
        # This is the URL for the form POST data.
        submit_url = "http://whyme-env.eba-h9z3xdpk.us-east-1.elasticbeanstalk.com/Home/DownloadCsv"

        # simulation data for form fields.
        form_data = {
            "timestamp": "2022-06-21 12:56:19",  # You might want to generate a dynamic timestamp
            "id": "10995737600"  # If the ID needs to be dynamic, generate it accordingly
        }

        # Submit a POST request to the form submission URL with the form data.
        self.client.post(submit_url, data=form_data)

class WebsiteUser(HttpUser):
    tasks = [UserBehavior]
    wait_time = between(5, 15)  # Simulate users waiting between 5 and 15 seconds before performing a task

    # Elastic Beanstalk environment
    host = "http://whyme-env.eba-h9z3xdpk.us-east-1.elasticbeanstalk.com/"

# Run the test by executing this script using the locust command in your terminal.
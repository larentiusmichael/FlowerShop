using Microsoft.AspNetCore.Mvc;
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Amazon.S3.Model;

namespace FlowerShop.Controllers
{
    public class SNSExampleController : Controller
    {
        private const string topicARN = "arn:aws:sns:us-east-1:404132070242:SNSExampleLab4";


        //function 1: create function to get back keys from appsettings.json file
        private List<string> getKeys()
        {
            List<string> keys = new List<string>();

            //1. link to appsettings.json and get back the values
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");

            IConfigurationRoot configure = builder.Build(); //build the json file

            //2. read the info from json using configure instance
            keys.Add(configure["Values:Key1"]);
            keys.Add(configure["Values:Key2"]);
            keys.Add(configure["Values:Key3"]);

            return keys;
        }

        //function 2: the page will be used to collect the user email
        //and do the subscription for them
        public IActionResult Index()
        {
            return View();
        }

        //function 3: process subscription function
        public async Task<IActionResult> processSubscription(string email)
        {
            List<string> keys = getKeys();
            AmazonSimpleNotificationServiceClient snsagent = new AmazonSimpleNotificationServiceClient(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

            try
            {
                //add email as the subscriber
                SubscribeRequest emailRequest = new SubscribeRequest
                {
                    TopicArn = topicARN,
                    Protocol = "email",
                    Endpoint = email
                };
                SubscribeResponse response = await snsagent.SubscribeAsync(emailRequest);
                ViewBag.emailRequestId = response.ResponseMetadata.RequestId;
                return View();
            }
            catch (AmazonSimpleNotificationServiceException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //function 4: admin side - send broadcast email to every subscriber!
        //so should have a view with columns and button
        public IActionResult sendBroadcastMessage()
        {
            return View();
        }

        //function 5: send Broadcast Message
        public async Task<IActionResult> processBroadcast(string subjectitle, string messagebody)
        {
            List<string> keys = getKeys();
            AmazonSimpleNotificationServiceClient snsagent = new AmazonSimpleNotificationServiceClient(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

            if (ModelState.IsValid)
            {
                try
                {
                    PublishRequest request = new PublishRequest
                    {
                        TopicArn = topicARN,
                        Subject = subjectitle,
                        Message = messagebody
                    };
                    await snsagent.PublishAsync(request);
                    return Content("Successfully broadcast the message to public!");
                }
                catch (AmazonSimpleNotificationServiceException ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return BadRequest("Broadcast Message is not success! Maybe form issue");
        }
    }

}
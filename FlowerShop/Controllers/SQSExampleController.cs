using Microsoft.AspNetCore.Mvc;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using System.IO;
using Amazon.SimpleNotificationService;
using FlowerShop.Models;
using Newtonsoft.Json;

namespace FlowerShop.Controllers
{
    public class SQSExampleController : Controller
    {
        private const string QueueName = "mvcOrderQueueExample";

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

        //function 2: Order form with client waiting number for admin response
        public async Task<IActionResult> Index()
        {
            List<string> keys = getKeys();
            AmazonSQSClient agent = new AmazonSQSClient(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

            //get the Queue URL
            var response = await agent.GetQueueUrlAsync(new GetQueueUrlRequest { QueueName = QueueName });

            //get as much as possible the messages from the queue.
            GetQueueAttributesRequest attReq = new GetQueueAttributesRequest();
            attReq.QueueUrl = response.QueueUrl;
            attReq.AttributeNames.Add("ApproximateNumberOfMessages");
            GetQueueAttributesResponse response1 = await agent.GetQueueAttributesAsync(attReq);
            ViewBag.count = response1.ApproximateNumberOfMessages;

            return View();
        }

        //function 3: send the customer reservation msg to the queue
        public async Task<IActionResult> sendMsgToQueue(string custname, int pax, string email)
        {
            ReservationOrder order = new ReservationOrder
            {
                custname = custname,
                pax = pax,
                email = email
            };

            List<string> keys = getKeys();
            AmazonSQSClient agent = new AmazonSQSClient(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

            //get the Queue URL
            var response = await agent.GetQueueUrlAsync(new GetQueueUrlRequest { QueueName = QueueName });

            try
            {
                SendMessageRequest message = new SendMessageRequest
                {
                    MessageBody = JsonConvert.SerializeObject(order),
                    QueueUrl = response.QueueUrl
                };
                await agent.SendMessageAsync(message);
            }
            catch(AmazonSQSException ex)
            {
                return BadRequest(ex.Message);
            }

            return RedirectToAction("Index", "SQSExample");
        }
    }
}

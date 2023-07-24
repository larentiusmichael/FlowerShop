using Microsoft.AspNetCore.Mvc;
using Amazon; //for linking your AWS account
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration; //appsettings.json section
using System.IO; // input output
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Constraints;
using System.Net.Mime;

namespace FlowerShop.Controllers
{
    public class ControllerS3Example : Controller
    {
        private const string s3BucketName = "mvcflowershoplab4tp061310";

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

        //function 2: create upload form for uploading the images
        public IActionResult Index()
        {
            return View();
        }

        //function 3: learn how to send image to s3
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessUploadImage(List<IFormFile> imagefile)
        {
            //1. add credential for action
            List<String> values = getKeys();
            AmazonS3Client awsS3client = new AmazonS3Client(values[0], values[1], values[2], RegionEndpoint.USEast1);

            //2. read image by image and store to S3
            foreach(var image in imagefile) 
            { 
                if(image.Length <= 0)
                {
                    return BadRequest("It is an empty file. Unable to upload!");
                }
                else if(image.Length > 1048576) //not more than 1MB
                {
                    return BadRequest("It is over 1MB limit of size. Unable to upload!");
                }
                else if(image.ContentType.ToLower() != "image/png" && image.ContentType.ToLower() != "image/jpeg" && image.ContentType.ToLower() != "image/gif")
                {
                    return BadRequest("It is not a valid image! Unable to upload!");
                }
                //if all the things passed the examination, then start send the file to the s3 bucket
                try
                {
                    PutObjectRequest request = new PutObjectRequest
                    {
                        InputStream = image.OpenReadStream(),
                        BucketName = s3BucketName,
                        Key = "images/" + image.FileName,
                        CannedACL = S3CannedACL.PublicRead
                    };

                    //process request
                    await awsS3client.PutObjectAsync(request);
                }
                catch(AmazonS3Exception ex)
                {
                    return BadRequest("Error in uploading file of " + image.FileName + ": " + ex.Message);

                }
                catch(Exception ex)
                {
                    return BadRequest("Error in uploading file of " + image.FileName + ": " + ex.Message);
                }
            }

            //return back to the index page
            return RedirectToAction("Index", "ControllerS3Example");
        }

        //function 4: learn how to display images from S3
        public async Task<IActionResult> DisplayImagesFromS3()
        {
            //1. add credential for action
            List<String> values = getKeys();
            AmazonS3Client awsS3client = new AmazonS3Client(values[0], values[1], values[2], RegionEndpoint.USEast1);

            List<S3Object> images = new List<S3Object>();   //later use for storing list of images

            try
            {
                //to get to know whether still have next item or not
                string token = null;
                do
                {
                    //create request to ask for 1 image from S3
                    ListObjectsRequest request = new ListObjectsRequest
                    {
                        BucketName = s3BucketName
                    };
                    ListObjectsResponse response = await awsS3client.ListObjectsAsync(request).ConfigureAwait(false);
                    token = response.NextMarker;    //get the next address
                    images.AddRange(response.S3Objects); //add the new image information to images list
                }
                while (token != null);
            }
            catch(AmazonS3Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }

            return View(images);
        }

        //function 5: Learn how to delete image from S3 (worker and No UI)
        public async Task<IActionResult> DeleteImage(string ImageName)
        {
            //get backs the keys and connect to aws account now
            List<String> values = getKeys();
            AmazonS3Client awsS3client = new AmazonS3Client(values[0], values[1], values[2], RegionEndpoint.USEast1);

            try
            {
                DeleteObjectRequest request = new DeleteObjectRequest
                {
                    BucketName = s3BucketName,
                    Key = ImageName
                };
                await awsS3client.DeleteObjectAsync(request);
            } catch(AmazonS3Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return RedirectToAction("DisplayImagesFromS3", "S3Example");
        }

        //function 6: Learn how to download image from S3 to PC (worker and NO UI)
        public async Task<IActionResult> DownloadImage(string ImageName)
        {
            List<String> values = getKeys();
            AmazonS3Client awsS3client = new AmazonS3Client(values[0], values[1], values[2], RegionEndpoint.USEast1);
            Stream imageFile;

            //step 1:
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = s3BucketName,
                    Key = ImageName
                };
                GetObjectResponse response = await awsS3client.GetObjectAsync(request);

                //step 2:
                using(var responseStream = response.ResponseStream)
                {
                    imageFile = new MemoryStream();
                    await responseStream.CopyToAsync(imageFile);
                    imageFile.Position = 0;
                }
            }
            catch(AmazonS3Exception ex)
            {
                return BadRequest(ex.Message);
            }

            //step 3:
            string imagename = Path.GetFileName(ImageName);

            Response.Headers.Add("Content-Disposition", new ContentDisposition
            {
                FileName = imagename,
                Inline = false   //inline = true means direct browse from web browser, inline = false means direct download to the PC
            }.ToString());
            return File(imageFile, "image/jpeg");
        }

        //function 7: Learn how to generate a temporary link for sharing purpose (usage: protect copyright) - (work + UI)
        public async Task<IActionResult> GetTemporaryImage(string ImageName)
        {
            List<String> values = getKeys();
            AmazonS3Client awsS3client = new AmazonS3Client(values[0], values[1], values[2], RegionEndpoint.USEast1);

            try
            {
                GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
                {
                    BucketName = s3BucketName,
                    Key = ImageName,
                    Expires = DateTime.Now.AddMinutes(1)
                };
                ViewBag.presignedURL = awsS3client.GetPreSignedURL(request);
            }
            catch(AmazonS3Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return View();
        }
    
    }
}

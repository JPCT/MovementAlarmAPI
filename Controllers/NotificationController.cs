using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Twilio;
using Twilio.AspNet.Core;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace MovementAlarmAPI
{
    [ApiController]
    [Route("api/notification")]
    public class NotificationController : Controller
    {
        public NotificationController()
        {
        }

        [HttpPost]
        [Route("notify")]
        public string NotifyMovement(String input)
        {
            Environment.SetEnvironmentVariable("turnedOn", "true");
            string messageContent = "Alerta, se detecto movimiento a las " + DateTime.Now;
            var accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
            var authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
            TwilioClient.Init(accountSid, authToken);

            //string fileId = UploadImageToDriveAsync("Images\\test.png", "test2.png").Result;
            //string fileId = UploadImageToDriveAsync(imagePath, imageName).Result;

            var message = MessageResource.Create(
                body: messageContent,
                //mediaUrl: new List<Uri> { new Uri("https://drive.google.com/uc?export=view&id=" + fileId) },
                from: new PhoneNumber("whatsapp:+14155238886"),
                to: new PhoneNumber("whatsapp:+573136367416")
            );

            return message.Status.ToString();
        }

        [HttpPost]
        [Route("receive")]
        public bool ReceiveMessage(string From, string Body)
        {
            Environment.SetEnvironmentVariable("turnedOn", "false");
            return true;
        }

        [HttpGet]
        [Route("turnoff")]
        public HttpStatusCodeResult Turn(HttpRequestMessage request)
        {
            string value = Environment.GetEnvironmentVariable("turnedOn");
            if (value == "true")
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        [HttpGet("upload")]
        public async Task<string> UploadImageToDriveAsync(string imagePath, string imageName)
        {
            UserCredential credential;
            string[] Scopes = { DriveService.Scope.Drive
            };

            using (var stream1 =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream1).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "MovementAlarmAPI",
            });

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = imageName,
                Parents = new List<string>() { }
            };

            string uploadedFileId;
            // Create a new file on Google Drive
            var root = Directory.GetCurrentDirectory();
            var path = Path.Combine(root, imagePath);

            await using (var fsSource = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                // Create a new file, with metadata and stream.
                var request = service.Files.Create(fileMetadata, fsSource, "image/png");
                request.Fields = "*";
                var results = await request.UploadAsync(CancellationToken.None);

                if (results.Status == UploadStatus.Failed)
                {
                    Console.WriteLine($"Error uploading file: {results.Exception.Message}");
                }

                // the file id of the new file we created
                uploadedFileId = request.ResponseBody?.Id;
            }

            var file = service.Files.Get(uploadedFileId);
            Permission permi = new Permission();
            permi.Type = "anyone";
            permi.Role = "writer";
            try
            {
                var x = service.Permissions.Create(permi, uploadedFileId).Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }

            return uploadedFileId;
        }

    }
}

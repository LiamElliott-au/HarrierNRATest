using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Mail;

namespace api
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            MailMessage msg = new MailMessage();
            string infoEmail = Environment.GetEnvironmentVariable("INFO_EMAIL_ADDRESS", EnvironmentVariableTarget.Process);
            string infoName = Environment.GetEnvironmentVariable("INFO_EMAIL_ADDRESS_NAME", EnvironmentVariableTarget.Process);
            string user = Environment.GetEnvironmentVariable("USER", EnvironmentVariableTarget.Process);
            string pwd = Environment.GetEnvironmentVariable("PWD", EnvironmentVariableTarget.Process);
            msg.To.Add(new MailAddress("Liam.Elliott@gmail.com", "Liam Elliott"));
            msg.From = new MailAddress(infoEmail, infoName);
            msg.Subject = "This is a Test Mail";
            msg.Body = "This is a test message using Exchange OnLine";
            msg.IsBodyHtml = true;

            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(user, pwd);
            client.Port = 587; 
            client.Host = "smtp.office365.com";
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            try
            {
                client.Send(msg);
                
            }
            catch (Exception ex)
            {
                return new BadRequestResult();
            }
            return new OkObjectResult("Sent");
        }
    }
}

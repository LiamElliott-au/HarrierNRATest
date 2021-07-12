using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;

namespace api
{
    public static class ContactPost
    {
        [FunctionName("ContactPost")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string name;
            string contactEmail;
            string contactPhone;
            string message;

            if (req.Method.Equals("post", StringComparison.CurrentCultureIgnoreCase))
             {
                try
                {
                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    dynamic data = JsonConvert.DeserializeObject(requestBody);
                    name = data.name_txt;
                    contactEmail = data.email_txt;
                    contactPhone = data.phone_txt;
                    message = data.message_txt;
                }
                catch (Exception e)
                {
                    return new OkObjectResult(e.Message);
                }
            }
            else
            {
               name = req.Query?["name_txt"];
               contactEmail = req.Query?["email_txt"];
               contactPhone = req.Query?["phone_txt"];
               message = req.Query?["message_txt"];
            }
            
            SendGridClient client = new SendGridClient(Environment.GetEnvironmentVariable("SENDGRID_KEY", EnvironmentVariableTarget.Process));

            string infoEmail = Environment.GetEnvironmentVariable("INFO_EMAIL_ADDRESS", EnvironmentVariableTarget.Process);
            string infoName = Environment.GetEnvironmentVariable("INFO_EMAIL_ADDRESS_NAME", EnvironmentVariableTarget.Process);

            var personalisation = new Personalization();
            personalisation.Tos = new List<EmailAddress> { new EmailAddress { Email = infoEmail, Name = infoName } };
            
            SendGridMessage msg = new SendGridMessage();
            msg.Subject = "NRA Contact Request";
            msg.From = new EmailAddress(infoEmail, infoName);
            msg.Personalizations = new List<Personalization> { personalisation };
            
            msg.HtmlContent = $"<p><strong> Contact Name:</strong>  {name}<br /><br /><strong>Contact Email Address:</strong>  {contactEmail}<br /><br /><strong>Contact Phone Number:</strong>  {contactPhone}<br/<br/><strong>Contact Message:</strong>  {message}<p>";

            try
            {
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    return new RedirectResult("/");
                }
                else
                {
                    var body = await response.Body.ReadAsStringAsync();
                    return new OkObjectResult($"Message failed: {body}");
                }
            }
            catch (Exception e)
            {
                return new OkObjectResult($"Message Exception: {e.Message}");
            }
        }
    }
}

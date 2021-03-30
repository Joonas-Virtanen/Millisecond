using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Millisecond
{
    class Program
    {
    
        static async Task Main(string[] args)
        {

            //Create database and tables
            DatabaseHelper.CreateDatabase();
            //// send a batch of messages to the queue
            await ServiceBusHelper.DoAPIPOST();

            //// receive message from the queue
            await ServiceBusHelper.ReceiveMessagesAsync();

            var mail = DatabaseHelper.GetEmailInformation();
            // SendMail(mail).Wait();
        }




        /// <summary>
        /// Send a mail
        /// </summary>
        /// <param name="mail"></param>
        /// <returns></returns>
       private static async Task SendMail(Email mail)
        {
            var apiKey = Environment.GetEnvironmentVariable("xHpX7J0xQyOQdTGWxGrD4Q.fAS0CPKPR91KfEFX4cNH4FTrshVSgGpi0GOYqPPEJTQ");
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("test@mail.com");
            var subject = "Onneksi olkoon";
            var to = new EmailAddress(mail.EmailAddress);
            var plainTextContent = "Moi, \n" +
                "Onneksi olkoon!\n" +
                "Teit 10 attribuuttia!\n" +
                mail.Attributes;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, "");
            var response = await client.SendEmailAsync(msg);
        }
            

        /// <summary>
        /// Handles API request
        /// </summary>
        /// <param name="args">Incoming request</param>
        /// <returns></returns>
      public static async Task HandleAPIRequest(ProcessMessageEventArgs args)
        {
            string json = args.Message.Body.ToString();
            Debug.WriteLine("Received: " + json);
            //Deserialize json
            var jsonObject = JsonConvert.DeserializeObject<TestObject>(json);
            //Distinct Attributes
            jsonObject.Attributes = jsonObject.Attributes.Distinct().ToList();
            //Log
            Logging.Log(jsonObject.Email);
            //Do SQL insert
            var id = DatabaseHelper.InsertEmailMessages(jsonObject.Key, jsonObject.Email);
            foreach (var i in jsonObject.Attributes)
                DatabaseHelper.InsertAttribute(id, i);
            // complete the message. messages is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message);
        }
    }
}



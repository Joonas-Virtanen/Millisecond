using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Millisecond
{
    class ServiceBusHelper
    {
  
        /// <summary>
        /// Receives messages from ServiceBuss
        /// </summary>
        /// <returns></returns>
        public static async Task ReceiveMessagesAsync()
        {
            await using (ServiceBusClient client = new ServiceBusClient(ConfigurationManager.AppSettings["ServiceBus.ConnectionString"], new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets,
                RetryOptions = new ServiceBusRetryOptions
                {
                    TryTimeout = TimeSpan.FromSeconds(1)
                }
            }))
                
            {
                // create a processor that we can use to process the messages
                ServiceBusProcessor processor = client.CreateProcessor(ConfigurationManager.AppSettings["ServiceBus.queueName"], new ServiceBusProcessorOptions() {});
                //  ServiceBusClientOptions.ServiceBusRetryOptions a;

                // add handler to process messages
                processor.ProcessMessageAsync += Program.HandleAPIRequest;

                // add handler to process any errors
                processor.ProcessErrorAsync += ErrorHandler;

                // start processing 
                await processor.StartProcessingAsync();

                Console.WriteLine("Wait for a few seconds and then press any key to end the processing");
                Console.ReadKey();

                // stop processing 
                Console.WriteLine("\nStopping the receiver...");
                await processor.StopProcessingAsync();
                Console.WriteLine("Stopped receiving messages");
            }
            DatabaseHelper.ReadDatabase();
        }

        // handle any errors when receiving messages
        private static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Debug.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        /// <summary>
        /// Does JSON API POST
        /// </summary>
        /// <returns></returns>
        private static Queue<ServiceBusMessage> CreateMessages()
        {
            // create a queue containing the messages and return it to the caller
            Queue<ServiceBusMessage> messages = new Queue<ServiceBusMessage>();
            messages.Enqueue(new ServiceBusMessage(@"{
                ""Key"": 1,
                ""Email"": ""testaaja@gmail.com"",
                ""Attributes"": [""Valkoinen"", ""Sininen""]
            }"));
            messages.Enqueue(new ServiceBusMessage(@"{
                ""Key"": 2,
                ""Email"": ""testaaja2@gmail.com"",
                ""Attributes"": [""Punainen"", ""Punainen""]
            }"));
            messages.Enqueue(new ServiceBusMessage(@"{
                    ""Key"": 5,
                    ""Email"": ""testaaja5@gmail.com"",
                    ""Attributes"": [""Orkidea"", ""Kellanruskea"", ""Peru"", ""Siena"", ""Vaaleanvihreä"", ""Sinivihreä"", ""AliceBlue"", ""Harmaansininen"", ""Vaalea Taivaansininen"", ""Light Blue""]
                }"));
            return messages;
        }

        public static async Task DoAPIPOST()
        {
            // create a Service Bus client 
            await using (ServiceBusClient client = new ServiceBusClient(ConfigurationManager.AppSettings["ServiceBus.ConnectionString"]))
            {
                // create a sender for the queue 
                ServiceBusSender sender = client.CreateSender(ConfigurationManager.AppSettings["ServiceBus.queueName"]);

                // get the messages to be sent to the Service Bus queue
                Queue<ServiceBusMessage> messages = CreateMessages();

                // total number of messages to be sent to the Service Bus queue
                int messageCount = messages.Count;

                // while all messages are not sent to the Service Bus queue
                while (messages.Count > 0)
                {
                    // start a new batch 
                    using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

                    // add the first message to the batch
                    if (messageBatch.TryAddMessage(messages.Peek()))
                    {
                        // dequeue the message from the .NET queue once the message is added to the batch
                        messages.Dequeue();
                    }
                    else
                    {
                        // if the first message can't fit, then it is too large for the batch
                        throw new Exception($"Message {messageCount - messages.Count} is too large and cannot be sent.");
                    }

                    // add as many messages as possible to the current batch
                    while (messages.Count > 0 && messageBatch.TryAddMessage(messages.Peek()))
                    {
                        // dequeue the message from the .NET queue as it has been added to the batch
                        messages.Dequeue();
                    }

                    // now, send the batch
                    await sender.SendMessagesAsync(messageBatch);

                    // if there are any remaining messages in the .NET queue, the while loop repeats 
                }

                Console.WriteLine($"Sent a batch of {messageCount} messages to the topic: {ConfigurationManager.AppSettings["ServiceBus.queueName"]}");
            }
        }
    }
}

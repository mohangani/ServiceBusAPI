using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace ServiceBusAPI.ServiceBus
{
    public class ServiceBusConsumer
    {
        private readonly QueueClient _queueClient;
        private const string QUEUE_NAME = "sbtest";
        private readonly string ConnectionString = "Endpoint=sb://sbquetest.serw3r2vicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ejxMtsx2c8PDC4fjhUfKHewx6be/UjYwOSVuEDFIhUI=;TransportType=AmqpWebSockets";

        public ServiceBusConsumer()
        {
            _queueClient = new QueueClient(ConnectionString, QUEUE_NAME);
        }

        public void RegisterOnMessageHandlerAndReceiveMessages()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };
            HostingEnvironment.QueueBackgroundWorkItem((ct) =>
            {
                // queueClient.RegisterMessageHandler(messageHandler, messageHandlerOptions);
                _queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
            });


        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            //var myPayload = JsonConvert.DeserializeObject<MyPayload>(Encoding.UTF8.GetString(message.Body));
            //_processData.Process(myPayload);
            System.IO.File.AppendAllText(@"E:\DataREceived\" + DateTime.Now.ToString("mmddyyyhhmmssfff") + ".txt", Encoding.UTF8.GetString(message.Body));
            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

        public async Task CloseQueueAsync()
        {
            await _queueClient.CloseAsync();
        }
    }
}
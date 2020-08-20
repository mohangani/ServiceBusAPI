using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace ServiceBusAPI.ServiceBus
{
    public class ServiceBuseQueueReceiver
    {
        public static void StartRecivingFromQueue(QueueClient queueClient, MessageHandlerOptions messageHandlerOptions, Func<Message, CancellationToken, Task> messageHandler)
        {
            HostingEnvironment.QueueBackgroundWorkItem((ct) => {
                queueClient.RegisterMessageHandler(messageHandler, messageHandlerOptions);
            });
        }
    }
}
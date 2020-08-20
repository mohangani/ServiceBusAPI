using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using ServiceBusAPI.ServiceBus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace ServiceBusAPI
{
  
    public class ServiceBusReceiver
    {
        private QueueClient queueClient =
          new QueueClient("Endpoint=sb://sbquetest.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ejxMtsx2c8PDC4fjhUfKHewx6be/UjYwOSVuEDFIhUI=",
                          "sbtest",
                           ReceiveMode.PeekLock
                            )
          {
              //new RetryExponential(TimeSpan.FromSeconds(.5), TimeSpan.FromSeconds(1), 2)
              OperationTimeout = TimeSpan.FromSeconds(10)
          };

        public void Start()
        {

            queueClient.RegisterMessageHandler(ProcessMessagesAsync, new MessageHandlerOptions(MessageBusExceptionHandlerAsync)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 1
            });
            ////Recording Start Time
            Directory.CreateDirectory(@"E:\Service Bus Data Testing");
            File.AppendAllText(@"E:\Service Bus Data Testing\StartTime.txt", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fffffff"));

        }
        public void Stop()
        {
            System.IO.File.AppendAllText(@"E:\Service Bus Data Testing\EndTime.txt", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fffffff") + Environment.NewLine);

           // queueClient.CloseAsync().GetAwaiter().GetResult();
        }
        private Task MessageBusExceptionHandlerAsync(ExceptionReceivedEventArgs args)
        {
            //if (Error == operationTimeoutErrorMessage) 
            //{
            //    //Retry To Get The Message

            //    //After Three Times 

            //    queueClient.CloseAsync();
            //    //Lose the message permanantly
            //    //there is nothing to do after retrying max times
            //    //you cannot save that message
            //    //you cannot get tht message
            //    //close the queueclient and move on to next message
            //    // 
            //}
            //// this will run if the service bus runs into an error
            ///

            Directory.CreateDirectory(@"E:\Service Bus Data Testing\ReceivingFailed\");
            System.IO.File.WriteAllText(@"E:\Service Bus Data Testing\ReceivingFailed\" + DateTime.Now.ToString("mmddyyyHHmmssfff") + "_" + Thread.CurrentThread.ManagedThreadId + ".txt", args.Exception.Message);
            return Task.CompletedTask;
        }

        public async Task<bool> writeText(Message message)
        {
            Directory.CreateDirectory(@"E:\Service Bus Data Testing\Receiver");
            System.IO.File.WriteAllText(@"E:\Service Bus Data Testing\Receiver\" + message.CorrelationId + ".txt", Encoding.UTF8.GetString(message.Body));
            return true;
        }

        public async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {


            using (token.Register(() => queueClient.AbandonAsync(message.SystemProperties.LockToken)))
            {
                //first check hash value 
                if (message.MessageId == GetHashCode(message.Body))
                {
                    await writeText(message);
                    await queueClient.CompleteAsync(message.SystemProperties.LockToken);
                }//Maximum Times it will check for max duration count set in settings of queue and after that it will go to dead letter queue
                else
                {
                    Directory.CreateDirectory(@"E:\Service Bus Data Testing\ReceivingFailed\HasValue Mismatch\");
                    //Writing into hard disk 
                    System.IO.File.WriteAllText(@"E:\Service Bus Data Testing\ReceivingFailed\HasValue Mismatch\" + message.CorrelationId + ".txt", Encoding.UTF8.GetString(message.Body));
                }
            }
            //while (!token.IsCancellationRequested)
            //{
            //    Thread.SpinWait(50000);
            //}
            ////if (token.IsCancellationRequested)
            ////{
            ////    await queueClient.CloseAsync();
            ////}
            //int a = 10, b = 10, numberOfTimesDownloadedAttemted = 0;
            //if (a == b)
            //{
            //    //await writeText(message);//Processing
            //    await queueClient.CompleteAsync(message.SystemProperties.LockToken);
            //}
            //else
            //{
            //    numberOfTimesDownlo0adedAttemted++;
            //    if (numberOfTimesDownloadedAttemted > 3)
            //    {
            //        writeToLog("Message number 52 Failed");

            //        await queueClient.CompleteAsync(message.SystemProperties.LockToken);
            //    }
            //}
            //return messages; // or process it 






        }


        /// <summary>
        /// return SHA256 HashCode
        /// </summary>
        /// <param name="messagebody"></param>
        /// <returns></returns>
        public string GetHashCode(byte[] messagebody)
        {
            // Creating a SHA256  Hash Code 
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                var bytes = sha256Hash.ComputeHash(messagebody);

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder(256);
                foreach (var item in bytes)
                {
                    //calling ToString() OverRide Method to format the byte to String  
                    builder.Append(item.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }

    public class WebApiApplication : System.Web.HttpApplication
    {

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            new ServiceBusReceiver().Start();
        }

        // Close the queue client when app ends
        protected void Application_End()
        {
            new ServiceBusReceiver().Stop();
        }



    }
}

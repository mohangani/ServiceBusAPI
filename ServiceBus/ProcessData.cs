using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ServiceBusAPI.ServiceBus
{
    public interface IProcessData
    {
        void Process(MyPayload myPayload);
    }
    public class ProcessData : IProcessData
    {
        public void Process(MyPayload myPayload)
        {
            
        }
    }

    public class MyPayload
    {

    }
}
using Azure.Core;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitachiQA.Helpers
{
    public class ServiceBus
    {
        private string NameSpaceConnectionString;
        public ServiceBus(string nameSpaceConnStr)
        {
            this.NameSpaceConnectionString = nameSpaceConnStr;

        }

        public static class Queues
        {
            public const string Materializaiton = "bill-processing-materialization";
            public const string BillProcessingPrint = "bill-processing-print";


        }

        public void SendMessage(object message, string queueName, string contentType = "application/json")
        {
            var client = new ServiceBusClient(NameSpaceConnectionString);
            var sender = client.CreateSender(queueName);

            var msg = new ServiceBusMessage(message.ToObject<string>());
            msg.ContentType = contentType;
            sender.SendMessageAsync(msg).Wait();
        }
    }
}

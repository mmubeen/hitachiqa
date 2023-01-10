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

        public void SendMessage(JToken message)
        {
            var client = new ServiceBusClient(NameSpaceConnectionString);
            var sender = client.CreateSender("bill-processing-materialization");

            var msg = new ServiceBusMessage(message.ToString(Newtonsoft.Json.Formatting.None));
            sender.SendMessageAsync(msg).Wait();
        }
    }
}

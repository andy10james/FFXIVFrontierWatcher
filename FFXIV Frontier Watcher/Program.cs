using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using NChronicle.Console.Extensions;
using NChronicle.Core.Model;
using NChronicle.File.Extensions;
using Newtonsoft.Json;

namespace FFXIV_Frontier_Watcher
{

    internal class Program {

        private static void Main (string[] args) {
            NChronicle.Core.NChronicle.Configure
                (c => {
                    c.WithConsoleLibrary();
                    c.WithFileLibrary();
                    //c.WithSmtpLibrary().Configure
                    //(s => {
                    //     s.WithRecipients("to@frontier.com");
                    //     s.WithSender("from@frontier.com");
                    //     s.UsingNetworkMethod("mailtrap.io", 465, "7934d276be13a5", "d75867f8f5c994");
                    //     s.ListeningTo(ChronicleLevel.Success);
                    //     s.AllowingRecurrences();
                    //     s.SendingSynchronously();
                    // });
                });
            var chronicle = new Chronicle();
            while (true)
            {
                PrintFrontierResponse(chronicle);
                Thread.Sleep(2500);
            }
        }

        private static void PrintFrontierResponse(Chronicle chronicle)
        {
            var statuses = GetFrontierResponse();
            if (statuses != null)
            {
                var onlineCount = statuses.Count(s => s.Value == 1);
                var count = statuses.Count;
                var message = $"{statuses.Count(s => s.Value == 1)} out of {statuses.Count} servers are online.";
                if (onlineCount == count) chronicle.Success(message);
                else chronicle.Critical(message);
            }
        }

        private static Dictionary<string, int> GetFrontierResponse()
        {
            var request = WebRequest.Create(ConfigurationManager.AppSettings.Get("statusUri"));
            request.Method = HttpMethod.Get.ToString();
            request.Timeout = 5000;
            var response = request.GetResponse();
            Dictionary<string, int> statuses = null;
            using (var stream = response.GetResponseStream())
            {
                using (var streamReader = new StreamReader(stream))
                {
                    var jsonReader = new JsonTextReader(streamReader);
                    var serializer = new JsonSerializer();
                    statuses = serializer.Deserialize<Dictionary<string, int>>(jsonReader);
                }
            }

            return statuses;
        }
    }

}
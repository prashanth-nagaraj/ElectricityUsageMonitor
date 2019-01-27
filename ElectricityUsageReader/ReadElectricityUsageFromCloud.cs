using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ElectricityUsageReader
{
    internal static class ReadElectricityUsageFromCloud
    {
        private static EventHubClient _eventHubClient;

        private static async Task Main(string[] args)
        {
            var connectionString = "replace";
            Console.WriteLine("Electricty Usage Reader - Reading messages from cloud");
            _eventHubClient = EventHubClient.CreateFromConnectionString(connectionString);
            var runtimeInfo = await _eventHubClient.GetRuntimeInformationAsync();
            var d2cPartitions = runtimeInfo.PartitionIds;

            CancellationTokenSource cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            var tasks = new List<Task>();
            foreach (string partition in d2cPartitions)
            {
                tasks.Add(ReadElectrictyUsageAsync(partition, cts.Token));
            }

            Task.WaitAll(tasks.ToArray());
        }

        private static async Task ReadElectrictyUsageAsync(string partition, CancellationToken ct)
        {
            var eventHubReceiver = _eventHubClient.CreateReceiver("$Default", partition, EventPosition.FromEnqueuedTime(DateTime.Now));
            Console.WriteLine("Create receiver on partition: " + partition);
            while (true)
            {
                if (ct.IsCancellationRequested) break;
                Console.WriteLine("Listening for messages on: " + partition);
                var events = await eventHubReceiver.ReceiveAsync(100);

                if (events == null) continue;

                foreach (EventData eventData in events)
                {
                    string data = Encoding.UTF8.GetString(eventData.Body.Array);
                    Console.WriteLine("Message received on partition {0}:", partition);
                    Console.WriteLine("  {0}:", data);
                }
            }
        }
    }
}
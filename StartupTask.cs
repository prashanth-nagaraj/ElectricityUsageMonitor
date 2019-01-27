using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace ElectricityUsageMonitor
{
    public sealed partial class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        private IElectricityUsageProvider _electricityUsageProvider;
        private DeviceClient _deviceClient;
        private const string connectionString = "replace";
        private const int maxReadings = 5;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Task Started");
            Initialize(taskInstance);
            SendElectricityUsageToCloud();
        }

        private void Initialize(IBackgroundTaskInstance taskInstance)
        {
            _electricityUsageProvider = new SimulatedElectricityUsageProvider();
            _deferral = taskInstance.GetDeferral();
            _deviceClient = DeviceClient.CreateFromConnectionString(connectionString);
        }
        private async void SendElectricityUsageToCloud()
        {
            Debug.WriteLine("Sending electricity usage to cloud");
            double currentUsage;
            for (int i = 0; i < maxReadings; i++)
            {
                currentUsage = _electricityUsageProvider.GetCurrentUsage();

                var electricityUsageData = new
                {
                    DeviceName = "SimulatedElectricityProvider",
                    Time = DateTime.Now.ToString(),
                    CurrentUsage = currentUsage
                };

                var electricityUsageJson = JsonConvert.SerializeObject(electricityUsageData);
                using (var electricityUsage = new Message(Encoding.UTF8.GetBytes(electricityUsageJson)))
                {
                    //On Azure IOT hub to route the messages based on body parameters
                    //content encoding and content type must be specified. ContentEncoding should be utf-8/utf-16/utf-32
                    // and content type must be application/json
                    electricityUsage.ContentEncoding = "utf-8";
                    electricityUsage.ContentType = "application/json";

                    Debug.WriteLine("{0} - Sending usage - {1}", DateTime.Now, electricityUsageJson);
                    await _deviceClient.SendEventAsync(electricityUsage);
                }
                await Task.Delay(1000);
            }

            _deferral.Complete();
        }
    }
}
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Threading;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace led_blink
{
    class Program
    {
        //public static string hubmessage = "empty"; 
        private const int lightTime = 1000;
        private const int dimTime = 200; 
        private const int pin = 18; 
        public static int DesiredLightTime { get; set; } = lighTime;
        public static int DesiredDimTime { get; set; } = dimTime;

        //device twin code from https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-csharp-csharp-module-twin-getstarted
        private const string ModuleConnectionString = "HostName=dev-iotsolution-iothub.azure-devices.net;DeviceId=EdgePi;ModuleId=ledblink;SharedAccessKey=EQ84KKRsUrCUfuOb3lpolFd0vg/y/VHbSOtf8OU/g+Y=";
        private static ModuleClient Client = null;
        static void ConnectionStatusChangeHandler(ConnectionStatus status,
          ConnectionStatusChangeReason reason)
        {
            Console.WriteLine("Connection Status Changed to {0}; the reason is {1}",
              status, reason);
        }
        static void Main(string[] args)
        {
            //var pin = 18;
            //var lightTime = 1000;
            //var dimTime = 200;

            Console.WriteLine($"Let's blink an LED!");
            using GpioController controller = new GpioController();
            controller.OpenPin(pin, PinMode.Output);
            Console.WriteLine($"GPIO pin enabled for use: {pin}");

            //Start Device twin code from https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-csharp-csharp-module-twin-getstarted 
            Microsoft.Azure.Devices.Client.TransportType transport = Microsoft.Azure.Devices.Client.TransportType.Amqp;

            try
            {
                Client = ModuleClient.CreateFromConnectionString(ModuleConnectionString, transport);
                Client.SetConnectionStatusChangesHandler(ConnectionStatusChangeHandler);
                Client.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChanged, null).Wait();

                Console.WriteLine("Retrieving twin");
                var twinTask = Client.GetTwinAsync();
                twinTask.Wait();
                var twin = twinTask.Result;
                Console.WriteLine(JsonConvert.SerializeObject(twin.Properties));

                Console.WriteLine("Sending app start time as reported property");
                TwinCollection reportedProperties = new TwinCollection();
                reportedProperties["DateTimeLastAppLaunch"] = DateTime.Now;

                Client.UpdateReportedPropertiesAsync(reportedProperties);
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Error in sample: {0}", ex);
            }

            Console.WriteLine("Waiting for Events.  Press enter to exit...");
            Console.ReadLine();
            //Client.CloseAsync().Wait();
            // End device twin code from https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-csharp-csharp-module-twin-getstarted

            // turn LED on and off
            while (true)
            {
                Console.WriteLine($"Light for {DesiredLightTime}ms");
                controller.Write(pin, PinValue.High);
                Thread.Sleep(DesiredLightTime);

                Console.WriteLine($"Dim for {DesiredDimTime}ms");
                controller.Write(pin, PinValue.Low);
                Thread.Sleep(DesiredDimTime);
            }
        }

        //device twin code from https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-csharp-csharp-module-twin-getstarted
        private static async Task OnDesiredPropertyChanged(TwinCollection desiredProperties, object userContext)
        {
            Console.WriteLine("desired property change:");
            Console.WriteLine(JsonConvert.SerializeObject(desiredProperties));
            DesiredLightTime = desiredProperties["LightTime"];
            DesiredDimTime = desiredProperties["DimTime"];
            Console.WriteLine("Sending current time as reported property");
            TwinCollection reportedProperties = new TwinCollection
            {
                ["DateTimeLastDesiredPropertyChangeReceived"] = DateTime.Now
            };

            await Client.UpdateReportedPropertiesAsync(reportedProperties).ConfigureAwait(false);
        }
    }


}

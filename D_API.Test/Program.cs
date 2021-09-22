using D_API.Lib;
using DiegoG.Utilities.Settings;
using System;
using System.Threading.Tasks;
using static System.Console;
using static D_API.Test.Helper;
using D_API.Lib.Exceptions;
using DiegoG.Utilities;
using Serilog;

namespace D_API.Test
{
    class Program
    {
        public static D_APIClient Client { get; private set; }

        public static async Task Main()
        {
            Settings<TestSettings>.Initialize(".config", "settings");
            Client = new(Settings<TestSettings>.Current.ClientCredentials ?? throw new NullReferenceException("Client Credentials cannot be null"),
                Settings<TestSettings>.Current.BaseAddress ?? "https://localhost:44379/");

            Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Console().CreateLogger();
            WriteLine("Press enter to start the tests");
            ReadLine();
            await TestAppData();
            //await TestQueue();
            await TestProbe();
            WriteLine("Finished tests, press any key to continue...");
            ReadKey();
        }

        public static async Task TestQueue()
        {
            StoreResult("Entering TestQueue");
            Task[] tasks = new Task[30];
            for (int i = 0; i < 30; i++)
                tasks[i] = Client.Probe();
            await Task.Delay(2000);
            await Task.WhenAll(tasks);
            StoreResult("TestQueue succesful");
            PrintResults();
        }

        public static async Task TestAppData()
        {
            StoreResult("Entering TestAppData");
            await Task.Run(async () =>
            {
                try
                {
                    StoreResult($"Get Test 1: {await Client.GetAppData<string>("Test")}");
                }
                catch (D_APIException exc)
                {
                    StoreResult($"Get Test 1 failed: {exc.Message}");
                }

                try
                {
                    await Client.PostAppData("Test", "papppasd", true);
                    StoreResult($"Post Test 1 (ow:true): PASSED");
                }
                catch (D_APIException exc)
                {
                    StoreResult($"Post Test 1 failed: {exc.Message}");
                }

                try
                {
                    StoreResult($"Get Test 2: {await Client.GetAppData<string>("Test")}");
                }
                catch (D_APIException exc)
                {
                    StoreResult($"Get Test 2 failed: {exc.Message}");
                }

                try
                {
                    await Client.PostAppData("Test", "papppasdadv", false);
                    StoreResult($"Post Test 2 (ow:true): FAILED");
                }
                catch (Exception exc)
                {
                    StoreResult($"Get Test 2: {exc.Message}");
                }

                PrintResults();
            });//.AwaitWithTimeout(10000, null, () => throw new Exception($"Probing took too long to complete"));
        }

        public static async Task TestProbe()
        {
            StoreResult("Entering TestProbe");
            await Task.Run(async () =>
            {
                StoreResult($"Probe: {await Client.Probe()}");
                StoreResult($"ProbeAuth: {await Client.ProbeAuth()}");
                StoreResult($"ProbeAuthMod: {await Client.ProbeAuthMod()}");
                StoreResult($"ProbeAuthAdmin: {await Client.ProbeAuthAdmin()}");
                StoreResult($"ProbeAuthRoot: {await Client.ProbeAuthRoot()}");
                PrintResults();
            });//.AwaitWithTimeout(5000, null, () => throw new Exception($"Probing took too long to complete"));
        }
    }
}
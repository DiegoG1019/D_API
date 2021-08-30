using D_API.Lib;
using DiegoG.Utilities.Settings;
using System;
using System.Threading.Tasks;
using static System.Console;
using static D_API.Test.Helper;
using D_API.Lib.Exceptions;
using DiegoG.Utilities;

namespace D_API.Test
{
    class Program
    {
        public static D_APIClient Client { get; private set; }

        public static async Task Main()
        {
            Settings<TestSettings>.Initialize(".config", "settings");
            Write("Insert APIKey\n> ");
            Client = new(Settings<TestSettings>.Current.BaseAddress ?? "https://localhost:44379/", ReadLine());

            await TestQueue();
            await TestProbe();
            await TestAppData();
        }

        public static async Task TestQueue()
        {
            Task[] tasks = new Task[30];
            for (int i = 0; i < 30; i++)
                tasks[i] = Client.Probe();
            await Task.Delay(2000);
            await Task.WhenAll(tasks);
            WriteLine("TestQueue succesful");
        }

        public static async Task TestAppData()
        {
            await Task.Run(async () =>
            {
                try
                {
                    Helper.StoreResult($"Get Test 1: {await Client.GetAppData<string>("Test")}");
                }
                catch (D_APIException exc)
                {
                    Helper.StoreResult($"Get Test 1 failed: {exc.Message}");
                }

                try
                {
                    await Client.PostAppData("Test", "papppasd", true);
                    Helper.StoreResult($"Post Test 1 (ow:true): PASSED");
                }
                catch (D_APIException exc)
                {
                    Helper.StoreResult($"Post Test 1 failed: {exc.Message}");
                }

                try
                {
                    Helper.StoreResult($"Get Test 2: {await Client.GetAppData<string>("Test")}");
                }
                catch (D_APIException exc)
                {
                    Helper.StoreResult($"Get Test 2 failed: {exc.Message}");
                }

                try
                {
                    await Client.PostAppData("Test", "papppasdadv", false);
                    Helper.StoreResult($"Post Test 2 (ow:true): FAILED");
                }
                catch (Exception exc)
                {
                    Helper.StoreResult($"Get Test 2: {exc.Message}");
                }

                PrintResults();
            }).AwaitWithTimeout(10000, null, () => throw new Exception($"Probing took too long to complete"));
        }

        public static async Task TestProbe()
        {
            await Task.Run(async () =>
            {
                Helper.StoreResult($"Probe: {await Client.Probe()}");
                Helper.StoreResult($"ProbeAuth: {await Client.ProbeAuth()}");
                Helper.StoreResult($"ProbeAuthMod: {await Client.ProbeAuthMod()}");
                Helper.StoreResult($"ProbeAuthAdmin: {await Client.ProbeAuthAdmin()}");
                Helper.StoreResult($"ProbeAuthRoot: {await Client.ProbeAuthRoot()}");
                PrintResults();
            }).AwaitWithTimeout(5000, null, () => throw new Exception($"Probing took too long to complete"));
        }
    }
}

using D_API.Lib;
using DiegoG.Utilities.Settings;
using System.Threading.Tasks;
using static System.Console;
using static D_API.Test.Helper;
using DiegoG.Utilities;
using D_API.Lib.Exceptions;
using System;

namespace D_API.Test
{
    public class Tests
    {
        public static D_APIClient Client { get; private set; }

        public static async Task Main()
        {
            Settings<TestSettings>.Initialize(".config", "settings");
            Write("Insert APIKey\n> ");
            Client = new(Settings<TestSettings>.Current.BaseAddress ?? "https://localhost:44379/", ReadLine());
        }

        public static async Task<bool> TestQueue()
        {
            Task[] tasks = new Task[30];
            for (int i = 0; i < 30; i++)
                tasks[i] = Client.Probe();
            await Task.Delay(2000);
            await Task.WhenAll(tasks);
            return true;
        }

        public static async Task<bool> TestAppData()
        {
            return await Task.Run(async () =>
            {
                try
                {
                    Helper.StoreResult($"Get Test 1: {await Client.GetAppData<string>("Test")}");
                }
                catch(D_APIException exc)
                {
                    Helper.StoreResult($"Get Test 1 failed: {exc.Message}");
                    return false;
                }

                try
                {
                    await Client.PostAppData("Test", "papppasd", true);
                    Helper.StoreResult($"Post Test 1 (ow:true): PASSED");
                }
                catch (D_APIException exc)
                {
                    Helper.StoreResult($"Post Test 1 failed: {exc.Message}");
                    return false;
                }

                try
                {
                    Helper.StoreResult($"Get Test 2: {await Client.GetAppData<string>("Test")}");
                }
                catch (D_APIException exc)
                {
                    Helper.StoreResult($"Get Test 2 failed: {exc.Message}");
                    return false;
                }

                try
                {
                    await Client.PostAppData("Test", "papppasdadv", false);
                    Helper.StoreResult($"Post Test 2 (ow:true): FAILED");
                    return false;
                }
                catch (Exception exc)
                {
                    Helper.StoreResult($"Get Test 2: {exc.Message}");
                }

                return true;
            }).AwaitWithTimeout(10000, null, () => throw new Exception($"Probing took too long to complete"));
        }

        public static async Task<bool> TestProbe()
        {
            return await Task.Run(async () =>
            {
                Helper.StoreResult($"Probe: {await Client.Probe()}");
                Helper.StoreResult($"ProbeAuth: {await Client.ProbeAuth()}");
                Helper.StoreResult($"ProbeAuthMod: {await Client.ProbeAuthMod()}");
                Helper.StoreResult($"ProbeAuthAdmin: {await Client.ProbeAuthAdmin()}");
                Helper.StoreResult($"ProbeAuthRoot: {await Client.ProbeAuthRoot()}");
                return true;
            }).AwaitWithTimeout(5000, null, () => throw new Exception($"Probing took too long to complete"));
        }
    }
}
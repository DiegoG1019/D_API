using D_API.Lib;
using DiegoG.Utilities.Settings;
using NUnit.Framework;
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
        public D_APIClient Client { get; private set; }
        [SetUp]
        public void Setup()
        {
            Settings<TestSettings>.Initialize(".config", "settings");
            Write("Insert APIKey\n> ");
            Client = new(Settings<TestSettings>.Current.BaseAddress ?? "https://localhost:44379/", ReadLine());
        }

        [Test(Author = "Diego Garcia", Description = "Bombard the Rate Limiting Queue to see if it works", ExpectedResult = "The Queue eventually clearing")]
        public async Task TestQueue()
        {

        }

        [Test(Author = "Diego Garcia", Description = "Tests if AppData works as intended", ExpectedResult = "Passing")]
        public async Task TestAppData()
        {
            await Task.Run(async () =>
            {
                try
                {
                    Helper.StoreResult($"Get Test 1: {await Client.GetAppData<string>("Test")}");
                }
                catch(D_APIException exc)
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
            }).AwaitWithTimeout(10000, null, () => Assert.Fail($"Probing took too long to complete"));

            Assert.Pass();
        }

        [Test(Author = "Diego Garcia", Description = "Tests the probing effectiveness of the Client", ExpectedResult = "Passing")]
        public async Task TestProbe()
        {
            await Task.Run(async () =>
            {
                Helper.StoreResult($"Probe: {await Client.Probe()}");
                Helper.StoreResult($"ProbeAuth: {await Client.ProbeAuth()}");
                Helper.StoreResult($"ProbeAuthMod: {await Client.ProbeAuthMod()}");
                Helper.StoreResult($"ProbeAuthAdmin: {await Client.ProbeAuthAdmin()}");
                Helper.StoreResult($"ProbeAuthRoot: {await Client.ProbeAuthRoot()}");
            }).AwaitWithTimeout(5000, null, ()=> Assert.Fail($"Probing took too long to complete"));
            
            Assert.Pass();
        }
    }
}
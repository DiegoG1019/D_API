using D_API.Lib;
using DiegoG.Utilities.Settings;
using NUnit.Framework;
using System.Threading.Tasks;
using static System.Console;
using static D_API.Test.Helper;
using DiegoG.Utilities;

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

        [Test(Author = "Diego Garcia", Description = "Tests the probing capabilities of the Client", ExpectedResult = "Passing")]
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
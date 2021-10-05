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
                Settings<TestSettings>.Current.BaseAddress ?? "https://localhost:44379/", true);

            Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Console().CreateLogger();
            WriteLine("Press enter to start the tests");
            ReadLine();
            await TestUserData();
            WriteLine("Finished tests, press any key to continue...");
            ReadKey();
        }

        public static async Task TestUserData()
        {
        Start:;
            WriteLine("Press enter to start TestUserData");
            ReadLine();
            try
            {
                WriteLine("Entering TestUserData");

                WriteLine($"First Check: {await Client.UserData.CheckAccess("asd")}");

                WriteLine($"Upload: {await Client.UserData.Upload("asd", new byte[] { 0, 1, 2, 3, 4, 5 }, true)}");

                WriteLine($"Second Check: {await Client.UserData.CheckAccess("asd")}");

                WriteLine($"Download: {string.Join(',', await Client.UserData.Download("asd"))}");

                WriteLine($"Transfer Report: {await Client.UserData.GetTransferReport()}");

                WriteLine("TestUserData succesful");
            }
            catch(Exception e)
            {
                WriteLine($"Failed Test with {e}");
                goto Start;
            }
        }
    }
}
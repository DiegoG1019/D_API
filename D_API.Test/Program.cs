using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static System.Console;

namespace D_API.Test
{
    static class Program
    {
        public static HttpClient Http { get; } = new HttpClient { BaseAddress = new("https://localhost:44379/") };

        static async Task Main(string[] args)
        {
            Write("Input API Key\n> ");
            var k = ReadLine();

            Http.DefaultRequestHeaders.Authorization = new("Bearer", k);
            var t = TestProbe();
            await TestAppDataHost();
            await t;

            Write("Press any key to continue...");
            ReadKey();
        }

        static async Task TestProbe()
        {
            var r1_t = Http.GetAsync("api/test/probe");
            var r2_t = Http.GetAsync("api/test/probeAuth");
            var r3_t = Http.GetAsync("api/test/probeAuthMod");

            StoreResult((await r1_t).StatusCode);
            StoreResult((await r2_t).StatusCode);
            StoreResult((await r3_t).StatusCode);

            int i = 0;
            foreach(var x in GetResults())
                WriteLine($"TestProbe Result {++i}: {x}");
        }

        static async Task TestAppDataHost()
        {
            StoreResult((await Http.GetAsync("api/v1/appdatahost/config/test")).StatusCode);
            StoreResult((await Http.PostAsJsonAsync("api/v1/appdatahost/config/test", "aaaaaaaaaaaaaaaaaa")).StatusCode);
            StoreResult(await (await Http.GetAsync("api/v1/appdatahost/config/test")).Content.ReadAsStringAsync());
            StoreResult((await Http.PostAsJsonAsync("api/v1/appdatahost/config/test/true", "aaaaaaaaaaaaaaaaaa")).StatusCode);

            int i = 0;
            foreach (var x in GetResults())
                WriteLine($"TestAppDataHost Result {++i}: {x}");
        }

        #region Utilities
        readonly static Dictionary<string, List<object>> ResultsDictionary = new();
        static void CheckCaller(string caller)
        {
            if (caller is null)
                throw new ArgumentNullException(nameof(caller));
        }
        static void StoreResult(object result, [CallerMemberName] string caller = null)
        {
            CheckCaller(caller);
            if (!ResultsDictionary.ContainsKey(caller))
                ResultsDictionary[caller] = new();
            ResultsDictionary[caller].Add(result);
        }

        static IEnumerable<object> GetResults([CallerMemberName] string caller = null)
        {
            CheckCaller(caller);
            var r = ResultsDictionary[caller];
            ResultsDictionary.Remove(caller);
            return r;
        }
        #endregion
    }
}

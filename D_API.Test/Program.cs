﻿using System;
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
            WriteLine("Input API Key\n> ");
            var k = ReadLine(); ;
            Http.DefaultRequestHeaders.Authorization = new("Bearer", k);
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

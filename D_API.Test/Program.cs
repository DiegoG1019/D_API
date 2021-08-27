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
            WriteLine("Input API Key\n> ");
            var k = ReadLine(); ;
            Http.DefaultRequestHeaders.Authorization = new("Bearer", k);
        }
    }
}

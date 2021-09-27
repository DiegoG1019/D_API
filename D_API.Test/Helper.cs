using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace D_API.Test
{
    public static class Helper
    {
        #region Utilities
        readonly static Dictionary<string, List<object>> ResultsDictionary = new();
        private static void CheckCaller(string caller)
        {
            if (caller is null)
                throw new ArgumentNullException(nameof(caller));
        }

        public static void StoreResult<T>(this T result, Func<T, string> data, [CallerMemberName] string? caller = null)
            => StoreResult(data(result), caller);

        public static void StoreResult(object result, [CallerMemberName] string? caller = null)
        {
            CheckCaller(caller ??= ".");
            if (!ResultsDictionary.ContainsKey(caller))
                ResultsDictionary[caller] = new();
            ResultsDictionary[caller].Add(result);
        }

        public static IEnumerable<object> GetResults([CallerMemberName] string? caller = null)
        {
            CheckCaller(caller ??= ".");
            var r = ResultsDictionary[caller];
            ResultsDictionary.Remove(caller);
            return r;
        }

        public static void PrintResults([CallerMemberName] string? caller = null)
        {
            foreach (var x in GetResults(caller))
                Console.WriteLine(x);
        }
        #endregion
    }
}

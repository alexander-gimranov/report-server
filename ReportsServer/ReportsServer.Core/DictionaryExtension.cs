using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace ReportsServer.Core
{
    public static class DictionaryExtension
    {
        public static string ToStr<T>(this T source) where T: IDictionary<string, StringValues>
        {
            return string.Join(";", source.Select(x => x.Key + "=" + x.Value).ToArray());
        }
    }
}
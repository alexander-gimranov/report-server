using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReportsServer.REST.Classes;

namespace ReportsServer.REST
{
    public class RestRVisionService : RestService
    {
        public const string GetResourceProperties = "get-resource-properties";

        public override async Task FillFieldFormRest(IReadOnlyDictionary<string, string> cookies,
            Dictionary<string, Dictionary<string, object>> collection, string field)
        {
            var list = ConverToNetCookies(cookies);
            var json = JsonConvert.SerializeObject(
                new
                {
                    resourcePropertyQuery = new
                    {
                        resourceIds = collection.Keys.ToArray(),
                        // todo properties mapper
                        selectedProperties = new string[] { "Position in the workbook" }
                    }
                }
            );

            var result = await GetContentAsResultAsync<APIResourceDataSuccessResult>(
                HttpMethod.Post, list, GetResourceProperties, null, json, true);
            FillFieldOfObjects(result, ref collection, field);
        }

        private void FillFieldOfObjects(APIResult<APIResourceDataSuccessResult> result,
            ref Dictionary<string, Dictionary<string, object>> objects, string field)
        {
            if (result.Success)
            {
                foreach (var data in result.Result.Data)
                {
                    if (data.PropertiesData.Any() &&
                        objects.TryGetValue(data.ResourceId.ToString(), out var obj))
                    {
                        var value = data.PropertiesData.First().Values.FirstOrDefault();
                        if (!string.IsNullOrEmpty(value))
                        {
                            obj[field] = value;
                        }
                    }
                }
            }
        }
    }
}
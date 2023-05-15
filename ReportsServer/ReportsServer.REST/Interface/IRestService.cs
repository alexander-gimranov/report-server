using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ReportsServer.REST.Classes;
using ReportsServer.REST.Config.Interface;
using ReportsServer.REST.Implementation;

namespace ReportsServer.REST.Interface
{
    public interface IRestService
    {
        void Initialize(string host, IRestServiceConfig config);

        IList<Cookie> ConverToNetCookies(IReadOnlyDictionary<string, string> cookies);
        void PrepareCookies(ref IList<Cookie> cookies);
        void FilterCookies(ref IList<Cookie> cookies);

        T GetParameterValue<T>(string name);

        RestClient CreateRestClient(IList<Cookie> cookies, string method, string queryString = null);

        Task<APIResult<TResult>> GetContentAsResultAsync<TResult>(HttpMethod httpMethod, IList<Cookie> cookies, string method,
            string queryString = null, string json = null,
            bool acceptJson = true, bool noCache = false);

        Task<APIResult<TResult>> GetContentAsResultAsync<TResult>(HttpMethod httpMethod, IList<Cookie> cookies, string method,
            HttpContent httpContent, bool acceptJson = false, bool noCache = false);

        Task<HttpContent> GetContentAsync(HttpMethod httpMethod, IList<Cookie> cookies, string method,
            string queryString = null, string json = null,
            bool acceptJson = true, bool noCache = false);

        Task<HttpContent> GetContentAsync(HttpMethod httpMethod, IList<Cookie> cookies, string method,
            HttpContent content, bool acceptJson = false, bool noCache = false);

        Task<List<string>> GetUserPermissionsAsync(IReadOnlyDictionary<string, string> cookies);

        Task FillFieldFormRest(IReadOnlyDictionary<string, string> cookies,
            Dictionary<string, Dictionary<string, object>> collection, string field);
    }
}
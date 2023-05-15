using Newtonsoft.Json;
using ReportsServer.REST.Classes;
using ReportsServer.REST.Config.Interface;
using ReportsServer.REST.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ReportsServer.REST.Implementation;
using BaseAPIResult = ReportsServer.REST.Classes.BaseAPIResult;

namespace ReportsServer.REST
{
    public class RestService : IRestService
    {
        protected Uri _baseAddress;
        protected IRestServiceConfig _config;

        public void Initialize(string host, IRestServiceConfig config)
        {
            _baseAddress = new Uri(new Uri(host), config.RelativeUrl);
            _config = config;
        }

        public IList<Cookie> ConverToNetCookies(IReadOnlyDictionary<string, string> cookies)
        {
            return cookies.Select(cookie => new Cookie(cookie.Key, cookie.Value)).ToList();
        }

        public T GetParameterValue<T>(string name)
        {
            return _config.GetParameterValue<T>(name);
        }

        public virtual void PrepareCookies(ref IList<Cookie> cookies)
        {
            if (cookies == null) return;
            foreach (var cookie in (List<Cookie>)cookies)
            {
                if (string.IsNullOrEmpty(cookie.Domain))
                    cookie.Domain = _baseAddress.DnsSafeHost;
                cookie.Path = _config.CoockiePath;
            }
        }

        public virtual void FilterCookies(ref IList<Cookie> cookies)
        {
            var fltCookie = _config.GetParameterValue<string[]>("cookies");
            if (fltCookie?.Any() ?? false)
            {
                cookies = cookies?.Where(c => fltCookie.Contains(c.Name)).ToList();
            }
        }

        public RestClient CreateRestClient(IList<Cookie> cookies, string method, string queryString = null)
        {
            PrepareCookies(ref cookies);
            FilterCookies(ref cookies);
            return new RestClient(_baseAddress, string.Format("{0}{1}", _config.GetMethod(method), queryString), cookies);
        }

        public virtual async Task<APIResult<TResult>> GetContentAsResultAsync<TResult>(HttpMethod httpMethod,
            IList<Cookie> cookies, string method, string queryString = null, string json = null, bool acceptJson = true, bool noCache = false)
        {
            var actionResult = new APIResult<TResult>();
            var content = await GetContentAsync(httpMethod, cookies, method, queryString, json, acceptJson, noCache);
            if (content != null)
            {
                var apiResult = await content.ReadAsStringAsync();
                if (apiResult != null)
                {
                    var apiShortResult = JsonConvert.DeserializeObject<BaseAPIResult>(apiResult);

                    if (!apiShortResult.Success)
                    {
                        var apiFailResult = JsonConvert.DeserializeObject<APIFailResult>(apiResult);
                        actionResult.ErrorMessage = apiFailResult.StatusMsg;
                    }
                    else
                    {
                        actionResult.Success = true;
                        actionResult.Result = JsonConvert.DeserializeObject<TResult>(apiResult);
                    }
                }
            }

            return actionResult;
        }

        public async Task<APIResult<TResult>> GetContentAsResultAsync<TResult>(HttpMethod httpMethod, IList<Cookie> cookies,
            string method, HttpContent httpContent,
            bool acceptJson = false, bool noCache = false)
        {
            var actionResult = new APIResult<TResult>();
            var content = await GetContentAsync(httpMethod, cookies, method, httpContent, acceptJson, noCache);
            if (content != null)
            {
                var apiResult = await content.ReadAsStringAsync();
                if (apiResult != null)
                {
                    var apiShortResult = JsonConvert.DeserializeObject<BaseAPIResult>(apiResult);

                    if (!apiShortResult.Success)
                    {
                        var apiFailResult = JsonConvert.DeserializeObject<APIFailResult>(apiResult);
                        actionResult.ErrorMessage = apiFailResult.StatusMsg;
                    }
                    else
                    {
                        actionResult.Success = true;
                        actionResult.Result = JsonConvert.DeserializeObject<TResult>(apiResult);
                    }
                }
            }

            return actionResult;
        }

        public Task<HttpContent> GetContentAsync(HttpMethod httpMethod, IList<Cookie> cookies, string method,
            string queryString = null, string json = null, bool acceptJson = true, bool noCache = false)
        {
            var httpContent = !string.IsNullOrEmpty(json)
                ? new StringContent(json, Encoding.UTF8, "application/json")
                : null;
            return GetContentAsync(httpMethod, cookies, method, httpContent, acceptJson, noCache);
        }

        public Task<HttpContent> GetContentAsync(HttpMethod httpMethod, IList<Cookie> cookies, string method,
            HttpContent content, bool acceptJson = false, bool noCache = false)
        {
            var restClient = CreateRestClient(cookies, method);
            return restClient.GetContentAsync(httpMethod, content, acceptJson, noCache);
        }

        public virtual Task<List<string>> GetUserPermissionsAsync(IReadOnlyDictionary<string, string> cookies)
        {
            throw new NotImplementedException();
        }

        public virtual Task FillFieldFormRest(IReadOnlyDictionary<string, string> cookies,
            Dictionary<string, Dictionary<string, object>> collection, string field)
        {
            throw new NotImplementedException();
        }

    }
}
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ReportsServer.REST.Implementation
{
    public class RestClient
    {
        private readonly Uri _baseAddress;
        private readonly string _apiMethod;
        private readonly IList<Cookie> _cookies;

        public RestClient(Uri baseUri, string method, IList<Cookie> cookies)
        {
            _baseAddress = baseUri;
            _apiMethod = method;
            _cookies = cookies;
        }

        public HttpClient CreateClient(HttpClientHandler handler, bool acceptJson = false, bool noCache = false)
        {
            var client = new HttpClient(handler) {BaseAddress = _baseAddress};
            client.DefaultRequestHeaders.Accept.Clear();
            if (acceptJson)
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (noCache) client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue {NoCache = true};
            return client;
        }

        private HttpClientHandler CreateHandler(CookieContainer cookieContainer = null)
        {
            if (cookieContainer == null) cookieContainer = new CookieContainer();
            ((List<Cookie>) _cookies)?.ForEach(cookieContainer.Add);

            return new HttpClientHandler {CookieContainer = cookieContainer};
        }

        public async Task<string> CreatePostRequest(HttpContent content)
        {
            var responseContent = await GetContentAsync(HttpMethod.Post, content, false, false);
            return responseContent != null ? await responseContent.ReadAsStringAsync() : null;
        }

        public async Task<string> CreateGetRequest()
        {
            var content = await GetContentAsync(HttpMethod.Get);
            return content != null ? await content.ReadAsStringAsync() : null;
        }

        public async Task<HttpResponseMessage> CreateResponseAsync(HttpMethod method, HttpContent content = null,
            bool acceptJson = true, bool noCache = false)
        {
            var cookiesContainer = new CookieContainer();
            using (var clientHandler = CreateHandler(cookiesContainer))
            {
                using (var client = CreateClient(clientHandler, acceptJson, noCache))
                {
                    HttpResponseMessage response;
                    if (method == HttpMethod.Post)
                    {
                        response = await client.PostAsync(_apiMethod, content);
                    }
                    else if (method == HttpMethod.Put)
                    {
                        response = await client.PutAsync(_apiMethod, content);
                    }
                    else
                    {
                        response = await client.GetAsync(_apiMethod);
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        return response;
                    }

                    return null;
                }
            }
        }

        public async Task<HttpContent> GetContentAsync(HttpMethod method, HttpContent content = null,
            bool acceptJson = true, bool noCache = false)
        {
            var response = await CreateResponseAsync(method, content, acceptJson, noCache);
            if (response != null) return response.Content;
            return null;
        }

    }
}
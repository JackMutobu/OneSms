using OneSms.Web.Shared.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneSms.Droid.Server.Services
{
    public class HttpClientService
    {
        private readonly HttpClient _httpClient;
        public HttpClientService(string url)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new System.Uri(url)
            };
        }

        public HttpClient HttpClient => _httpClient;

        public async Task<Result<T>> GetAsync<T>(object content, string uri) where T : class
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var json = JsonSerializer.Serialize(content);
            using var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            request.Content = stringContent;

            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);
            return await GetResponse<T>(response);
        }

        public async Task<Result<T>> PostAsync<T>(object content, string uri) where T : class
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, uri);
            var json = JsonSerializer.Serialize(content);
            using var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            request.Content = stringContent;

            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);
            return await GetResponse<T>(response);
        }

        public async Task<Result<T>> PutAsync<T>(object content, string uri) where T : class
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, uri);
            var json = JsonSerializer.Serialize(content);
            using var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            request.Content = stringContent;

            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);
            return await GetResponse<T>(response);
        }

        private async Task<Result<T>> GetResponse<T>(HttpResponseMessage response) where T : class
        {
            var message = await response.Content.ReadAsStringAsync();
            Result<T> result;
            if (response.IsSuccessStatusCode)
            {
                var value = typeof(T) != "string".GetType() ? JsonSerializer.Deserialize<T>(message) : message as T;
                result = new Result<T>(response.StatusCode, value);
            }
            else
                result = new Result<T>(response.StatusCode, await response.Content.ReadAsStringAsync());
            return result;
        }

    }
}
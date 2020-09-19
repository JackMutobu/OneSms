using Microsoft.AspNetCore.Http;
using OneSms.Contracts.V1;
using OneSms.Contracts.V1.Requests;
using OneSms.Contracts.V1.Responses;
using OneSms.Droid.Server.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneSms.Droid.Server.Services
{
    public interface IHttpClientService
    {
        HttpClient HttpClient { get; }

        Task<Result<T>> GetAsync<T>(object content, string uri) where T : class;
        Task<Result<T>> PostAsync<T>(object content, string uri) where T : class;
        Task<Result<T>> PutAsync<T>(object content, string uri) where T : class;
        Task<FileUploadSuccessReponse> UploadImage(IFormFile formFile);
        void SetAuthorizationHeaderToken(string token);
        Task<AuthSuccessResponse> Authenticate(ServerAuthRequest model);
        void ChangeBaseAdresse(Uri uri);
    }

    public class HttpClientService : IHttpClientService
    {
        public HttpClientService(string url)
        {
            HttpClient = new HttpClient
            {
                BaseAddress = new Uri(url)
            };
        }

        public HttpClient HttpClient { get; private set; }

        public void ChangeBaseAdresse(Uri uri)
        {
            HttpClient = new HttpClient
            {
                BaseAddress = uri
            };
        }

        public void SetAuthorizationHeaderToken(string token)
        {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<AuthSuccessResponse> Authenticate(ServerAuthRequest model)
        {
            var data = JsonSerializer.Serialize(model);
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync(ApiRoutes.Auth.Server, content);
            if (response.IsSuccessStatusCode)
            {
                var stringResponse = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AuthSuccessResponse>(stringResponse, new JsonSerializerOptions {PropertyNameCaseInsensitive = true });
                return result;
            }

            throw new Exception(await response.Content.ReadAsStringAsync());
        }

        public async Task<Result<T>> GetAsync<T>(object content, string uri) where T : class
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var json = JsonSerializer.Serialize(content);
            using var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            request.Content = stringContent;

            using var response = await HttpClient
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

            using var response = await HttpClient
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

            using var response = await HttpClient
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

        public async Task<FileUploadSuccessReponse> UploadImage(IFormFile file)
        {
            using var content = new MultipartFormDataContent
            {
                {
                    new StreamContent(file.OpenReadStream())
                    {
                        Headers =
                {
                    ContentLength = file.Length,
                    ContentType = new MediaTypeHeaderValue(file.ContentType)
                }
                    },
                    "formImage",
                    file.FileName
                }
            };

            var response = await HttpClient.PostAsync(ApiRoutes.Upload.Image, content);
            if (response.IsSuccessStatusCode)
            {
                var stringResult = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FileUploadSuccessReponse>(stringResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result;
            }
            else
                throw new Exception(await response.Content.ReadAsStringAsync());
        }

    }
}
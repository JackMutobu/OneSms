using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reactive.Linq;

namespace OneSms.Services
{
    public class TimService
    {
        private IHttpClientFactory _clientFactory;
        private IConfiguration _configuration;

        public TimService(IHttpClientFactory clientFactory,IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        public void StartTimer()
        {
            Observable.Interval(TimeSpan.FromSeconds(60))
             .Subscribe(async _ => 
             {
                 var client = _clientFactory.CreateClient();
                 var url = _configuration!.GetSection("Url").Value;
                 var request = new HttpRequestMessage(HttpMethod.Get, $"{url}api/Tim/activate");
                 var response = await client.SendAsync(request);
                 if (response.IsSuccessStatusCode)
                     Debug.WriteLine("Activation done");
                 else
                     Debug.WriteLine("Activation failed");
             });
        }
        
    }
}

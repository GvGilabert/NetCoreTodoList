using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AspNetCoreTodo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AspNetCoreTodo.Services
{
    public class GetMapInfo
    {
        private readonly IConfiguration Configuration;

        public GetMapInfo(IConfiguration config)
        {
            Configuration = config;
        }

        public async Task<info> CallApiAsync(string address)
        {
            var client = new HttpClient();
            var url = new System.Uri(Configuration.GetSection("GeoLocationApi").GetSection("BaseURLForSearch").Value+
                                     address +
                                     Configuration.GetSection("GeoLocationApi").GetSection("UrlEndForSearch").Value);
            var response = await client.GetAsync(url);
            string json;

            using (var content = response.Content)
            json = await content.ReadAsStringAsync();
            info addresses = JsonConvert.DeserializeObject<info>(json);
            return addresses;
        }
        public string CallApiMap(string[] address)
        {
            string finalAddress ="";
            foreach(string item in address)
            {
                finalAddress += (item +"||");
            }        
            return (finalAddress=="") ? Configuration.GetSection("GeoLocationApi").GetSection("DefaultURL").Value :
            Configuration.GetSection("GeoLocationApi").GetSection("BaseUrlForStaticMap").Value
            + finalAddress + "&size=@2x&key=" + Configuration.GetSection("GeoLocationApi").GetSection("ApiKey").Value;
        }
    }
}
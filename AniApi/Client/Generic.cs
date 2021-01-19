using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AniApi.Client
{
    public class Generic
    {
        private readonly HttpClient httpClient;
        public readonly NavigationManager navigationManager;

        //Costruttore
        public Generic(NavigationManager _navigationManager,
                HttpClient _httpClient)
        {
            navigationManager = _navigationManager;
            httpClient = _httpClient;
        }
        public async Task<T> PostSingleRequest<T, Z>(string urlApi, Z data) where T : new()
        {
            T res = new T();
            try
            {
                string absoluteUrl = string.Format("{0}{1}", httpClient.BaseAddress, urlApi);
                HttpResponseMessage responsePost = await httpClient.PostAsJsonAsync<Z>(absoluteUrl, data);
                res = JsonConvert.DeserializeObject<T>(await responsePost.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                //SetErrorData(res, 500, ex.Message);
            }

            return res;
        }

        public async Task<List<T>> PostListRequest<T, Z>(string urlApi, Z data) where T : class, new()
        {
            List<T> res = new List<T>();

            try
            {
                string absoluteUrl = string.Format("{0}{1}", httpClient.BaseAddress, urlApi);
                HttpResponseMessage responsePost = await httpClient.PostAsJsonAsync<Z>(absoluteUrl, data);
                res = JsonConvert.DeserializeObject<List<T>>(await responsePost.Content.ReadAsStringAsync());

            }
            catch (Exception)
            { }

            return res;
        }

        public async Task<T> GetSingleRequest<T>(string urlApi) where T : class, new()
        {
            T res = new T();

            try
            {
                string absoluteUrl = string.Format("{0}{1}", httpClient.BaseAddress, urlApi);
                HttpResponseMessage responseGet = await httpClient.GetAsync(absoluteUrl);
                res = JsonConvert.DeserializeObject<T>(await responseGet.Content.ReadAsStringAsync());

            }
            catch (Exception ex)
            {
                //SetErrorData(res, 500, ex.Message);
            }

            return res;
        }

        public async Task<List<T>> GetListRequest<T>(string urlApi) where T : class, new()
        {
            List<T> res = new List<T>();

            try
            {
                string absoluteUrl = string.Format("{0}{1}", httpClient.BaseAddress, urlApi);
                HttpResponseMessage responseGet = await httpClient.GetAsync(absoluteUrl);
                res = JsonConvert.DeserializeObject<List<T>>(await responseGet.Content.ReadAsStringAsync());

            }
            catch (Exception)
            { }

            return res;
        }
    }
}

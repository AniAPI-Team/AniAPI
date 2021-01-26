using Commons;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebApp
{
    public class Generic
    {
        private readonly HttpClient _httpClient;
        public readonly NavigationManager _navigationManager;

        //Costruttore
        public Generic(NavigationManager navigationManager, HttpClient httpClient)
        {
            this._navigationManager = navigationManager;
            this._httpClient = httpClient;
        }
        public async Task<T> PostSingleRequest<T, Z>(string urlApi, Z data) where T : new()
        {
            T res = new T();
            
            try
            {
                JsonSerializerOptions jso = new JsonSerializerOptions()
                {
                    IgnoreNullValues = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string absoluteUrl = string.Format("{0}{1}", this._httpClient.BaseAddress, urlApi);
                HttpResponseMessage responsePost = await this._httpClient.PostAsJsonAsync<Z>(absoluteUrl, data, jso);
                res = JsonConvert.DeserializeObject<T>(await responsePost.Content.ReadAsStringAsync());

            }
            catch (Exception ex)
            {
                ApiResponseManager.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return res;
        }

        public async Task<List<T>> PostListRequest<T, Z>(string urlApi, Z data) where T : class, new()
        {
            List<T> res = new List<T>();

            try
            {
                JsonSerializerOptions jso = new JsonSerializerOptions()
                {
                    IgnoreNullValues = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string absoluteUrl = string.Format("{0}{1}", this._httpClient.BaseAddress, urlApi);
                HttpResponseMessage responsePost = await this._httpClient.PostAsJsonAsync<Z>(absoluteUrl, data, jso);
                res = JsonConvert.DeserializeObject<List<T>>(await responsePost.Content.ReadAsStringAsync());

            }
            catch (Exception ex)
            {
                ApiResponseManager.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return res;
        }

        public async Task<T> GetSingleRequest<T>(string urlApi) where T : class, new()
        {
            T res = new T();

            try
            {
                JsonSerializerOptions jso = new JsonSerializerOptions()
                {
                    IgnoreNullValues = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                res = await _httpClient.GetFromJsonAsync<T>(urlApi, jso);
            }
            catch (Exception ex)
            {
                ApiResponseManager.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return res;
        }

        public async Task<List<T>> GetListRequest<T>(string urlApi) where T : class, new()
        {
            List<T> res = new List<T>();

            try
            {
                JsonSerializerOptions jso = new JsonSerializerOptions()
                {
                    IgnoreNullValues = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                res = await _httpClient.GetFromJsonAsync<List<T>>(urlApi, jso);
            }
            catch (Exception ex)
            {
                ApiResponseManager.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return res;
        }
    }
}

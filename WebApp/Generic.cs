using Blazored.LocalStorage;
using Commons;
using Commons.Enums;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebApp
{
    public class Generic
    {
        #region Injection
        protected SpinnerService Spinner { get; set; }
        protected HttpClient Client { get; set; }
        protected ISyncLocalStorageService LocalStorage { get; set; }
        #endregion

        public Generic (SpinnerService _spinner, HttpClient _client, ISyncLocalStorageService _localStorage)
        {
            Spinner = _spinner;
            Client = _client;
            LocalStorage = _localStorage;
        }

        /// <summary>
        /// Post to Server Data and receive an Object with type defined in parameters
        /// </summary>
        /// <typeparam name="T">Data Type of server response</typeparam>
        /// <typeparam name="Z">Data Type of server request</typeparam>
        /// <param name="urlApi">Relative path to Server API</param>
        /// <param name="data">Data to be sent to Server</param>
        /// <param name="useSpinner">Activate the spinner while calling the server</param>
        /// <returns>Object with type defined in parameters</returns>
        public async Task<T> PostSingleRequest<T, Z>(string urlApi, Z data, bool useSpinner = false) where T : new()
        {
            if (useSpinner)
                Spinner.Show();
            T res = new T();

            try
            {
                JsonSerializerOptions jso = new JsonSerializerOptions()
                {
                    IgnoreNullValues = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string absoluteUrl = string.Format("{0}{1}", Client.BaseAddress, urlApi);
                HttpResponseMessage responsePost = await Client.PostAsJsonAsync<Z>(absoluteUrl, data, jso);
                res = JsonConvert.DeserializeObject<T>(await responsePost.Content.ReadAsStringAsync());

            }
            catch (APIException ex)
            {
                APIManager.ErrorResponse(ex);
            }
            catch (Exception)
            {
                throw;
                //ApiManager.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {
                if (useSpinner)
                    Spinner.Hide();
            }

            return res;
        }

        /// <summary>
        /// Post to Server Data and receive a List Object with type defined in parameters
        /// </summary>
        /// <typeparam name="T">Data Type of server response</typeparam>
        /// <typeparam name="Z">Data Type of server request</typeparam>
        /// <param name="urlApi">Relative path to Server API</param>
        /// <param name="data">Data to be sent to Server</param>
        /// <param name="useSpinner">Activate the spinner while calling the server</param>
        /// <returns>List of Object with type defined in parameters</returns>
        public async Task<List<T>> PostListRequest<T, Z>(string urlApi, Z data, bool useSpinner = false) where T : class, new()
        {
            if (useSpinner)
                Spinner.Show();
            List<T> res = new List<T>();

            try
            {
                JsonSerializerOptions jso = new JsonSerializerOptions()
                {
                    IgnoreNullValues = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string absoluteUrl = string.Format("{0}{1}", Client.BaseAddress, urlApi);
                HttpResponseMessage responsePost = await Client.PostAsJsonAsync<Z>(absoluteUrl, data, jso);
                res = JsonConvert.DeserializeObject<List<T>>(await responsePost.Content.ReadAsStringAsync());

            }
            catch (APIException ex)
            {
                APIManager.ErrorResponse(ex);
            }
            catch (Exception)
            {
                throw;
                //ApiManager.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {
                if (useSpinner)
                    Spinner.Hide();
            }

            return res;
        }

        /// <summary>
        /// Get Data (Object with type defined in parameters) from Server
        /// </summary>
        /// <typeparam name="T">Data Type of server response</typeparam>
        /// <param name="urlApi">Relative path to Server API</param>
        /// <param name="useSpinner">Activate the spinner while calling the server</param>
        /// <returns>Object with type defined in parameters</returns>
        public async Task<T> GetSingleRequest<T>(string urlApi, bool useSpinner = false) where T : class, new()
        {
            if (useSpinner)
                Spinner.Show();
            T res = new T();

            try
            {
                JsonSerializerOptions jso = new JsonSerializerOptions()
                {
                    IgnoreNullValues = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                res = await Client.GetFromJsonAsync<T>(urlApi, jso);
            }
            catch (APIException ex)
            {
                APIManager.ErrorResponse(ex);
            }
            catch (Exception)
            {
                throw;
                //ApiManager.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {
                if (useSpinner)
                    Spinner.Hide();
            }

            return res;
        }

        /// <summary>
        /// Get Data (List of Object with type defined in parameters) from Server
        /// </summary>
        /// <typeparam name="T">Data Type of server response</typeparam>
        /// <param name="urlApi">Relative path to Server API</param>
        /// <param name="useSpinner">Activate the spinner while calling the server</param>
        /// <returns>List fo Object with type defined in parameters</returns>
        public async Task<List<T>> GetListRequest<T>(string urlApi, bool useSpinner = false) where T : class, new()
        {
            if (useSpinner)
                Spinner.Show();
            List<T> res = new List<T>();

            try
            {
                JsonSerializerOptions jso = new JsonSerializerOptions()
                {
                    IgnoreNullValues = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                res = await Client.GetFromJsonAsync<List<T>>(urlApi, jso);
            }
            catch (APIException ex)
            {
                APIManager.ErrorResponse(ex);
            }
            catch (Exception)
            {
                throw;
                //ApiManager.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {
                if (useSpinner)
                    Spinner.Hide();
            }

            return res;
        }

        //GetValue by Localization
        public string GetValueLocalized(Dictionary<string, string> List, string localization)
        {
            string value;
            localization ??= LocalizationEnum.English;

            if ((List ??= new Dictionary<string, string>()).Count == 0)
                return "No data in your Language";


            //Prendo il dato nella lingua impostata
            bool found = List.TryGetValue(localization, out value);
            if (found)
                return value;

            //Prendo il dato in inglese (a prescindere dalla lingua impostata)
            found = List.TryGetValue(LocalizationEnum.English, out value);
            if (found)
                return value;

            //Prendo il primo dato valido
            value = List.Values.ToList().FirstOrDefault();

            return value ?? "No data in your Language";
        }
    }
}

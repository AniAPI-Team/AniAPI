using Commons;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebApp
{
    public class Generic
    {
        private readonly HttpClient _httpClient;
        private readonly SpinnerService _spinner;

        //Costruttore
        public Generic(HttpClient httpClient, SpinnerService spinner)
        {
            this._httpClient = httpClient;
            this._spinner = spinner;
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
                _spinner.Show();
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
            catch (APIException ex)
            {
                APIManager.ErrorResponse(ex);
            }
            catch (Exception ex)
            {
                throw ex;
                //ApiManager.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {
                if (useSpinner)
                    _spinner.Hide();
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
                _spinner.Show();
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
            catch (APIException ex)
            {
                APIManager.ErrorResponse(ex);
            }
            catch (Exception ex)
            {
                throw ex;
                //ApiManager.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {
                if (useSpinner)
                    _spinner.Hide();
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
                _spinner.Show();
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
            catch (APIException ex)
            {
                APIManager.ErrorResponse(ex);
            }
            catch (Exception ex)
            {
                throw ex;
                //ApiManager.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {
                if (useSpinner)
                    _spinner.Hide();
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
                _spinner.Show();
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
            catch (APIException ex)
            {
                APIManager.ErrorResponse(ex);
            }
            catch (Exception ex)
            {
                throw ex;
                //ApiManager.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {
                if (useSpinner)
                    _spinner.Hide();
            }

            return res;
        }
    }
}

using System.Net.Http;
using System.Net.Http.Headers;
using System.Json;
using System;
using System.Threading.Tasks;
using System.IO;

namespace PeoplePlusMobile
{
    class DataApi
    {
        private TimeSpan _timeOut;
        private static HttpClient _client;                 //private backing variable for client property
        public HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpClient();
                    _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
                _client.Timeout = _timeOut;

                return _client;
            }
        }

        private HttpResponseMessage Response { get; set; }  //default response for checking response status

        public DataApi()
        {
            _timeOut = new TimeSpan(0, 0, Values.Delay);
        }

        public DataApi(int timeOut)
        {
            _timeOut = new TimeSpan(0, 0, timeOut);
        }

        //get data from web api
        public async Task<dynamic> GetAsync(string restUrl)
        {
            return await HandleResponseTask(Client.GetAsync(restUrl));
        }

        //get image data from web api
        public Task<Stream> GetImageAsync(string restUrl)
        {
            return Client.GetStreamAsync(restUrl);
        }

        //post/send data and get response from web api
        public Task<dynamic> PostAsync(string restUrl, HttpContent content)
        {
            return HandleResponseTask(Client.PostAsync(restUrl, content));
        }

        //put/update data and get response from web api
        public Task<dynamic> PutAsync(string restUrl, HttpContent content)
        {
            return HandleResponseTask(Client.PutAsync(restUrl, content));
        }

        //delete data and get response from web api
        public Task<dynamic> DeleteAsync(string restUrl)
        {
            return HandleResponseTask(Client.DeleteAsync(restUrl));
        }

        //check the success status of task
        public Task<dynamic> HandleResponseTask(Task<HttpResponseMessage> task)
        {
            return task.ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                {
                    Response = t.Result;
                    return GenerateResult(Response);
                }
                else if (t.Status == TaskStatus.Faulted)
                {
                    return Values.NoInternetMsg;
                }
                else if (t.Status == TaskStatus.Canceled)
                {
                    return Values.NetworkTimeoutMsg;
                }
                else
                {
                    return Values.ErrorMsg;
                }
            });
        }

        //generate and parse result for the caller
        public dynamic GenerateResult(HttpResponseMessage response)
        {
            dynamic result;

            if (response.IsSuccessStatusCode)
            {
                result = response.Content.ReadAsStringAsync().Result;
                result = JsonValue.Parse(result);
            }
            else
            {
                result = Values.ServerErrorMsg;                         //return formatted error message
            }

            return result;
        }

        //check if the response generated is a server side error or an actual response
        public static bool IsJsonObject(dynamic result)
        {
            bool status = true;

            if (result.GetType() == typeof(string))
            {
                if (result == Values.NetworkTimeoutMsg || result == Values.ServerErrorMsg || result == Values.NoInternetMsg || result == Values.ErrorMsg)
                {
                    status = false;
                }
            }

            return status;
        }

        //check if the network is responsive
        public async static Task<object[]> NetworkAccessStatus()
        {
            object[] status = new object[2];

            string statusUrl = Values.ApiRootAddress + "Login/GetNetworkStatus";
            var state = await new DataApi(10).GetAsync(statusUrl);

            if (IsJsonObject(state))
            {
                status[0] = true;
                status[1] = (string)state["Error"];
            }
            else
            {
                status[0] = false;
                status[1] = state;
            }

            return status;
        }
    }
}
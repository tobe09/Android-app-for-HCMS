using System.Net.Http;
using System.Net.Http.Headers;
using System.Json;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace PeoplePlusMobile.Old
{
    class DataApi
    {
        private static HttpClient Client;                 //private backing variable for client property
        public DataApi()
        {
            if (Client == null)
            {
                Client = new HttpClient();
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                Client.Timeout = new TimeSpan(0, 0, Values.Delay);
            }
        }
        //private HttpClient Client
        //{
        //    get
        //    {
        //        if (_client == null)
        //        {
        //            _client = new HttpClient();
        //            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //            _client.Timeout = new TimeSpan(0, 0, Values.Delay);
        //        }
        //        return _client;
        //    }
        //}

        private HttpResponseMessage response;  //default response for checking response status

        //get data from web api
        public Task<dynamic> GetAsync(string restUrl)
        {
            return Client.GetAsync(restUrl).ContinueWith(task =>
            {
                response = task.Result;                                                              //set response as gotten from server
                    return GenerateResult(response);
            });
        }

        //get image data from web api
        public Task<Stream> GetImageAsync(string restUrl)
        {
            return Client.GetStreamAsync(restUrl);
        }


        //update image data and get response from web api
        public Task<dynamic> PutImageAsync(string restUrl, byte[] ImageData)
        {
            HttpClient imageClient = new HttpClient();
            imageClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
            imageClient.Timeout = new TimeSpan(0, 0, 10);

            //string output = JsonConvert.SerializeObject(item);
            var byteContent = new ByteArrayContent(ImageData);
            MultipartFormDataContent form = new MultipartFormDataContent();
            form.Add(byteContent);

            //string json = JsonConvert.SerializeObject(item);
            //var stringContent = new StringContent("{id:}");//, Encoding.UTF8, "application/octet-stream");

            return imageClient.PutAsync(restUrl, form).ContinueWith(task =>
            {
                response = task.Result;                                                              //set response as gotten from server
                return GenerateResult(response);
            });
        }


        //post/send data and get response from web api
        public Task<dynamic> PostAsync(string restUrl, HttpContent content)
        {
            return Client.PostAsync(restUrl, content).ContinueWith(task =>
                   {
                       response = task.Result;                                                              //set response as gotten from server
                    return GenerateResult(response);
                   });
        }

        //put/update data and get response from web api
        public dynamic PutAsync(string restUrl, HttpContent content)
        {
            return Client.PutAsync(restUrl, content).ContinueWith(task =>
             {
                 response = task.Result;                                                              //set response as gotten from server
                     return GenerateResult(response);
             });
        }

        //delete data and get response from web api
        public dynamic DeleteAsync(string restUrl)
        {
            return Client.DeleteAsync(restUrl).ContinueWith(task =>
            {
                response = task.Result;                                                              //set response as gotten from server
                    return GenerateResult(response);
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
        public static bool ValidateResult(dynamic result)
        {
            bool status = true;

            if (result.GetType() == Values.NetworkTimeoutMsg.GetType() || result.GetType() == Values.ServerErrorMsg.GetType())
            {
                if (result == Values.NetworkTimeoutMsg || result == Values.ServerErrorMsg)
                {
                    status = false;
                }
            }

            return status;
        }
    }
}
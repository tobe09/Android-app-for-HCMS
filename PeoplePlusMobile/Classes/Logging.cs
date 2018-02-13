using System;
using System.Collections.Generic;
using System.Net.Http;

namespace PeoplePlusMobile
{
    public static class Logging
    {
        public async static void Log(this Exception ex)
        {
            try
            {
                string restUrl = Values.ApiRootAddress + "Error";

                var errorContent = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "Message", ex.Message },
                    { "StackTrace", ex.StackTrace }
                });

                await new DataApi().PostAsync(restUrl, errorContent);
            }
            catch { }       //unsucessful log
        }

    }
}
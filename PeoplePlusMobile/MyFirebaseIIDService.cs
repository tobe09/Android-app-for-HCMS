using Android.App;
using Firebase.Iid;
using System.Collections.Generic;
using System.Net.Http;

namespace PeoplePlusMobile
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class MyFirebaseIIDService : FirebaseInstanceIdService
    {
        //for getting new unique device id token
        public override void OnTokenRefresh()
        {
            string deviceId = FirebaseInstanceId.Instance.Token;
            SendRegistrationToServer(deviceId);
        }

        //send registered device id to server
        async void SendRegistrationToServer(string token)
        {
            string restUrl = Values.ApiRootAddress + "Notification/PostNewDevId";
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "DeviceId", token}
                    });

            dynamic status = await new DataApi().PostAsync(restUrl, content);
        }
    }
}
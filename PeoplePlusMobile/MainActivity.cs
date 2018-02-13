using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Views;
using System;
using System.Net.Http;
using System.Collections.Generic;
//using Android.Webkit;
using System.Timers;
using Android;
using System.Threading.Tasks;
using Firebase.Iid;
using Android.Content;
using Android.Gms.Common;

namespace PeoplePlusMobile
{
    [Activity(Label = "PeoplePlus", MainLauncher = true, Icon = "@drawable/neptunelogo", WindowSoftInputMode = SoftInput.AdjustResize,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class MainActivity : Activity
    {
        Timer t;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //check for notification and send to worklistviewer

            if (User.IsValidUser())
            {
                if (Intent.Extras != null) //!string.IsNullOrEmpty(Intent.Extras.GetString("title"))
                {
                    //ICollection<string> val = Intent.Extras.KeySet();
                    string title = Intent.Extras.GetString("title");
                    string body = Intent.Extras.GetString("body");
                    StartActivity(typeof(WorkListActivity));
                }
                else
                    StartActivity(typeof(HomeActivity));
            }
            else
            {
                SetContentView(Resource.Layout.Main);

                Button btnSubmit = FindViewById<Button>(Resource.Id.btnSubmit);
                btnSubmit.Click += BtnSubmit_Click;
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            LoadImageView();
        }

        protected override void OnPause()
        {
            base.OnPause();
            t?.Dispose();
        }

        void LoadImageView()
        {
            t = new Timer();
            t.Interval = 2000;

            Type type = typeof(Resource.Drawable);
            Random rand = new Random();

            int next = 1;
            t.Elapsed += (s, e) =>
            {
                RunOnUiThread(() =>
                {
                    try
                    {
                        //int next = rand.Next(1, 6);
                        if (next >= 5) next = 1;
                        next++;
                        var imgField = type.GetField("login" + next);
                        FindViewById<ImageView>(Resource.Id.imageView1).SetBackgroundResource((int)imgField.GetValue(null));
                        //FindViewById<ImageView>(Resource.Id.imageView1).SetImageResource(Resource.Drawable.login5);
                    }
                    catch { }
                });
            };
            t.Start();
        }

        //void LoadWebView()
        //{
        //    var webView = FindViewById<WebView>(Resource.Id.webView1);
        //    string url = Values.ApiRootAddress.Substring(0, Values.ApiRootAddress.Length - 4);
        //    webView.LoadUrl(url + "WebViewPage.html");
        //    webView.SetWebChromeClient(new WebChromeClient());
        //    webView.Settings.JavaScriptEnabled = true;
        //    webView.Settings.DomStorageEnabled = true;
        //}
        
        private async void BtnSubmit_Click(object sender, EventArgs e)
        {
            EditText edtUserId = FindViewById<EditText>(Resource.Id.edtUserId);
            EditText edtPassword = FindViewById<EditText>(Resource.Id.edtPassword);
            TextView tvwMsg = FindViewById<TextView>(Resource.Id.tvwMsg);

            if (edtUserId.Text.Trim().Length > 0 && edtPassword.Text.Length > 0)
            {
                try
                {
                    tvwMsg.SetTextColor(Color.White);
                    tvwMsg.Text = Values.WaitingMsg;
                    string encryptedPassword = new Encryption().EncryptText(edtPassword.Text);
                    string restUrl = Values.ApiRootAddress + "Login";
                    var content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "UserId", edtUserId.Text.Trim() },
                        { "Password", encryptedPassword }
                    });

                    dynamic response = await new DataApi().PostAsync(restUrl, content);                // await result from data api asynchroonously

                    bool success = DataApi.IsJsonObject(response);
                    //valid result
                    if (success)
                    {
                        //check content length
                        if (response["Error"] == "")
                        {
                            object[] status = await PreLoginActions((string)response["CompId"], edtUserId.Text.Trim().ToUpper(), response["EmployeeNo"].ToString());

                            if ((bool)status[0])
                            {
                                if ((string)status[1] != "") Toast.MakeText(this, (string)status[1], ToastLength.Short).Show();     //show any warning available
                                SaveToPreferences(response, edtUserId.Text.Trim().ToUpper(), encryptedPassword);
                                StartActivity(typeof(HomeActivity));        //go to the home page
                            }
                            else
                                tvwMsg.ErrorMsg((string)status[1]);
                        }
                        else
                        {
                            tvwMsg.ErrorMsg((string)response["Error"]);        //error from the server response body
                        }
                    }
                    else
                        tvwMsg.ErrorMsg((string)response);                       //error communicating with server
                }
                catch (Exception ex)
                {
                    ex.Log();
                    tvwMsg.ErrorMsg(Values.ErrorMsg);                  //standard exception messag
                }
            }
            else
            {
                if (edtUserId.Text.Length == 0)
                    tvwMsg.ErrorMsg("Please Enter Your Username");
                else
                    tvwMsg.ErrorMsg("Please Enter Your Password");
            }
        }

        //used to generate local notifications
        public void CreateNotification(dynamic response)
        {
            TaskStackBuilder stackBuilder = TaskStackBuilder.Create(this);
            Intent wklIntent = new Intent(this, typeof(WorkListActivity));
            stackBuilder.AddNextIntent(wklIntent);

            PendingIntent pendingIntent = stackBuilder.GetPendingIntent(0, PendingIntentFlags.OneShot);  //id=0

            Notification.Style style;
            if (response.Count == 1)
            {
                Notification.BigTextStyle textStyle = new Notification.BigTextStyle();
                textStyle.BigText(response[0]["MSG_CONTENT"]);
                textStyle.SetSummaryText(response[0]["MSG_TITLE"]);
                style = textStyle;
            }
            else
            {
                Notification.InboxStyle inboxStyle = new Notification.InboxStyle();
                inboxStyle.AddLine(response[0]["MSG_CONTENT"]);
                inboxStyle.AddLine(response[1]["MSG_CONTENT"]);
                if (response.Count > 2) inboxStyle.SetSummaryText("+" + (response.Count - 2) + " more");
                style = inboxStyle;
            }

            Notification.Builder builder = new Notification.Builder(Application.Context)
                .SetContentTitle("PeoplePlus Notification")
                .SetContentText("Pending Tasks in your WorkList Viewer")
                .SetSmallIcon(Resource.Drawable.login3)
                .SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.login3))
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent);

            if ((int)Build.VERSION.SdkInt >= 21)
            {
                builder.SetVisibility(NotificationVisibility.Private)
                .SetCategory(Notification.CategoryAlarm)
                .SetCategory(Notification.CategoryCall)
                .SetStyle(style);
            }

            builder.SetPriority((int)NotificationPriority.High);
            builder.SetDefaults(NotificationDefaults.Sound | NotificationDefaults.Vibrate);

            Notification notification = builder.Build();

            NotificationManager notificationManager = GetSystemService(NotificationService) as NotificationManager;
            notificationManager.Notify(0, notification);
        }

        //save user data to preference dictionary
        private bool SaveToPreferences(dynamic result, string userId, string password)
        {
            AppPreferences pref = new AppPreferences();

            //save values to collection
            pref.SaveValue(User.UserId, userId);
            pref.SaveValue(User.Password, password);
            pref.SaveValue(User.CompId, result["CompId"]);
            pref.SaveValue(User.CompName, result["CompName"]);
            pref.SaveValue(User.RoleId, result["RoleId"]);
            pref.SaveValue(User.EmployeeNo, result["EmployeeNo"].ToString());
            pref.SaveValue(User.Name, result["Name"]);
            pref.SaveValue(User.Title, result["Title"]);
            pref.SaveValue(User.DateOfBirth, result["DateOfBirth"]);
            pref.SaveValue(User.AccountNo, result["AccountNo"]);
            pref.SaveValue(User.Email, result["Email"]);
            pref.SaveValue(User.DeptCode, result["DeptCode"]);
            pref.SaveValue(User.GradeCode, result["GradeCode"]);
            pref.SaveValue(User.LicenseDate, result["LicenseDate"]);
            pref.SaveValue(User.AccountingYear, result["AccountingYear"]);
            pref.SaveValue(User.Location, result["Location"]);
            pref.SaveValue(User.Department, result["Department"]);
            pref.SaveValue(User.Designation, result["Designation"]);
            pref.SaveValue(User.Grade, result["Grade"]);
            pref.SaveValue(User.LocationCode, result["LocationCode"]);

            return true;
        }

        //check the registration status of device
        public async Task<object[]> DeviceRegistered(Activity callingActivity)
        {
            object[] values = new object[2];
            values[0] = true;

            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(callingActivity);

            if (resultCode == ConnectionResult.Success && !string.IsNullOrEmpty(FirebaseInstanceId.Instance.Token))
            {
                values[1] = "";
            }
            else
            {
                if (resultCode != ConnectionResult.Success)
                {
                    if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                        values[1] = "From Google Play Service: " + GoogleApiAvailability.Instance.GetErrorString(resultCode);
                    else
                    {
                        values[1] = "This device does not support google play services";
                        //t = new Timer(3000);                                  do not terminate app
                        //t.Elapsed += (s, e) => { Finish(); t.Dispose(); };
                        //t.Start();
                    }
                }
                else
                {
                    values[0] = false;
                    if ((bool)(await DataApi.NetworkAccessStatus())[0])
                        values[1] = "Google Play Service Initializing. Please try again later";
                    else
                        values[1] = "Connect to the internet to initialize device";
                }
            }

            return values;
        }

        //action to take befor user logs in
        private async Task<object[]> PreLoginActions(string compId, string userId, string empNo)
        {
            object[] status = await DeviceRegistered(this);

            if ((bool)status[0])
            {
                string restUrl = Values.ApiRootAddress + "Notification/PostPreLogin";
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "CompId", compId },
                        { "EmpNo", empNo },
                        { "UserId", userId },
                        { "DeviceId", FirebaseInstanceId.Instance.Token }
                    });

                dynamic response = await new DataApi().PostAsync(restUrl, content);

                if (DataApi.IsJsonObject(response))
                {
                    response = response["Values"];

                    //generate notification(s)
                    if (response.Count > 0)
                    {
                        CreateNotification(response);
                    }
                }
                else
                {
                    status[1] = (string)response;
                }
            }

            return status;
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            FinishAffinity();
        }
    }
}


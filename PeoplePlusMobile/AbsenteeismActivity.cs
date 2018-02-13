using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Widget;
using System.Net.Http;

namespace PeoplePlusMobile
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class AbsenteeismActivity : BaseActivity
    {
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Absenteeism);

            // Create your application here
            Button btnSubmitAbsReq = FindViewById<Button>(Resource.Id.btnSubmitAbsReq);
            btnSubmitAbsReq.Click += btnSubmitAbsReq_Click;
            //set date textview
            TextView tvwMsg = FindViewById<TextView>(Resource.Id.tvwMsg);
            TextView tvwReason = FindViewById<TextView>(Resource.Id.edtReasonAbsReq);
            EditText edtDateAbsReq = FindViewById<EditText>(Resource.Id.edtDateAbsReq);
            
            edtDateAbsReq.Click += (s, e) =>
            {
                var dateTimeNow = DateTime.Now;
                DatePickerDialog datePicker = new DatePickerDialog
                (this,
                (sender, eventArgs) => { edtDateAbsReq.Text = eventArgs.Date.Day + "/" + eventArgs.Date.Month + "/" + eventArgs.Date.Year; },
                dateTimeNow.Year, dateTimeNow.Month - 1, dateTimeNow.Day);
                datePicker.Show();
            };

            tvwMsg.ErrorMsg((string)(await DataApi.NetworkAccessStatus())[1]);
        }

        private async void btnSubmitAbsReq_Click(object sender, EventArgs e)
        {
            (sender as Button).Enabled = false;

            TextView tvwMsg = FindViewById<TextView>(Resource.Id.tvwMsg);
            TextView tvwReason = FindViewById<TextView>(Resource.Id.edtReasonAbsReq);
            string dateStr = FindViewById<EditText>(Resource.Id.edtDateAbsReq).Text;
            DateTime? date = ConvertStringToDate(dateStr);
            string reason = FindViewById<EditText>(Resource.Id.edtReasonAbsReq).Text;

            if (date != null && date > DateTime.Now && reason.Length > 0)
            {
                try
                {
                    string restUrl = Values.ApiRootAddress + "Absenteeism";
                    var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "EmployeeNo", new AppPreferences().GetValue(User.EmployeeNo) },
                    { "DateStr", dateStr },
                    { "Reason", reason },
                    { "UserId",new AppPreferences().GetValue(User.UserId) },
                    { "CompId", new AppPreferences().GetValue(User.CompId) }
                });

                    dynamic response = await new DataApi().PostAsync(restUrl, content);

                    bool success = DataApi.IsJsonObject(response);
                    if (success)
                    {
                        if (response["Error"] == "")     //success
                        {
                            FindViewById<EditText>(Resource.Id.edtDateAbsReq).Text = "";
                            FindViewById<EditText>(Resource.Id.edtReasonAbsReq).Text = "";
                            tvwMsg.SuccessMsg("Records Saved Successfully");
                        }
                        else
                        {
                            tvwMsg.ErrorMsg((string)response["Error"]);
                        }
                    }
                    else
                    {
                        tvwMsg.ErrorMsg((string)response);
                    }
                }
                catch (Exception ex)
                {
                    ex.Log();
                    tvwMsg.ErrorMsg(Values.ErrorMsg);
                }
            }
            else
            {
                if (date == null) tvwMsg.ErrorMsg("Please select a date");
                else if (date <= DateTime.Now) tvwMsg.ErrorMsg("Please select a future time");
                else tvwMsg.ErrorMsg("Please enter your reason");
            }

            (sender as Button).Enabled = true;
        }

        public override void OnBackPressed()
        {
            MyOnBackPressed();
        }
    }
}
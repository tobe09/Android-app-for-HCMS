using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Graphics;
using Android.Widget;
using System.Net.Http;

namespace PeoplePlusMobile
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class MedicalRequestActivity : BaseActivity
    {
        List<string> spinnerHospCode;
        //bool callFlag = true;       //to check double calls on older api levels to handle called event e.g. OnTextChanged especially for non-idempotent actions eg Post.

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.MedicalRequest);

            TextView tvwMsg = FindViewById<TextView>(Resource.Id.tvwMsgMedReq);
            //set date textview
            EditText edtDateMedReq = FindViewById<EditText>(Resource.Id.edtDateMedReq);
            edtDateMedReq.Click += (s, e) =>
            {
                var dateTimeNow = DateTime.Now;
                bool flag = true;       //to check double calls on older android api levels
                DatePickerDialog datePicker = new DatePickerDialog
                (this,
                (sender, eventArgs) =>
                {
                    if (flag)
                    {
                        edtDateMedReq.Text = eventArgs.Date.Day + "/" + eventArgs.Date.Month + "/" + eventArgs.Date.Year;
                        flag = !flag;
                        //callFlag = true;
                    }
                    //else callFlag = false;
                },
                dateTimeNow.Year, dateTimeNow.Month - 1, dateTimeNow.Day + 1);
                datePicker.Show();
            };

            try
            {
                //to get the list of hospitals from the web api asynchronously
                string restUrl = Values.ApiRootAddress + "MedicalRequest/?compId=" + new AppPreferences().GetValue(User.CompId);

                tvwMsg.BasicMsg(Values.LoadingMsg);
                dynamic response = await new DataApi().GetAsync(restUrl);
                tvwMsg.Text = "";

                if (IsJsonObject(response))
                {
                    response = response["Values"];          //get the values sent by the web api

                    spinnerHospCode = GetAttributeList(response, "CODE");         //save code values
                    List<string> hospitals = GetAttributeList(response, "NAME", "Select a Hospital");     //list to be used to populate spinner
                    var hospitalAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, hospitals);

                    Spinner spnHospital = FindViewById<Spinner>(Resource.Id.spnHospitalMedReq);
                    spnHospital.Adapter = hospitalAdapter;
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

            //get submit button and subscribe to its click event
            Button btnSubmitMedReq = FindViewById<Button>(Resource.Id.btnSubmitMedReq);
            btnSubmitMedReq.Click += BtnSubmitMedReq_Click;
        }

        private async void BtnSubmitMedReq_Click(object sender, EventArgs e)
        {
            TextView tvwMsg = FindViewById<TextView>(Resource.Id.tvwMsgMedReq);
            string dateStr = FindViewById<EditText>(Resource.Id.edtDateMedReq).Text;
            try
            {
                DateTime? date = new CommonMethodsClass().ConvertStringToDate(dateStr);
                string reason = FindViewById<EditText>(Resource.Id.edtReasonMedReq).Text;
                int selection = (int)FindViewById<Spinner>(Resource.Id.spnHospitalMedReq).SelectedItemId;

                if (date != null && date > DateTime.Now && selection > 0 && reason.Length > 0)
                {
                    string restUrl = Values.ApiRootAddress + "MedicalRequest";
                    var content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "EmployeeNo", new AppPreferences().GetValue(User.EmployeeNo) },
                        { "HospitalId", spinnerHospCode[selection] },
                        { "DateStr", dateStr },
                        { "Reason", reason },
                        {"UserId",new AppPreferences().GetValue(User.UserId) },
                        {"CompId", new AppPreferences().GetValue(User.CompId) }
                    });

                    (sender as Button).Enabled = false;
                    tvwMsg.BasicMsg(Values.WaitingMsg);
                    dynamic response = await new DataApi().PostAsync(restUrl, content);
                    tvwMsg.Text = "";
                    (sender as Button).Enabled = true;

                    bool success = DataApi.IsJsonObject(response);
                    if (success)
                    {
                        if (response["Error"] == "")     //success
                        {
                            FindViewById<EditText>(Resource.Id.edtDateMedReq).Text = "";
                            FindViewById<EditText>(Resource.Id.edtReasonMedReq).Text = "";
                            FindViewById<Spinner>(Resource.Id.spnHospitalMedReq).SetSelection(0);
                            tvwMsg.SuccessMsg("Record Saved Successfully");
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
                else
                {
                    if (date == null) tvwMsg.ErrorMsg("Please select a leave date");
                    else if (date <= DateTime.Now) tvwMsg.ErrorMsg("Please select a future date");
                    else if (selection == 0) tvwMsg.ErrorMsg("Please select a hospital");
                    else tvwMsg.ErrorMsg("Please enter your reason");
                }
            }
            catch (Exception ex)
            {
                ex.Log();
                tvwMsg.ErrorMsg(Values.ErrorMsg);
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            MyOnBackPressed();
        }
    }
}
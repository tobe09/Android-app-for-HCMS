using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Widget;
using System.Net.Http;

namespace PeoplePlusMobile
{

    [Activity]
    public class LeaveRecallActivity : BaseActivity
    {
        List<string> leaveitemList = new List<string>();
        string RqstNo = "";
        string LvGrp = "";
        string LevCode = "";
        string GRQSTID = "";
        string EName = "";
        string LvDesc = "";
        string SpentDays = "";
        string ENMBR = "";
        string OldDuration = "";
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.LeaveRecall);

            //set date textview
            EditText edtRecallDt = FindViewById<EditText>(Resource.Id.edtRecallDt);
            //  edtExtendTo.Text = DateTime.Today.ToString("d");
            edtRecallDt.Click += (s, e) =>
            {
                var dateTimeNow = DateTime.Now;
                DatePickerDialog datePicker = new DatePickerDialog
                (this,
                (sender, eventArgs) => { edtRecallDt.Text = eventArgs.Date.Day + "/" + eventArgs.Date.Month + "/" + eventArgs.Date.Year; },
                dateTimeNow.Year, dateTimeNow.Month - 1, dateTimeNow.Day);
                datePicker.Show();
            };

            LoadSpinner();

            //get submit button and subscribe to its click event
            Button btnSubmitLvRec = FindViewById<Button>(Resource.Id.btnSubmitLvRec);
            btnSubmitLvRec.Click += btnSubmitLvRec_Click;
        }

        public async void GetWorkDays()
        {
            TextView tvwMsg = FindViewById<TextView>(Resource.Id.tvwMsgLeaveRecall);
            EditText edtRecallDt = FindViewById<EditText>(Resource.Id.edtRecallDt);
            TextView tvwStDateRec = FindViewById<TextView>(Resource.Id.tvwStDateRec);

                //Call getworking Days   
                if (tvwStDateRec.Text != "" && edtRecallDt.Text != "" && tvwStDateRec.Text != edtRecallDt.Text)
                {
                DateTime? StDate = new CommonMethodsClass().ConvertStringToDate(tvwStDateRec.Text);
                DateTime? EndDt = new CommonMethodsClass().ConvertStringToDate(edtRecallDt.Text);

                if (EndDt > StDate)
                {
                    string restUrl1 = Values.ApiRootAddress + "LeaveRecall/GetWorkDays/?strFromDate=" + tvwStDateRec.Text + "&strToDate=" + edtRecallDt.Text + "&compId=" + new AppPreferences().GetValue(User.CompId);

                    tvwMsg.BasicMsg(Values.WaitingMsg);
                    dynamic response = await new DataApi().GetAsync(restUrl1);
                    tvwMsg.Text = "";

                    if (IsJsonObject(response))
                        SpentDays = (int)response + "";
                    else
                        tvwMsg.ErrorMsg((string)response);
                }
            }
        }

        public async void LoadSpinner()
        {
            TextView tvwMsg = FindViewById<TextView>(Resource.Id.tvwMsgLeaveRecall);
            try
            {
                //To get Employee Leave To be Recalled from the web api asynchronously and Bind To The Spinner
                string restUrl = Values.ApiRootAddress + "LeaveRecall/GetEmployeeLv/?compId=" + new AppPreferences().GetValue(User.CompId) + "&EmployeeNo=" + new AppPreferences().GetValue(User.EmployeeNo);

                tvwMsg.BasicMsg(Values.LoadingMsg);
                dynamic response = await new DataApi().GetAsync(restUrl);
                tvwMsg.Text = "";

                bool success = DataApi.IsJsonObject(response);
                if (success)
                {
                    response = response["Values"];          //get the values sent by the web api

                    StoreToPref(response);              //store to preferences for future access
                    var Leave = GetLeaveType(response);     //list to be used to populate spinner
                    var LeaveAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, Leave);
                    Spinner spnLeaveRec = FindViewById<Spinner>(Resource.Id.spnLeaveRec);
                    spnLeaveRec.Adapter = LeaveAdapter;
                    spnLeaveRec.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spnLeaveRec_ItemSelected);
                    LeaveAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
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

        //genereate a list of Leave for spinner
        private List<string> GetLeaveType(dynamic response)
        {
            List<string> LeaveExt = new List<string>();

            LeaveExt.Add("Please Select...");
            for (int i = 0; i < response.Count; i++)
            {
                LeaveExt.Add(response[i]["DESCRIPTION"]);
            }

            for (int i = 0; i < response.Count; i++)               //start from one to ensure synchronization
            {
                leaveitemList.Add(response[i]["CODE"].ToString());
            }
            return LeaveExt;
        }

        //store Leave values to preference dictionary for submittion access
        private void StoreToPref(dynamic response)
        {
            AppPreferences LeaveExtPref = new AppPreferences();
            for (int i = 0; i < response.Count; i++)               //start from one to ensure synchronization
            {
                LeaveExtPref.SaveValue(i + "", response[i]["CODE"].ToString());
            }
        }

        //Used to Get Other Information
        private async void spnLeaveRec_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            TextView tvwMsg = FindViewById<TextView>(Resource.Id.tvwMsgLeaveRecall);
            Spinner spinner = (Spinner)sender;
            Array Leaveitem = leaveitemList.ToArray();
            if (e.Id != 0)
            {

                //tvwStrDtExt, tvwEndDtExt,  tvwRemainDaysExt
                TextView tvwLvTypeRec = FindViewById<TextView>(Resource.Id.tvwLvTypeRec);
                TextView tvwStartDtRec = FindViewById<TextView>(Resource.Id.tvwStartDtRec);
                TextView tvwResumeRec = FindViewById<TextView>(Resource.Id.tvwResumeRec);
                TextView tvwStDateRec = FindViewById<TextView>(Resource.Id.tvwStDateRec);
                EditText edtRecallDt = FindViewById<EditText>(Resource.Id.edtRecallDt);

                int RqstId = (int)double.Parse(Leaveitem.GetValue(e.Id - 1).ToString());
                string restUrl2 = Values.ApiRootAddress + "LeaveRecall/GetLeaveInfo/?compId=" + new AppPreferences().GetValue(User.CompId) + "&RqstNo=" + RqstId + "&EmpNo=" + new AppPreferences().GetValue(User.EmployeeNo);

                tvwMsg.BasicMsg(Values.WaitingMsg);
                dynamic response = await new DataApi().GetAsync(restUrl2);
                tvwMsg.Text = "";

                if (IsJsonObject(response))
                {
                    response = response["Values"];

                    tvwLvTypeRec.Text = response[0]["LVDESC"];
                    tvwStartDtRec.Text = response[0]["FROMDT"];
                    tvwResumeRec.Text = response[0]["RESUMEDATE"];
                    tvwStDateRec.Text = response[0]["STARTDT"];

                    ENMBR = response[0]["NMBR"].ToString();
                    EName = response[0]["NAME"];
                    LvDesc = response[0]["LVDESC"];
                    RqstNo = response[0]["RQSTNUM"].ToString();
                    GRQSTID = response[0]["GRQSTID"].ToString();
                    LvGrp = response[0]["LVGRP"].ToString();
                    LevCode = response[0]["LVCODE"];
                    OldDuration = response[0]["DURATN"].ToString();
                }
                else
                {
                    tvwMsg.ErrorMsg((string)response);
                }
            }
        }

        //Used to get the Working Days based on the Dates selected
        //private async void GetWorkingDays()
        //{
        //    TextView tvwMsg = FindViewById<TextView>(Resource.Id.tvwMsgLeaveReq);
        //    TextView tvwDurationExt = FindViewById<TextView>(Resource.Id.tvwDurationExt);
        //    EditText edtStartDate = FindViewById<EditText>(Resource.Id.edtStartDate);
        //    EditText edtEndDate = FindViewById<EditText>(Resource.Id.edtExtendTo);
        //    string RemainingDays = FindViewById<TextView>(Resource.Id.tvwRemainDaysExt).Text;
        //    tvwMsg.Text = "";
        //    //Call getworking Days   
        //    if (edtStartDate.Text != "" && edtEndDate.Text != "")
        //    {
        //        int StartDt = Convert.ToInt32(edtStartDate.Text.Replace("/", ""));
        //        int EndDt = Convert.ToInt32(edtEndDate.Text.Replace("/", ""));

        //        if (EndDt > StartDt)
        //        {
        //            string restUrl4 = Values.ApiRootAddress + "LeaveRecall/GetWorkDays/?strFromDate=" + edtStartDate.Text + "&strToDate=" + edtEndDate.Text + "&compId=" + new AppPreferences().GetValue(User.CompId);

        //            dynamic response = await new DataApi().GetAsync(restUrl4);
        //            int Days = (int)response;
        //            if (int.Parse(RemainingDays) >= Days)
        //            {
        //                tvwDurationExt.Text = (int)response + "";
        //            }
        //            else
        //            {
        //                tvwDurationExt.Text = "";
        //                tvwMsg.Text = "Day Applied For Cannot Be Greater Than Remaining Days";
        //            }                       
        //        }
        //    }
        //}

        //Used To Do variuos checks And To save the records 
        private async void btnSubmitLvRec_Click(object sender, EventArgs e)
        {
            TextView tvwMsg = FindViewById<TextView>(Resource.Id.tvwMsgLeaveRecall);

            string edtRecallDt = FindViewById<EditText>(Resource.Id.edtRecallDt).Text; //Get End Date
            DateTime? date = ConvertStringToDate(edtRecallDt);

            int selection = (int)FindViewById<Spinner>(Resource.Id.spnLeaveRec).SelectedItemId;
            string RecallEmp = selection + "";
            if (date != null && date > DateTime.Now && selection > 0)
            {
                try
                {
                    string restUrl = Values.ApiRootAddress + "LeaveRecall";
                    var contentLeave = new FormUrlEncodedContent(new Dictionary<string, string>
                            {
                                 { "spentDays", SpentDays },
                                 { "AccountingYear",new AppPreferences().GetValue(User.AccountingYear) },
                                 { "LvCode", LevCode },
                                 { "LvGrp", LvGrp },
                                 { "RqstId", RqstNo },
                                 { "RecallEmp", ENMBR},
                                 { "UserId", new AppPreferences().GetValue(User.UserId)  },
                                 { "CompId", new AppPreferences().GetValue(User.CompId)  },
                                 { "GRqstID",GRQSTID},
                                 { "OldDuration", OldDuration },
                                 { "RecallDt", edtRecallDt}
                            });

                    (sender as Button).Enabled = false;
                    tvwMsg.BasicMsg(Values.WaitingMsg);
                    dynamic response = await new DataApi().PostAsync(restUrl, contentLeave);
                    tvwMsg.Text = "";
                    (sender as Button).Enabled = true;

                    bool success = DataApi.IsJsonObject(response);
                    if (success)
                    {
                        if (response["Error"] == "")     //success
                        {
                            FindViewById<TextView>(Resource.Id.tvwLvTypeRec).Text = "";
                            FindViewById<TextView>(Resource.Id.tvwStartDtRec).Text = "";
                            FindViewById<TextView>(Resource.Id.tvwResumeRec).Text = "";
                            FindViewById<EditText>(Resource.Id.edtRecallDt).Text = "";
                            FindViewById<Spinner>(Resource.Id.spnLeaveRec).SetSelection(0);
                            LoadSpinner();
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
                if (date == null) tvwMsg.ErrorMsg("Please select a recall Date");
                else if (date <= DateTime.Now) tvwMsg.ErrorMsg("Please select a future date");
                else if (selection == 0) tvwMsg.ErrorMsg("Please select a employee");
            }
        }

        public override void OnBackPressed()
        {
            MyOnBackPressed();
        }
    }
}










using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Widget;
using System.Net.Http;

namespace PeoplePlusMobile
{

    [Activity]
    public class LeaveExtensionActivity : BaseActivity
    {
        List<string> leaveitemList = new List<string>();
        string RqstNo = "";
        string RelOfficerId = "";
        string PrevReqId = "";
        string LvGrp = "";
        string LevCode = "";

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.LeaveExtension);

            //set date textview
            TextView tvwMsg = FindViewById<TextView>(Resource.Id.tvwMsgLeaveExt);
            EditText edtExtendTo = FindViewById<EditText>(Resource.Id.edtExtendTo);
            //  edtExtendTo.Text = DateTime.Today.ToString("d");
            edtExtendTo.Click += (s, e) =>
            {
                var dateTimeNow = DateTime.Now;
                DatePickerDialog datePicker = new DatePickerDialog
                (this,
                (sender, eventArgs) =>
                {
                    edtExtendTo.Text = eventArgs.Date.Day + "/" + eventArgs.Date.Month + "/" + eventArgs.Date.Year;
                    GetWorkDays();
                },
                dateTimeNow.Year, dateTimeNow.Month - 1, dateTimeNow.Day);
                datePicker.Show();
            };

            try
            {
                //to get other parameters to compute number of days from the web api asynchronously
                string restUrl = Values.ApiRootAddress + "LeaveExtension/?compId=" + new AppPreferences().GetValue(User.CompId) + "&EmployeeNo=" + new AppPreferences().GetValue(User.EmployeeNo);

                tvwMsg.BasicMsg(Values.LoadingMsg);
                dynamic response = await new DataApi().GetAsync(restUrl);
                tvwMsg.Text = "";

                if (IsJsonObject(response))
                {
                    response = response["Values"];          //get the values sent by the web api

                    StoreToPref(response, "LeaveExt");              //store to preferences for future access
                    var Leave = GetLeaveType(response);     //list to be used to populate spinner
                    var LeaveAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, Leave);
                    Spinner spnLeaveExt = FindViewById<Spinner>(Resource.Id.spnLeaveExt);
                    spnLeaveExt.Adapter = LeaveAdapter;
                    spnLeaveExt.ItemSelected += spnLeaveExt_ItemSelected;
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
            //get submit button and subscribe to its click event
            Button btnSubmitLeaveExt = FindViewById<Button>(Resource.Id.btnSubmitLeaveExt);
            btnSubmitLeaveExt.Click += btnSubmitLeaveExt_Click;
        }


        async void GetWorkDays()
        {
            TextView tvwMsg = FindViewById<TextView>(Resource.Id.tvwMsgLeaveExt);
            tvwMsg.Text = "";
            EditText edtExtendTo = FindViewById<EditText>(Resource.Id.edtExtendTo);
            string RemainingDaysExt = FindViewById<TextView>(Resource.Id.tvwRemainDaysExt).Text;
            TextView tvwDurationExt = FindViewById<TextView>(Resource.Id.tvwDurationExt);
            TextView tvwStartDt = FindViewById<TextView>(Resource.Id.tvwStrDtExt);

            //Call getworking Days   
            DateTime? StartDt = ConvertStringToDate(tvwStartDt.Text);
            DateTime? EndDt = ConvertStringToDate(edtExtendTo.Text);

            if (StartDt != null && EndDt != null && EndDt > StartDt)
            {
                string restUrlE1 = Values.ApiRootAddress + "LeaveExtension/GetWorkDays/?strFromDate=" + tvwStartDt.Text + "&strToDate=" + edtExtendTo.Text + "&compId=" + new AppPreferences().GetValue(User.CompId);

                dynamic response = await new DataApi().GetAsync(restUrlE1);
                int Days = (int)response;
                if (int.Parse(RemainingDaysExt) >= Days)
                {
                    tvwDurationExt.Text = (int)response + "";
                    tvwMsg.Text = "";
                }
                else
                {
                    tvwDurationExt.Text = "";
                    edtExtendTo.Text = "";
                    tvwMsg.ErrorMsg("Day Applied For Cannot Be Greater Than Remaining Days");
                }
            }
            else
            {
                tvwDurationExt.Text = "";
                edtExtendTo.Text = "";
                if (StartDt == null) tvwMsg.ErrorMsg("No start date selected");
                else if (EndDt == null) tvwMsg.ErrorMsg("Please select an extension date");
                else tvwMsg.ErrorMsg("Extension date cannot be less than start date");
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
        private void StoreToPref(dynamic response, string Where)
        {
            AppPreferences LeaveExtPref = new AppPreferences();
            for (int i = 0; i < response.Count; i++)               //start from one to ensure synchronization
            {
                LeaveExtPref.SaveValue(i + "", response[i]["CODE"].ToString());
            }
        }

        //Used to Get and bind Leave to the Spinner
        private async void spnLeaveExt_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            if (e.Id != 0)
            {
                TextView tvwMsg = FindViewById<TextView>(Resource.Id.tvwMsgLeaveExt);
                //tvwStrDtExt, tvwEndDtExt,  tvwRemainDaysExt
                TextView tvwStrDtExt = FindViewById<TextView>(Resource.Id.tvwStrDtExt);
                TextView tvwResumeDtExt = FindViewById<TextView>(Resource.Id.tvwResumeDtExt);
                TextView tvwRemainDaysExt = FindViewById<TextView>(Resource.Id.tvwRemainDaysExt);

                string RqstId = leaveitemList[(int)e.Id - 1];
                string restUrlE2 = Values.ApiRootAddress + "LeaveExtension/GetLeaveExtInfo/?compId=" + new AppPreferences().GetValue(User.CompId) + "&RqstId=" + RqstId + "&EmployeeNo=" + new AppPreferences().GetValue(User.EmployeeNo);

                dynamic response = await new DataApi().GetAsync(restUrlE2);

                if (IsJsonObject(response))
                {
                    response = response["Values"];

                    string StartDate = response[0]["STARTDT"];
                    string EndDate = response[0]["ENDDT"];

                    DateTime StartDateExt = DateTime.Parse(StartDate);
                    DateTime EndDateExt = DateTime.Parse(EndDate);
                    DateTime dt = DateTime.Parse("09/12/2009");

                    tvwStrDtExt.Text = StartDateExt.ToString("dd/MM/yyyy");
                    tvwResumeDtExt.Text = EndDateExt.ToString("dd/MM/yyyy");
                    tvwRemainDaysExt.Text = (int)double.Parse(response[0]["RMNDAYS"].ToString()) + "";
                    RqstNo = response[0]["RQSTNMBR"].ToString();
                    RelOfficerId = (string)response[0]["LV_REL_OFF"];
                    PrevReqId = RqstId;
                    LvGrp = response[0]["LVGRP"].ToString();
                    LevCode = response[0]["LVCODE"];
                }
                else
                {
                    tvwMsg.ErrorMsg((string)response);
                }
            }
        }

        ////Used to get the Working Days based on the Dates selected
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
        //            string restUrl4 = Values.ApiRootAddress + "LeaveExtension/GetWorkDays/?strFromDate=" + edtStartDate.Text + "&strToDate=" + edtEndDate.Text + "&compId=" + new AppPreferences().GetValue(User.CompId);

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
        private async void btnSubmitLeaveExt_Click(object sender, EventArgs e)
        {
            TextView tvwMsg = FindViewById<TextView>(Resource.Id.tvwMsgLeaveExt);
            tvwMsg.Text = "";

            string resumeDatestr = FindViewById<TextView>(Resource.Id.tvwResumeDtExt).Text;
            DateTime? resumeDate = new CommonMethodsClass().ConvertStringToDate(resumeDatestr);
            string ExtendTo = FindViewById<EditText>(Resource.Id.edtExtendTo).Text; //Get End Date
            DateTime? date = new CommonMethodsClass().ConvertStringToDate(ExtendTo);

            string RemainingDays = FindViewById<TextView>(Resource.Id.tvwRemainDaysExt).Text;
            string Duration = FindViewById<TextView>(Resource.Id.tvwDurationExt).Text;
            string reason = FindViewById<EditText>(Resource.Id.edtReasonExt).Text;
            int selection = (int)FindViewById<Spinner>(Resource.Id.spnLeaveExt).SelectedItemId;


            //Get Seklected ID for the Leave

            if (date != null && date > resumeDate && selection > 0)
            {

                if (int.Parse(RemainingDays) >= int.Parse(Duration))
                {
                    try
                    {
                        string restUrl = Values.ApiRootAddress + "LeaveExtension";
                        var contentLeave = new FormUrlEncodedContent(new Dictionary<string, string>
                            {
                                { "ReqNo", RqstNo },
                                { "EmployeeNo",new AppPreferences().GetValue(User.EmployeeNo) },
                                { "AccountingYear", new AppPreferences().GetValue(User.AccountingYear)},
                                { "LeaveGrp", LvGrp },
                                { "LvCode", LevCode },
                                { "RemainingDays" , RemainingDays},
                                { "Duration", Duration},
                                { "ToDate", ExtendTo },
                                { "Reason", reason },
                                { "UserId",new AppPreferences().GetValue(User.UserId) },
                                { "CompId", new AppPreferences().GetValue(User.CompId) },
                                { "RelOfficerId", (int)double.Parse(RelOfficerId)+""},
                                { "PrevReqId", (int)double.Parse(PrevReqId)+""}
                            });

                        (sender as Button).Enabled = false;
                        tvwMsg.BasicMsg(Values.WaitingMsg);
                        dynamic response = await new DataApi().PostAsync(restUrl, contentLeave);
                        tvwMsg.Text = "";                   
                        (sender as Button).Enabled = true;

                        bool success = IsJsonObject(response);
                        if (success)
                        {
                            if (response["Error"] == "")     //success
                            {
                                FindViewById<TextView>(Resource.Id.tvwStrDtExt).Text = "";
                                FindViewById<TextView>(Resource.Id.tvwResumeDtExt).Text = "";
                                FindViewById<TextView>(Resource.Id.tvwDurationExt).Text = "";
                                FindViewById<TextView>(Resource.Id.tvwRemainDaysExt).Text = "";
                                FindViewById<EditText>(Resource.Id.edtExtendTo).Text = "";
                                FindViewById<TextView>(Resource.Id.edtReasonExt).Text = "";
                                FindViewById<Spinner>(Resource.Id.spnLeaveExt).SetSelection(0);
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
                    tvwMsg.ErrorMsg("Day Applied For Cannot Be Greater Than Remaining Days");
                }

            }

            else
            {

                if (date == null) tvwMsg.ErrorMsg("Please choose a valid Resumption Date");
                else if (date <= resumeDate) tvwMsg.ErrorMsg("Extension date must be greater than previous resumption date");
                else if (selection <= 0) tvwMsg.ErrorMsg("Please select a Leave Type");
            }
        }

        public override void OnBackPressed()
        {
            MyOnBackPressed();
        }
    }
}










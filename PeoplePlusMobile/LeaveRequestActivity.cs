using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Widget;
using System.Net.Http;

namespace PeoplePlusMobile
{

    [Activity]
    public class LeaveRequestActivity : BaseActivity
    {
        List<string> leaveitemList = new List<string>();

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.LeaveRequest);

            //set date textview
            TextView tvwMsg = FindViewById<TextView>(Resource.Id.tvwMsgLeaveReq);
            EditText edtStartDate = FindViewById<EditText>(Resource.Id.edtStartDate);
            edtStartDate.Text = new CommonMethodsClass().ConvertDateToString(DateTime.Today);
            edtStartDate.Click += (s, e) =>
            {
                var dateTimeNow = DateTime.Now;
                DatePickerDialog datePicker = new DatePickerDialog
                (this,
                (sender, eventArgs) =>
                {
                    edtStartDate.Text = eventArgs.Date.Day + "/" + eventArgs.Date.Month + "/" + eventArgs.Date.Year;
                    GetWorkDays();
                },
                dateTimeNow.Year, dateTimeNow.Month - 1, dateTimeNow.Day);
                datePicker.Show();
            };
            EditText edtEndDate = FindViewById<EditText>(Resource.Id.edtEndDate);
            // edtEndDate.Text = DateTime.Today.ToString("d");
            edtEndDate.Click += (s, e) =>
            {
                var dateTimeNow = DateTime.Now;
                DatePickerDialog datePicker = new DatePickerDialog
                (this,
                (sender, eventArgs) =>
                {
                    edtEndDate.Text = eventArgs.Date.Day + "/" + eventArgs.Date.Month + "/" + eventArgs.Date.Year;
                    GetWorkDays();
                },
                dateTimeNow.Year, dateTimeNow.Month - 1, dateTimeNow.Day);
                datePicker.Show();
            };

            try
            {
                //to get other parameters to compute number of days from the web api asynchronously
                string restUrl = Values.ApiRootAddress + "LeaveRequest/?compId=" + new AppPreferences().GetValue(User.CompId) + "&EmployeeNo=" + new AppPreferences().GetValue(User.EmployeeNo);
                tvwMsg.BasicMsg(Values.LoadingMsg);
                dynamic response = await new DataApi().GetAsync(restUrl);
                tvwMsg.Text = "";

                if (IsJsonObject(response))
                {
                    response = response["Values"];          //get the values sent by the web api

                    StoreToPref(response, "Leave");              //store to preferences for future access
                    var Leave = GetLeaveType(response);     //list to be used to populate spinner
                    var LeaveAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, Leave);
                    Spinner spnLeave = FindViewById<Spinner>(Resource.Id.spnLeaveReq);
                    spnLeave.Adapter = LeaveAdapter;
                    spnLeave.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spnLeave_ItemSelected);
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

            try
            {
                //to get other parameters to compute number of days from the web api asynchronously
                // string urlMethod = "LeaveRequest" + "/" + "GetReliefOfficer";
                string restUrl2 = Values.ApiRootAddress + "LeaveRequest/GetReliefOfficer/?compId=" + new AppPreferences().GetValue(User.CompId) + "&LocationCode=" + new AppPreferences().GetValue(User.LocationCode) + "&Department=" + new AppPreferences().GetValue(User.DeptCode) + "&EmployeeNo=" + new AppPreferences().GetValue(User.EmployeeNo);

                tvwMsg.BasicMsg(Values.LoadingMsg);
                dynamic response = await new DataApi().GetAsync(restUrl2);
                tvwMsg.Text = "";

                if (IsJsonObject(response))
                {
                    response = response["Values"];          //get the values sent by the web api 

                    StoreToPref(response, "relief");              //store to preferences for future access
                    var ReliefOfficer = GetReliefOfficer(response);     //list to be used to populate spinner
                    var ReliefOfficerAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, ReliefOfficer);
                    Spinner spnReliefOfficer = FindViewById<Spinner>(Resource.Id.spnReliefOfficer);
                    spnReliefOfficer.Adapter = ReliefOfficerAdapter;
                }
                else
                {
                    tvwMsg.ErrorMsg((string)response);
                }
            }
            catch (Exception ex)
            {
                ex.Log();
                tvwMsg.SuccessMsg(Values.ErrorMsg);
            }

            //get submit button and subscribe to its click event
            Button BtnSubmitLeaveReq = FindViewById<Button>(Resource.Id.btnSubmitLeaveReq);
            BtnSubmitLeaveReq.Click += BtnSubmitLeaveReq_Click;        
        }

        public async void GetWorkDays()
        {
            string RemainingDays = FindViewById<TextView>(Resource.Id.tvwNoOfDaysleft).Text;
            TextView tvwMsg = FindViewById<TextView>(Resource.Id.tvwMsgLeaveReq);
            TextView tvwNoOfDays = FindViewById<TextView>(Resource.Id.tvwNoOfDays);
            EditText tvwEndDate = FindViewById<EditText>(Resource.Id.edtEndDate);
            string startDate = FindViewById<EditText>(Resource.Id.edtStartDate).Text;
            string endDate = FindViewById<EditText>(Resource.Id.edtEndDate).Text;

            DateTime? dateStart = ConvertStringToDate(startDate);
            DateTime? dateEnd = ConvertStringToDate(endDate);
            if (dateStart != null && dateEnd != null && dateStart >= DateTime.Today && dateEnd > dateStart)
            {
                string restUrl4 = Values.ApiRootAddress + "LeaveRequest/GetWorkDays/?strFromDate=" + startDate + "&strToDate=" + endDate + "&compId=" + new AppPreferences().GetValue(User.CompId);

                tvwMsg.BasicMsg(Values.WaitingMsg);
                dynamic response = await new DataApi().GetAsync(restUrl4);
                tvwMsg.Text = "";

                if (IsJsonObject(response))
                {
                    int Days = (int)response;
                    int remDays;
                    bool validRemDays = int.TryParse(RemainingDays, out remDays);
                    if (validRemDays && remDays >= Days)
                    {
                        tvwNoOfDays.Text = (int)response + "";
                        tvwMsg.Text = "";
                    }
                    else
                    {
                        tvwEndDate.Text = "";
                        tvwNoOfDays.Text = "";
                        if (!validRemDays) tvwMsg.ErrorMsg("Please select a leave type");
                        else tvwMsg.ErrorMsg("Day Applied For Cannot Be Greater Than Remaining Days");
                    }
                }
                else
                {
                    tvwEndDate.Text = "";
                    tvwNoOfDays.Text = "";
                    tvwMsg.ErrorMsg((string)response);
                }
            }
            else
            {
                tvwEndDate.Text = "";
                tvwNoOfDays.Text = "";
                if (dateStart == null) tvwMsg.ErrorMsg("");
                else if (dateEnd == null) tvwMsg.ErrorMsg("");
                else if (dateStart < DateTime.Today) tvwMsg.ErrorMsg("Start date cannot be less than today's date");
                else tvwMsg.ErrorMsg("End date must be greater than start date");
            }
        }

        //genereate a list of Leave for spinner
        private List<string> GetLeaveType(dynamic response)
        {
            List<string> Leave = new List<string>();

            Leave.Add("Please Select...");
            for (int i = 0; i < response.Count; i++)
            {
                Leave.Add(response[i]["DESCRIPTION"]);

            }

            for (int i = 0; i < response.Count; i++)               //start from one to ensure synchronization
            {
                leaveitemList.Add(response[i]["CODE"]);
            }
            return Leave;
        }

        //genereate a list of ReliefOfficer fopr spinner
        private List<string> GetReliefOfficer(dynamic response)
        {
            List<string> ReliefOfficer = new List<string>();
            ReliefOfficer.Add("Please Select...");
            for (int i = 0; i < response.Count; i++)
            {
                ReliefOfficer.Add(response[i]["name"]);
            }
            return ReliefOfficer;
        }

        //Used to Get Leave Days
        private List<string> GetLeaveDays(dynamic response)
        {
            List<string> LeaveDays = new List<string>();
            for (int i = 0; i < response.Count; i++)
            {
                LeaveDays.Add(response[i]["LvRmn"]);
            }
            return LeaveDays;
        }

        //store Leave values to preference dictionary for submittion access
        private void StoreToPref(dynamic response, string Where)
        {
            if (Where == "Leave")
            {
                AppPreferences LeavePref = new AppPreferences();
                for (int i = 0; i < response.Count; i++)               //start from one to ensure synchronization
                {
                    LeavePref.SaveValue(i + "", response[i]["CODE"]);
                }
            }
            else
            {
                AppPreferences ReliefOfficerID = new AppPreferences();
                for (int i = 0; i < response.Count; i++)               //start from one to ensure synchronization
                {
                    ReliefOfficerID.SaveValue(i + "", response[i]["emplyeno"]);
                }
            }


        }

        //Used to Get and bind Leave to the Spinner
        private async void spnLeave_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {

            Spinner spinner = (Spinner)sender;
            Array Leaveitem = leaveitemList.ToArray();
            if (e.Id != 0)
            {
                TextView tvwNoOfDaysleft = FindViewById<TextView>(Resource.Id.tvwNoOfDaysleft);
                string LeaveCode = Leaveitem.GetValue(e.Id - 1).ToString();
                string restUrl3 = Values.ApiRootAddress + "LeaveRequest/GetLeaveDays/?compId=" + new AppPreferences().GetValue(User.CompId) + "&EmployeeNo=" + new AppPreferences().GetValue(User.EmployeeNo) + "&LeaveCode=" + LeaveCode + "&AccountingYear=" + new AppPreferences().GetValue(User.AccountingYear) + "&StartDate=" + 0 + "&EndDate=" + 0;

                if (spinner.SelectedItem.ToString().ToUpper().StartsWith("ANNUAL")) FindViewById<CheckBox>(Resource.Id.chkAllowance).Enabled = true;
                else FindViewById<CheckBox>(Resource.Id.chkAllowance).Enabled = false;

                string response = await new DataApi().GetAsync(restUrl3);
                //    response = response["values"];          //get the values sent by the web api 
                tvwNoOfDaysleft.Text = response;
            }

        }
        
        //validate all input entry
        private string ValidEntries()
        {
            string msg = "";

            Spinner spnRelOfficer = FindViewById<Spinner>(Resource.Id.spnReliefOfficer);
            Spinner spnLvlType = FindViewById<Spinner>(Resource.Id.spnLeaveReq);
            string RemainingDays = FindViewById<TextView>(Resource.Id.tvwNoOfDaysleft).Text;
            string noOfDays = FindViewById<TextView>(Resource.Id.tvwNoOfDays).Text;

            int outInt;
            if (!(spnRelOfficer.SelectedItemId > 0 && spnLvlType.SelectedItemId > 0 && int.TryParse(RemainingDays, out outInt) &&
                int.TryParse(noOfDays, out outInt) && int.Parse(RemainingDays) >= int.Parse(noOfDays)))
            {
                if (spnLvlType.SelectedItemId <= 0) msg = "Please select a leave type";
                else if (spnRelOfficer.SelectedItemId <= 0) msg = "Please select a relief officer";
                else if (!int.TryParse(RemainingDays, out outInt)) msg = "Please select a leave type";   //repeated for extra caution
                else if (!int.TryParse(noOfDays, out outInt)) msg = "Please select an end date";
                else msg = "Days requested cannot be greater then remaining days";
            }

            return msg;
        }

        //Used To Do variuos checks And To save the records 
        private async void BtnSubmitLeaveReq_Click(object sender, EventArgs e)
        {
            TextView tvwMsg = FindViewById<TextView>(Resource.Id.tvwMsgLeaveReq);
            string msg = ValidEntries();

            if (msg == "")
            {
                string StartdateStr = FindViewById<EditText>(Resource.Id.edtStartDate).Text; // Get Start Date
                string EnddateStr = FindViewById<EditText>(Resource.Id.edtEndDate).Text; //Get End Date

                string Allowance;
                if (FindViewById<CheckBox>(Resource.Id.chkAllowance).Checked) //checkbox item
                    Allowance = "Y";
                else
                    Allowance = "N";

                Spinner spnRelOfficer = FindViewById<Spinner>(Resource.Id.spnReliefOfficer);             //findViewById(R.id.spinner);
                string RemainingDays = FindViewById<TextView>(Resource.Id.tvwNoOfDaysleft).Text;
                string Duration = FindViewById<TextView>(Resource.Id.tvwNoOfDays).Text;
                string reason = FindViewById<EditText>(Resource.Id.edtReasonReq).Text;
                int selection = (int)FindViewById<Spinner>(Resource.Id.spnLeaveReq).SelectedItemId;

                string[] Leaveitem = leaveitemList.ToArray();
                string LeaveCode = Leaveitem.GetValue(selection - 1).ToString();
                // GetActiveLeave(string compId, string LvCode, string EmployeeNo)
                //   string RunningLeave = "";
                string restUrl4 = Values.ApiRootAddress + "LeaveRequest/GetActiveLeave/?compId=" + new AppPreferences().GetValue(User.CompId) + "&LvCode=" + LeaveCode + "&EmployeeNo=" + new AppPreferences().GetValue(User.EmployeeNo);

                try
                {
                    (sender as Button).Enabled = false;
                    tvwMsg.BasicMsg(Values.WaitingMsg);
                    dynamic responseL = await new DataApi().GetAsync(restUrl4);
                    tvwMsg.Text = "";
                    (sender as Button).Enabled = true;

                    bool success = IsJsonObject(responseL);
                    if (success)
                    {
                        string RunningLeave = responseL + "";

                        if (RunningLeave.Length == 0)
                        {
                            string restUrl = Values.ApiRootAddress + "LeaveRequest";
                            var contentLeave = new FormUrlEncodedContent(new Dictionary<string, string>
                            {
                                { "EmployeeNo", new AppPreferences().GetValue(User.EmployeeNo) },
                                { "AccountingYear", new AppPreferences().GetValue(User.AccountingYear)},
                                { "LvCode", LeaveCode },
                                { "RemainingDays" , RemainingDays},
                                { "Duration", Duration},
                                { "StartdateStr", StartdateStr },
                                { "EnddateStr", EnddateStr },
                                { "Reason", reason },
                                { "Allowance",Allowance },
                                { "UserId",new AppPreferences().GetValue(User.UserId) },
                                { "CompId", new AppPreferences().GetValue(User.CompId) },
                                { "ReliefOfficerID", new AppPreferences().GetValue((spnRelOfficer.SelectedItemId - 1) + "")},
                                { "ReliefOfficer", spnRelOfficer.SelectedItem.ToString() }
                            });

                            (sender as Button).Enabled = false;
                            tvwMsg.BasicMsg(Values.WaitingMsg);
                            dynamic response = await new DataApi().PostAsync(restUrl, contentLeave);
                            tvwMsg.Text = "";
                            (sender as Button).Enabled = true;

                            success = DataApi.IsJsonObject(response);
                            if (success)
                            {
                                if (response["Error"] == "")     //success
                                {
                                    FindViewById<TextView>(Resource.Id.tvwNoOfDays).Text = "";
                                    FindViewById<TextView>(Resource.Id.tvwNoOfDaysleft).Text = "";
                                    FindViewById<EditText>(Resource.Id.edtStartDate).Text = "";
                                    FindViewById<EditText>(Resource.Id.edtEndDate).Text = "";
                                    FindViewById<TextView>(Resource.Id.edtReasonReq).Text = "";
                                    FindViewById<Spinner>(Resource.Id.spnLeaveReq).SetSelection(0);
                                    FindViewById<Spinner>(Resource.Id.spnReliefOfficer).SetSelection(0);
                                    FindViewById<CheckBox>(Resource.Id.chkAllowance).Checked = false;
                                    FindViewById<CheckBox>(Resource.Id.chkAllowance).Enabled = false;
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

                        else
                        {
                            tvwMsg.ErrorMsg(RunningLeave);
                        }
                    }
                    else
                    {
                        tvwMsg.ErrorMsg((string)responseL);
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
                tvwMsg.ErrorMsg(msg);
            }
        }

        public override void OnBackPressed()
        {
            MyOnBackPressed();
        }
    }
}










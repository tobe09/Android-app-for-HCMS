using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using System.Threading.Tasks;
using System.Net.Http;

namespace PeoplePlusMobile
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class WorkListActivity : BaseActivity
    {
        ApprovalAdapter adapter;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.WorkListLayout);

            if (!User.IsValidUser())
            {
                StartActivity(typeof(MainActivity));
                return;
            }

            TextView tvwMainMsg = FindViewById<TextView>(Resource.Id.tvwWkvMsg);

            (GetSystemService(NotificationService) as NotificationManager).Cancel(0);       //cancel all related notifications

            try
            {
                tvwMainMsg.Text = Values.LoadingMsg;
                string restUrl = Values.ApiRootAddress + "Approval?compId=" + new AppPreferences().GetValue(User.CompId) + "&empNo=" +
                    new AppPreferences().GetValue(User.EmployeeNo);
                dynamic response = await new DataApi().GetAsync(restUrl);
                tvwMainMsg.Text = "";

                bool success = DataApi.IsJsonObject(response);
                if (success)
                {
                    if (response["Error"] == "")
                    {
                        response = response["Values"];

                        List<object> appraisees = new List<object>();
                        for (int i = 0; i < response.Count; i++)
                        {
                            Appraisee appraisee = new Appraisee();
                            appraisee.SerialNo = i + 1;
                            appraisee.Message = response[i]["Description"];
                            appraisee.ApprovalType = response[i]["Approval_Type"];
                            appraisee.SentBy = response[i]["Created By"];
                            appraisee.DateSent = response[i]["Date"];
                            appraisee.Alertid = response[i]["Alert_Id"];
                            appraisee.RequestId = response[i]["REQUEST"] ?? 0;

                            appraisees.Add(appraisee);
                        }

                        tvwMainMsg.SuccessMsg("Outstanding Approvals: " + appraisees.Count);

                        adapter = new ApprovalAdapter(appraisees);
                        adapter.ItemClick += async (sender, position) =>
                        {
                            try
                            {
                                Appraisee appraisee = (Appraisee)adapter[position];

                                if (appraisee.ApprovalType == "Medical")
                                {
                                    restUrl = Values.ApiRootAddress + "Approval/GetMedicalRequest?compId=" + new AppPreferences().GetValue(User.CompId) + "&empNo=" +
                                    new AppPreferences().GetValue(User.EmployeeNo) + "&accYear=" + new AppPreferences().GetValue(User.AccountingYear) + "&reqId=" +
                                    appraisee.RequestId;

                                    response = await new DataApi().GetAsync(restUrl);

                                    if (response["Error"] == "")
                                    {
                                        response = response["Values"];

                                        var view = LayoutInflater.Inflate(Resource.Layout.MedApprovalDialogLayout, null);

                                        AlertDialog dialog = new AlertDialog.Builder(this).Create();
                                        dialog.SetTitle("MEDICAL APPROVAL");

                                        view.FindViewById<TextView>(Resource.Id.tvwMedEmpName).Text = response[0]["HEMP_EMPLYE_NAME"];
                                        view.FindViewById<TextView>(Resource.Id.tvwMedReqDate).Text = response[0]["Date"];
                                        view.FindViewById<TextView>(Resource.Id.tvwMedHospital).Text = response[0]["HOSP_NAME"];
                                        view.FindViewById<TextView>(Resource.Id.tvwMedLimBal).Text = response[0]["LIMITBAL"];
                                        view.FindViewById<TextView>(Resource.Id.tvwMedUsed).Text = response[0]["USED"];
                                        view.FindViewById<TextView>(Resource.Id.tvwMedReason).Text = response[0]["REQ_REASON"];

                                        dialog.SetView(view);

                                        dialog.SetButton("Approve", async (s, e) =>
                                        {
                                            try
                                            {
                                                string comment = view.FindViewById<EditText>(Resource.Id.edtMedComment).Text;

                                                string status = await PostMedical(appraisee, response, "A", comment);
                                                if (status == "")
                                                {
                                                    adapter.Remove(position);
                                                    dialog.Dismiss();
                                                    tvwMainMsg.SuccessMsg(Values.SuccessMsg);
                                                }
                                                else
                                                {
                                                    dialog.Dismiss();
                                                    tvwMainMsg.ErrorMsg(status);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                ex.Log();
                                                tvwMainMsg.ErrorMsg(Values.ErrorMsg);
                                            }
                                        });

                                        dialog.SetButton2("Reject", async (s, e) =>
                                        {
                                            try
                                            {
                                                string comment = view.FindViewById<EditText>(Resource.Id.edtMedComment).Text;

                                                string status = await PostMedical(appraisee, response, "D", comment);
                                                if (status == "")
                                                {
                                                    adapter.Remove(position);
                                                    dialog.Dismiss();
                                                    tvwMainMsg.SuccessMsg(Values.SuccessMsg);
                                                }
                                                else
                                                {
                                                    dialog.Dismiss();
                                                    tvwMainMsg.ErrorMsg(status);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                ex.Log();
                                                tvwMainMsg.ErrorMsg(Values.ErrorMsg);
                                            }
                                        });

                                        dialog.SetButton3("Cancel", (s, e) =>
                                        {
                                            dialog.Dismiss();
                                            tvwMainMsg.SuccessMsg("Outstanding Approvals: " + adapter.ItemCount);
                                        });

                                        dialog.Show();
                                    }
                                    else
                                    {
                                        tvwMainMsg.ErrorMsg((string)response["Error"]);
                                    }
                                }

                                else if (appraisee.ApprovalType == "Leave")
                                {
                                    restUrl = Values.ApiRootAddress + "Approval/GetLeaveRequest?compId=" + new AppPreferences().GetValue(User.CompId) + "&empNo=" +
                                    new AppPreferences().GetValue(User.EmployeeNo) + "&reqId=" + appraisee.RequestId;

                                    response = await new DataApi().GetAsync(restUrl);

                                    if (response["Error"] == "")
                                    {
                                        response = response["Values"];

                                        var view = LayoutInflater.Inflate(Resource.Layout.LvlApprovalDialogLayout, null);

                                        AlertDialog dialog = new AlertDialog.Builder(this).Create();
                                        dialog.SetTitle("LEAVE APPROVAL");

                                        view.FindViewById<TextView>(Resource.Id.tvwLvlEmpName).Text = response[0]["EmpName"];
                                        view.FindViewById<TextView>(Resource.Id.tvwLvlEmpNo).Text = (int)response[0]["EmpNo"] + "";
                                        view.FindViewById<TextView>(Resource.Id.tvwLvlDesc).Text = response[0]["LvDesc"];
                                        view.FindViewById<TextView>(Resource.Id.tvwLvlStartDate).Text = response[0]["StrtDt"];
                                        view.FindViewById<TextView>(Resource.Id.tvwLvlEndDate).Text = response[0]["EndDt"];
                                        view.FindViewById<TextView>(Resource.Id.tvwLvlNoOfDays).Text = (int)response[0]["NoOfDays"] + "";
                                        view.FindViewById<TextView>(Resource.Id.tvwLvlReason).Text = response[0]["Reason"];

                                        dialog.SetView(view);

                                        dialog.SetButton("Approve", async (s, e) =>
                                        {
                                            try
                                            {
                                                string comment = view.FindViewById<EditText>(Resource.Id.edtLvlComment).Text;

                                                string status = await PostLeave(appraisee, response, "A", comment);
                                                if (status == "")
                                                {
                                                    adapter.Remove(position);
                                                    dialog.Dismiss();
                                                    tvwMainMsg.SuccessMsg(Values.SuccessMsg);
                                                }
                                                else
                                                {
                                                    dialog.Dismiss();
                                                    tvwMainMsg.ErrorMsg(status);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                ex.Log();
                                                tvwMainMsg.ErrorMsg(Values.ErrorMsg);
                                            }
                                        });

                                        dialog.SetButton2("Reject", async (s, e) =>
                                        {
                                            try
                                            {
                                                string comment = view.FindViewById<EditText>(Resource.Id.edtLvlComment).Text;

                                                string status = await PostLeave(appraisee, response, "D", comment);
                                                if (status == "")
                                                {
                                                    adapter.Remove(position);
                                                    dialog.Dismiss();
                                                    tvwMainMsg.SuccessMsg(Values.SuccessMsg);
                                                }
                                                else
                                                {
                                                    dialog.Dismiss();
                                                    tvwMainMsg.ErrorMsg(status);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                ex.Log();
                                                tvwMainMsg.ErrorMsg(Values.ErrorMsg);
                                            }
                                        });

                                        dialog.SetButton3("Cancel", (s, e) =>
                                        {
                                            dialog.Dismiss();
                                            tvwMainMsg.SuccessMsg("Outstanding Approvals: " + adapter.ItemCount);
                                        });

                                        dialog.Show();
                                    }
                                    else
                                    {
                                        tvwMainMsg.ErrorMsg((string)response["Error"]);
                                    }
                                }

                                else if (appraisee.ApprovalType == "Training")
                                {
                                    restUrl = Values.ApiRootAddress + "Approval/GetTrainingRequest?compId=" + new AppPreferences().GetValue(User.CompId) + "&empNo=" +
                                    new AppPreferences().GetValue(User.EmployeeNo) + "&reqId=" + appraisee.RequestId;

                                    response = await new DataApi().GetAsync(restUrl);
                                    if (response["Error"] == "")
                                    {
                                        response = response["Values"];

                                        var view = LayoutInflater.Inflate(Resource.Layout.TrnApprovalDialogLayout, null);

                                        AlertDialog dialog = new AlertDialog.Builder(this).Create();
                                        dialog.SetTitle("TRAINING APPROVAL");

                                        view.FindViewById<TextView>(Resource.Id.tvwTrnEmpName).Text = response[0]["Employee Name"];
                                        view.FindViewById<TextView>(Resource.Id.tvwTrnDesc).Text = response[0]["Training Description"];
                                        view.FindViewById<TextView>(Resource.Id.tvwTrnReason).Text = response[0]["Reason"];
                                        view.FindViewById<TextView>(Resource.Id.tvwTrnNomBy).Text = response[0]["Nominating Emp Name"];

                                        dialog.SetView(view);

                                        dialog.SetButton("Approve", async (s, e) =>
                                        {
                                            try
                                            {
                                                string comment = view.FindViewById<EditText>(Resource.Id.edtTrnComment).Text;

                                                string status = await PostTraining(appraisee, response, "A", comment);
                                                if (status == "")
                                                {
                                                    adapter.Remove(position);
                                                    dialog.Dismiss();
                                                    tvwMainMsg.SuccessMsg(Values.SuccessMsg);
                                                }
                                                else
                                                {
                                                    dialog.Dismiss();
                                                    tvwMainMsg.ErrorMsg(status);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                ex.Log();
                                                tvwMainMsg.ErrorMsg(Values.ErrorMsg);
                                            }
                                        });

                                        dialog.SetButton2("Reject", async (s, e) =>
                                        {
                                            try
                                            {
                                                string comment = view.FindViewById<EditText>(Resource.Id.edtTrnComment).Text;

                                                string status = await PostTraining(appraisee, response, "D", comment);
                                                if (status == "")
                                                {
                                                    adapter.Remove(position);
                                                    dialog.Dismiss();
                                                    tvwMainMsg.SuccessMsg(Values.SuccessMsg);
                                                }
                                                else
                                                {
                                                    dialog.Dismiss();
                                                    tvwMainMsg.ErrorMsg(status);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                ex.Log();
                                                tvwMainMsg.ErrorMsg(Values.ErrorMsg);
                                            }
                                        });

                                        dialog.SetButton3("Cancel", (s, e) =>
                                        {
                                            dialog.Dismiss();
                                            tvwMainMsg.SuccessMsg("Outstanding Approvals: " + adapter.ItemCount);
                                        });

                                        dialog.Show();
                                    }
                                    else
                                    {
                                        tvwMainMsg.ErrorMsg((string)response["Error"]);
                                    }
                                }

                                else
                                {
                                    tvwMainMsg.ErrorMsg("Request type not provisioned");
                                }
                            }
                            catch(Exception ex)
                            {
                                ex.Log();
                                tvwMainMsg.ErrorMsg(Values.ErrorMsg);
                            }
                        };
                        
                        RecyclerView recyclerView = (RecyclerView)FindViewById(Resource.Id.rvwWorkListViewer);
                        recyclerView.SetLayoutManager(new LinearLayoutManager(this));
                        recyclerView.SetAdapter(adapter);
                    }
                    else
                    {
                        tvwMainMsg.ErrorMsg((string)response["Error"]);
                    }
                }
                else
                {
                    tvwMainMsg.ErrorMsg((string)response);
                }
            }
            catch (Exception ex)
            {
                ex.Log();
                tvwMainMsg.ErrorMsg(Values.ErrorMsg);
            }
        }

        public async Task<string> PostMedical(Appraisee appraisee, dynamic response, string status, string comment)
        {
            string restUrl = Values.ApiRootAddress + "Approval";

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "EmployeeNo", new AppPreferences().GetValue(User.EmployeeNo) },
                    { "RequestId", appraisee.RequestId + "" },
                    { "Reason", comment },
                    {"UserName", new AppPreferences().GetValue(User.UserId) },
                    {"CompanyCode", new AppPreferences().GetValue(User.CompId) },
                    {"ApproveeNo", (int)response[0]["REQ_EMP_ID"] + ""},
                    {"Status", status},
                    {"AlertId", appraisee.Alertid + "" },
                    {"RequestType", appraisee.ApprovalType },
                    {"RequestNo", (string)response[0]["REQ_NO"] }
                });

            response = await new DataApi().PostAsync(restUrl, content);

            bool success = DataApi.IsJsonObject(response); 

            if (success) return response["Error"];
            else return (string)response;
        }

        public async Task<string> PostLeave(Appraisee appraisee, dynamic response, string status, string comment)
        {
            string restUrl = Values.ApiRootAddress + "Approval";

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "EmployeeNo", new AppPreferences().GetValue(User.EmployeeNo) },
                    { "RequestId", appraisee.RequestId+"" },
                    { "Reason", comment },
                    {"UserName",new AppPreferences().GetValue(User.UserId) },
                    {"CompanyCode", new AppPreferences().GetValue(User.CompId) },
                    {"ApproveeNo", (int)response[0]["EmpNo"] + ""},
                    {"Status", status},
                    {"AlertId", appraisee.Alertid+"" },
                    {"RequestType",appraisee.ApprovalType },
                    {"RequestNo", response[0]["Rqst_No"].ToString() },
                    {"ProcessFrom", DateTime.Parse((string)response[0]["StrtDt"]).ToString("dd-MMM-yyyy") },
                    {"ProcessTo", DateTime.Parse((string)response[0]["EndDt"]).ToString("dd-MMM-yyyy") },
                    {"DayStart", DateTime.Parse((string)response[0]["StrtDt"]).ToString("dd-MMM-yyyy") },
                    {"DayEnd", DateTime.Parse((string)response[0]["EndDt"]).ToString("dd-MMM-yyyy")  },
                    {"DayDiff", (int)response[0]["NoOfDays"]+""  }
            });

            response = await new DataApi().PostAsync(restUrl, content);

            bool success = DataApi.IsJsonObject(response); // || response.ToString() == Values.ServerErrorMsg;

            if (success) return response["Error"];
            else return (string)response;
        }

        public async Task<string> PostTraining(Appraisee appraisee, dynamic response, string status, string comment)
        {
            string restUrl = Values.ApiRootAddress + "Approval";

            int outInt;
            string approveeNo = response[0]["Employee No"].ToString();
            approveeNo = int.TryParse(approveeNo, out outInt) ? approveeNo : approveeNo.Remove(approveeNo.Length - 2);
            string serialNo = response[0]["SrNo"].ToString();
            serialNo = int.TryParse(serialNo, out outInt) ? serialNo : serialNo.Remove(serialNo.Length - 2);

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "EmployeeNo", new AppPreferences().GetValue(User.EmployeeNo) },
                    { "RequestId", appraisee.RequestId+"" },
                    { "Reason", comment },
                    {"UserName",new AppPreferences().GetValue(User.UserId) },
                    {"CompanyCode", new AppPreferences().GetValue(User.CompId) },
                    {"ApproveeNo", approveeNo},
                    {"Status", status},
                    {"AlertId", appraisee.Alertid + "" },
                    {"RequestType",appraisee.ApprovalType },
                    {"SerialNo", serialNo},
                    {"ProgramCode", (string)response[0]["Training"] }
                });

            response = await new DataApi().PostAsync(restUrl, content);

            bool success = DataApi.IsJsonObject(response);

            if (success) return response["Error"];
            else return (string)response;
        }

        public override void OnBackPressed()
        {
            MyOnBackPressed();
        }
    }


    public class Appraisee
    {
        public int SerialNo { get; set; }
        public string Message { get; set; }
        public string ApprovalType { get; set; }
        public string SentBy { get; set; }
        public string DateSent { get; set; }
        public int Alertid { get; set; }
        public int RequestId { get; set; }
    }


    public class WorkListViewHolder : RecyclerView.ViewHolder
    {
        public TextView TvwSerialNo { get; set; }
        public TextView TvwMsg { get; set; }
        public TextView TvwApprovalType { get; set; }
        public TextView TvwSentBy { get; set; }
        public TextView TvwDateSent { get; set; }

        public WorkListViewHolder(View itemView, Action<int> listener) : base(itemView)
        {
            TvwSerialNo = itemView.FindViewById<TextView>(Resource.Id.tvwWkvSerialNo);
            TvwMsg = itemView.FindViewById<TextView>(Resource.Id.tvwWkvMsgInfo);
            TvwApprovalType = itemView.FindViewById<TextView>(Resource.Id.tvwWkvType);
            TvwSentBy = itemView.FindViewById<TextView>(Resource.Id.tvwWkvSentBy);
            TvwDateSent = itemView.FindViewById<TextView>(Resource.Id.tvwWkvDateSent);

            itemView.Click += (s, e) => listener(LayoutPosition);
        }
    }


    public class ApprovalAdapter : BaseRecyclerViewAdapter
    {
        public ApprovalAdapter(List<object> appraisees) : base(appraisees) { }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            WorkListViewHolder vh = holder as WorkListViewHolder;
            Appraisee appraisee = BaseObjects[position] as Appraisee;
            vh.TvwSerialNo.Text = appraisee.SerialNo + "";
            vh.TvwMsg.Text = appraisee.Message;
            vh.TvwApprovalType.Text = appraisee.ApprovalType;
            vh.TvwSentBy.Text = appraisee.SentBy;
            vh.TvwDateSent.Text = appraisee.DateSent;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.WorkListFragment, parent, false);
            WorkListViewHolder vh = new WorkListViewHolder(itemView, OnClick);
            return vh;
        }
    }


    public class BaseRecyclerViewAdapter : RecyclerView.Adapter
    {
        public List<object> BaseObjects { get; set; }

        public event EventHandler<int> ItemClick;

        public BaseRecyclerViewAdapter(List<object> objects) : base()
        {
            BaseObjects = objects;
        }

        public override int ItemCount
        {
            get { return BaseObjects.Count; }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            throw new NotImplementedException("Override The Method OnBindViewHolder");
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            throw new NotImplementedException("Override The Method OnCreateViewHolder");
        }

        public virtual object this[int position]
        {
            get { return BaseObjects[position]; }
        }

        public virtual void Add(object obj)
        {
            BaseObjects.Add(obj);
            NotifyItemInserted(ItemCount);
        }

        public virtual void Insert(object obj, int position)
        {
            if (position < ItemCount)
            {
                BaseObjects.Insert(position, obj);
                NotifyItemInserted(position);
            }
            else
            {
                Add(obj);
            }
        }

        public virtual void Remove(int position)
        {
            BaseObjects.RemoveAt(position);
            NotifyItemRemoved(position);
        }

        public virtual void Update(object obj, int position)
        {
            BaseObjects.RemoveAt(position);
            BaseObjects.Insert(position, obj);
            NotifyItemChanged(position);
        }

        public void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }
    }
}
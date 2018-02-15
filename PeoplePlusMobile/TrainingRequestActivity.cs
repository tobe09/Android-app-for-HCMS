using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System.Json;
using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Content;
using Android.Preferences;
using System.Threading.Tasks;
using System.Net.Http;

namespace PeoplePlusMobile
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class TrainingRequestActivity: BaseActivity
    {

        RecyclerView mRecyclerView;
        RecyclerView.LayoutManager mLayoutManager;
        EmpDetailsAdapter mAdapter;
        List<EmployeeInfo> mDatasource = new List<EmployeeInfo>();

        List<SelectedTraining> selectedTraining = new List<SelectedTraining>();

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.TrainingRequestLayout);

            try
            {
                TextView tvwResultMsg = FindViewById<TextView>(Resource.Id.tvwMsgTrnReq);
                string url = Values.ApiRootAddress + "training/getTrainings?compId=" + new AppPreferences().GetValue(User.CompId);

                tvwResultMsg.BasicMsg(Values.LoadingMsg);
                dynamic json = await new DataApi().GetAsync(url);
                tvwResultMsg.Text = "";
                if (IsJsonObject(json))
                {
                    JsonValue TrainingResults = json["Training"];

                    List<string> lstTrainingCode = new List<string>();
                    lstTrainingCode.Add("Nothing");
                    List<string> lstTrainingDesc = new List<string>();
                    lstTrainingDesc.Add("Please Select");

                    for (int i = 0; i < TrainingResults.Count; i++)
                    {
                        lstTrainingCode.Add(TrainingResults[i]["TRAININGCODE"]);
                        lstTrainingDesc.Add(TrainingResults[i]["DESCRIPTION"]);

                        SelectedTraining obj = new SelectedTraining();

                        obj.TrainingCode = TrainingResults[i]["TRAININGCODE"];
                        obj.Description = TrainingResults[i]["DESCRIPTION"];
                        obj.Duration = TrainingResults[i]["DURATION"];
                        obj.Type = TrainingResults[i]["TYPE"];
                        obj.TrainingSerialNo = TrainingResults[i]["TRNSNO"];
                        obj.Venue = TrainingResults[i]["VENUE"];

                        selectedTraining.Add(obj);
                    };

                    Spinner spnrTrainings = FindViewById<Spinner>(Resource.Id.spnrTrainingCode);

                    ArrayAdapter<string> spinnerArrayAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, lstTrainingDesc);
                    //Use the ArrayAdapter you've set up to populate your spinner
                    spnrTrainings.Adapter = spinnerArrayAdapter;
                    //optionally pre-set spinner to an index
                    spnrTrainings.SetSelection(0);

                    spnrTrainings.ItemSelected += async (sender, e) =>
                  {
                      Spinner spinner = (Spinner)sender;

                      //save the CODE and SERIALNO of training selected
                      ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
                      ISharedPreferencesEditor editor = prefs.Edit();
                      editor.PutString("TrainingCode", lstTrainingCode[e.Position]);

                      //var position = e.Position.Equals(0) ? 0 : e.Position - 1;
                      if (e.Position > 0)
                      {
                          editor.PutInt("TrainingSerialNo", selectedTraining[e.Position - 1].TrainingSerialNo);
                          editor.Apply();
                      }

                      if (mDatasource.Count > 0)
                      {
                          mDatasource.Clear();
                      }

                      AppPreferences appPrefs = new AppPreferences();
                      tvwResultMsg.BasicMsg(Values.LoadingMsg);
                      mDatasource = await Nominees(lstTrainingCode[e.Position], appPrefs.GetValue(User.EmployeeNo), appPrefs.GetValue(User.CompId));
                      tvwResultMsg.Text = "";
                      //mDatasource = await new GetNominees().Nominees(spinner.GetItemAtPosition(e.Position).ToString());
                      if (mDatasource.Count == 0 && spnrTrainings.SelectedItemPosition != 0) tvwResultMsg.ErrorMsg("No Employee To Nominate");
                      else tvwResultMsg.Text = "";

                      mAdapter = new EmpDetailsAdapter(mDatasource);
                      mAdapter.ItemClick += (s, args) =>
                      {

                          //Toast.MakeText(this, "who's the BOSS now!", ToastLength.Short).Show();
                          //Toast.MakeText(this, "who's the BOSS now!" + mDatasource[position].name, ToastLength.Short).Show();

                          editor.PutString("SelectedName", mDatasource[args].name);
                          editor.PutInt("NomineeEmployeeNo", mDatasource[args].number);
                          editor.Apply();

                          FragmentTransaction transcation = FragmentManager.BeginTransaction();
                          Dialogclass ConfirmNominee = new Dialogclass();
                          ConfirmNominee.DialogClosed += (dlgSender, dlgEvent) =>
                          {
                              if (dlgEvent)
                              {
                                  mAdapter.empList.RemoveAt(args);
                                  mAdapter.NotifyItemRemoved(args);
                              }
                          };
                          ConfirmNominee.Show(transcation, "Dialog Fragment");

                      };

                      mRecyclerView = (RecyclerView)FindViewById(Resource.Id.recyclerView1);
                      mLayoutManager = new LinearLayoutManager(this);
                      mRecyclerView.SetLayoutManager(mLayoutManager);
                      mRecyclerView.SetAdapter(mAdapter);

                      if (e.Position > 0)
                      {
                          Toast.MakeText(this, "Training Venue: " + selectedTraining[e.Position - 1].Venue + "\r\n" + "Training Duration: " + selectedTraining[e.Position - 1].Duration, ToastLength.Short).Show();
                      }
                  };
                }
                else
                {
                    tvwResultMsg.ErrorMsg((string)json);
                }
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        public async Task<List<EmployeeInfo>> Nominees(string code, string empNo, string compId)
        {
            List<EmployeeInfo> myEmployeeLst = new List<EmployeeInfo>();
            string url = Values.ApiRootAddress + "training/getNominees?TrainingCode=" + code + "&empNo=" + empNo + "&compId=" + compId;
            dynamic json = await new DataApi().GetAsync(url);
            if (IsJsonObject(json))
            {
                JsonValue NomineeResults = json["Nominees"];

                var b = json["Nominees"].Count;

                if (json["Nominees"].Count > 0)
                {
                    for (int i = 0; i < NomineeResults.Count; i++)
                    {
                        if (NomineeResults[i]["NAME"] != null)
                        {
                            myEmployeeLst.Add(new EmployeeInfo { name = NomineeResults[i]["NAME"], number = NomineeResults[i]["STAFFID"], grade = NomineeResults[i]["GRADE"], designation = NomineeResults[i]["DESIGNATION"], department = NomineeResults[i]["DEPARTMENT"] });
                        }

                    }
                }
            }
            else
            {
                FindViewById<TextView>(Resource.Id.tvwMsgTrnReq).ErrorMsg((string)json);
            }
            return myEmployeeLst;
        }

        public override void OnBackPressed()
        {
            MyOnBackPressed();
        }
    }


    public class EmployeeInfo
    {
        public string name { get; set; }
        public int number { get; set; }
        public string grade { get; set; }
        public string department { get; set; }
        public string designation { get; set; }
    }
    

    public class EmpDetailsAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;

        public List<EmployeeInfo> empList;

        public EmpDetailsAdapter(List<EmployeeInfo> empList)
        {
            this.empList = empList;
        }


        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.TrainingRecyclerRows, parent, false);
            ItemViewHolder vh = new ItemViewHolder(itemView, OnLongClick);
            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ItemViewHolder vh = holder as ItemViewHolder;

            //get image from database to populate here
            vh.Image.SetImageResource(Resource.Drawable.defaultprofile);


            vh.Name.Text = empList[position].name;
            vh.Department.Text = empList[position].department;
            vh.Grade.Text = empList[position].grade;
            vh.Designation.Text = empList[position].designation;
        }

        public override int ItemCount
        {
            get { return empList.Count; }
        }

        void OnLongClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }

    }


    public class ItemViewHolder : RecyclerView.ViewHolder
    {
        public ImageView Image { get; private set; }
        public TextView Name { get; private set; }
        public TextView Department { get; private set; }
        public TextView Grade { get; private set; }
        public TextView Designation { get; private set; }

        public ItemViewHolder(View itemView, Action<int> listener) : base(itemView)
        {
            //Locate and cache view references:
            Image = itemView.FindViewById<ImageView>(Resource.Id.ImageView1);
            Name = itemView.FindViewById<TextView>(Resource.Id.tvName);
            Department = itemView.FindViewById<TextView>(Resource.Id.tvDepartment);
            Grade = itemView.FindViewById<TextView>(Resource.Id.tvGrade);
            Designation = itemView.FindViewById<TextView>(Resource.Id.tvDesignation);

            //set the longclick event of the view
            //itemView.LongClick  += (sender, e) => listener(LayoutPosition);
            itemView.Click += (sender, e) => listener(LayoutPosition);
        }
    }


    //creating a dialog to confirm nomination
    class Dialogclass : DialogFragment
    {
        public event EventHandler<bool> DialogClosed;

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            //get the selected name from shared preference
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            var Name = prefs.GetString("SelectedName", "");

            var view = Activity.LayoutInflater.Inflate(Resource.Layout.TrainingNominateDialog, null);
            TextView tvMessage = view.FindViewById<TextView>(Resource.Id.textView2);
            TextView tvwResultMsg = Activity.FindViewById<TextView>(Resource.Id.tvwMsgTrnReq);

            AlertDialog.Builder alert = new AlertDialog.Builder(Activity);
            alert.SetTitle("Confirm selection");

            string Message = "are you nominating " + Name + " for the training?";
            //alert.SetMessage(Message);
            tvMessage.Text = Message;

            string uri = Values.ApiRootAddress + "training/SaveNominee";
            alert.SetPositiveButton("Nominate", async (senderAlert, args) =>
            {
                AppPreferences appPrefs = new AppPreferences();
                var NominateUser = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "CompanyCode", appPrefs.GetValue(User.CompId) }, // "CO000001",
                    { "TrainingCode", prefs.GetString("TrainingCode", "") },
                    { "TrainingSerialNo", prefs.GetInt("TrainingSerialNo", 0) + "" }, // 1,
                    { "NomineeEmployeeNo", prefs.GetInt("NomineeEmployeeNo", 0) + "" }, //43,
                    { "ReqEmployeeNo", appPrefs.GetValue(User.EmployeeNo) }, // 694,
                    { "Reason", view.FindViewById<EditText>(Resource.Id.edtTxtReason).Text },
                    { "Username", appPrefs.GetValue(User.UserId) }
                });

                tvwResultMsg.Text = Values.WaitingMsg;
                dynamic json = await new DataApi().PostAsync(uri, NominateUser);
                tvwResultMsg.Text = "";
                if (DataApi.IsJsonObject(json))
                {
                    if (json != null)
                    {
                        if (json["status"] == 0)
                        {
                            DialogClosed?.Invoke(this, true);
                            tvwResultMsg.SuccessMsg((string)json["message"]);
                        }
                        else
                        {
                            tvwResultMsg.ErrorMsg((string)json["message"]);
                        }
                        //Toast.MakeText(new Activity(), json["message"].ToString(), ToastLength.Short).Show();
                    }
                }
                else
                {
                    tvwResultMsg.ErrorMsg((string)json);
                }
                Dismiss();
            });

            alert.SetNegativeButton("Cancel", (senderAlert, args) =>
            {
                Dismiss();
                //Toast.MakeText(Activity, "Cancelled!", ToastLength.Short).Show();
            });

            alert.SetView(view);
            return alert.Create();
        }
    }


    public class TrainingParam
    {
        private string _companyCode;
        private string _trainingCode;
        private string _reason;
        private string _username;

        public string CompanyCode { get { return _companyCode; } set { _companyCode = value.ToUpper(); } }
        public string TrainingCode { get { return _trainingCode; } set { _trainingCode = value.ToUpper(); } }
        public int TrainingSerialNo { get; set; }
        public int NomineeEmployeeNo { get; set; }
        public int ReqEmployeeNo { get; set; }
        public string Reason { get { return _reason; } set { _reason = value.ToUpper(); } }
        public string Username { get { return _username; } set { _username = value.ToUpper(); } }
    }


    public class SelectedTraining
    {
        private string _trainingCode;
        private string _description;
        private string _type;
        private string _duration;
        private string _venue;

        public string TrainingCode { get { return _trainingCode; } set { _trainingCode = value.ToUpper(); } }
        public string Description { get { return _description; } set { _description = value.ToUpper(); } }
        public string Type { get { return _type; } set { _type = value.ToUpper(); } }
        public string Duration { get { return _duration; } set { _duration = value.ToUpper(); } }
        public int TrainingSerialNo { get; set; }
        public string Venue { get { return _venue; } set { _venue = value.ToUpper(); } }
    }

}


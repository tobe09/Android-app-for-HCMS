using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace PeoplePlusMobile
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class HomeActivity : BaseActivity
    {
        //int count = 0;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Home);
            
            NewLayoutSetup();

            ExpandableListView expListView = FindViewById<ExpandableListView>(Resource.Id.expandableListView1);
            //expListView.Visibility = ViewStates.Invisible;

            //Bind list
            List<string> parentList = GenerateParents();
            Dictionary<string, List<string>> childDictionaryList = GenerateChildren(parentList);
            ExpandableListAdapter listAdapter = new ExpandableListAdapter(this, parentList, childDictionaryList);
            expListView.SetAdapter(listAdapter);

            //expListView.GroupClick += (sender, eventArgs) =>
            //  {
            //      if (eventArgs.GroupPosition == 2) StartActivity(typeof(AbsenteeismActivity));
            //  };

            //Listening to child item selection
            expListView.ChildClick += delegate (object sender, ExpandableListView.ChildClickEventArgs e)
            {
                int parent = e.GroupPosition;
                int child = e.ChildPosition;

                switch (parent)
                {
                    case 0:     //leave
                        if (child == 0) { StartActivity(typeof(LeaveRequestActivity)); }          //request
                        else if (child == 1) { StartActivity(typeof(LeaveExtensionActivity)); }    //recall
                        else if (child == 2) { StartActivity(typeof(LeaveRecallActivity)); }    //extension
                        else { StartActivity(typeof(MainActivity)); }                   //exceptional
                        break;

                    case 1:     //medical
                        if (child == 0) { StartActivity(typeof(MedicalRequestActivity)); }       //request
                        else { StartActivity(typeof(MainActivity)); }               //approval
                        break;

                    case 2:     //absenteeism
                        if (child == 0) StartActivity(typeof(AbsenteeismActivity));
                        else StartActivity(typeof(UserProfileActivity));
                        break;

                    case 3:     //training
                        if (child == 0) { StartActivity(typeof(TrainingRequestActivity)); }    //request
                        else { StartActivity(typeof(MainActivity)); }               //approval
                        break;

                    default:
                        break;
                }
            };

            ////Listening to group expand //modified so that on selection of one group other opened group has been closed
            //expListView.GroupExpand += delegate (object sender, ExpandableListView.GroupExpandEventArgs e)
            //{
            //    int previousGroup = -1;
            //    if (e.GroupPosition != previousGroup) expListView.CollapseGroup(previousGroup);
            //    previousGroup = e.GroupPosition;
            //    Toast.MakeText(this, "Group expanded", ToastLength.Short).Show();
            //};

            ////Listening to group collapse
            //expListView.GroupCollapse += delegate (object sender, ExpandableListView.GroupCollapseEventArgs e)
            //{
            //    Toast.MakeText(this, "Group collapsed", ToastLength.Short).Show();
            //};
        }

        protected async override void OnStart()
        {
            base.OnStart();
            Button btnWkl = FindViewById<Button>(Resource.Id.btnCountHome);
            string restUrl = Values.ApiRootAddress + "Approval/GetCount?compId=" + new AppPreferences().GetValue(User.CompId) +
                "&empNo=" + new AppPreferences().GetValue(User.EmployeeNo);
            dynamic result = await new DataApi().GetAsync(restUrl);
            if (IsJsonObject(result))
            {
                btnWkl.Text = (int)result["Count"] + "";
            }
            else
            {
                btnWkl.Text = "0";
            }
        }

        protected void NewLayoutSetup()
        {
            FindViewById<TextView>(Resource.Id.tvwWkListVwr).Click += (s, e) => StartActivity(typeof(WorkListActivity));
            FindViewById<TextView>(Resource.Id.tvwMedReq).Click += (s, e) => StartActivity(typeof(MedicalRequestActivity));
            FindViewById<TextView>(Resource.Id.tvwTrnReq).Click += (s, e) => StartActivity(typeof(TrainingRequestActivity));
            FindViewById<TextView>(Resource.Id.tvwAbsenteeism).Click += (s, e) => StartActivity(typeof(AbsenteeismActivity));
            FindViewById<TextView>(Resource.Id.tvwUserProfile).Click += (s, e) => StartActivity(typeof(UserProfileActivity));
        }
        
        //generate parent items
        private List<string> GenerateParents()
        {
            List<string> parentItems = new List<string>();

            parentItems.Add(" Leave");
            //parentItems.Add("Medical");
            //parentItems.Add("Absent");
            //parentItems.Add("Training Approval");

            return parentItems;
        }

        //generate children items
        private Dictionary<string, List<string>> GenerateChildren(List<string> parentItems)
        {
            Dictionary<string, List<string>> childrenItems = new Dictionary<string, List<string>>();

            List<string> leaveList = new List<string> { "Leave Request", "Leave Extension", "Leave Recall" };
            //List<string> medicalList = new List<string> { "Request", "Approval" };
            //List<string> absentList = new List<string> { "Absenteeism", "User Profile" };
            //List<string> trainingList = new List<string> { "Request", "Approval" };

            childrenItems[parentItems[0]] = leaveList;
            //childrenItems[parentItems[1]] = medicalList;
            //childrenItems[parentItems[2]] = absentList;
            //childrenItems[parentItems[3]] = trainingList;

            return childrenItems;
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            FinishAffinity();
        }
    }

    public class ExpandableListAdapter : BaseExpandableListAdapter
    {
        protected Activity Context { get; set; }
        protected List<string> ParentGroup { get; set; }
        protected Dictionary<string, List<string>> ChildGroup { get; set; }

        public ExpandableListAdapter(Activity context, List<string> parentGroup, Dictionary<string, List<string>> childGroup)
        {
            Context = context;
            ParentGroup = parentGroup;
            ChildGroup = childGroup;
        }


        //for child item view
        public override Java.Lang.Object GetChild(int parentPosition, int childPosition)
        {
            return ChildGroup[ParentGroup[parentPosition]][childPosition];
        }

        public override long GetChildId(int groupPosition, int childPosition)
        {
            return childPosition;
        }

        public override View GetChildView(int parentPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
        {
            string childText = (string)GetChild(parentPosition, childPosition);

            convertView = convertView ?? Context.LayoutInflater.Inflate(Resource.Layout.ChildItems, null);
            TextView tvwChild = (TextView)convertView.FindViewById(Resource.Id.tvwChildHome);
            tvwChild.Text = childText;

            return convertView;
        }

        public override int GetChildrenCount(int parentPosition)
        {
            return ChildGroup[ParentGroup[parentPosition]].Count;
        }


        //for parent item view
        public override Java.Lang.Object GetGroup(int groupPosition)
        {
            return ParentGroup[groupPosition];
        }

        public override int GroupCount
        {
            get { return ParentGroup.Count; }
        }

        public override long GetGroupId(int groupPosition)
        {
            return groupPosition;
        }

        public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
        {
            string headerTitle = (string)GetGroup(groupPosition);

            convertView = convertView ?? Context.LayoutInflater.Inflate(Resource.Layout.ParentItem, null);
            TextView tvwParent = (TextView)convertView.FindViewById(Resource.Id.tvwParentHome);
            tvwParent.Text = headerTitle;

            return convertView;
        }

        public override bool HasStableIds
        {
            get { return false; }
        }

        public override bool IsChildSelectable(int groupPosition, int childPosition)
        {
            return true;
        }
    }
}
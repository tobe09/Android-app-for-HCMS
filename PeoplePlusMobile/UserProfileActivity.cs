using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Json;
using Newtonsoft.Json;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.IO;
using Android.Graphics;
using Android.Preferences;
using Android.Provider;
using System.Net.Http;

namespace PeoplePlusMobile
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class UserProfileActivity : BaseActivity
    {
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            try
            {
                SetContentView(Resource.Layout.UserProfile);
                
                //get data from API
                AppPreferences appPref = new AppPreferences();
                string uri = Values.ApiRootAddress + "UserProfile/GetBasicDetails?compId=" + appPref.GetValue(User.CompId) + "&empNo=" + appPref.GetValue(User.EmployeeNo);

                Toast.MakeText(this, Values.LoadingMsg, ToastLength.Short).Show();

                dynamic json = await new DataApi().GetAsync(uri);

                if (IsJsonObject(json))
                {
                    JsonValue basicDetailsData = json["BasicDetails"];
                    JsonValue qualificationsData = json["Qualifications"];
                    JsonValue profCredentialsData = json["ProfessionalCredentials"];
                    JsonValue employeeTypeData = json["EmployeeType"];
                    JsonValue reportToData = json["ReportTo"];
                    ExpandableListView expListView = FindViewById<ExpandableListView>(Resource.Id.expdLstVw1);

                    //Bind list
                    List<string> parentList = GenerateParents();
                    Dictionary<string, List<string>> childDictionaryList = GenerateChildItems(parentList, basicDetailsData, qualificationsData, profCredentialsData, employeeTypeData, reportToData);
                    MyExpandableListAdapter listAdapter = new MyExpandableListAdapter(this, parentList, childDictionaryList);
                    expListView.SetAdapter(listAdapter);
                    expListView.ExpandGroup(0); expListView.ExpandGroup(1); expListView.ExpandGroup(2); expListView.ExpandGroup(3); expListView.ExpandGroup(4);
                }
                else
                {
                    Toast.MakeText(this, (string)json, ToastLength.Long).Show();
                }
            }
            catch (Exception ex)
            {
                ex.Log();
                Toast.MakeText(this, Values.ErrorMsg, ToastLength.Long).Show();
            }
        }

        //generate parent items
        private List<string> GenerateParents()
        {
            List<string> parentItems = new List<string>();

            parentItems.Add("Bio-data");
            parentItems.Add("Contact Details");
            parentItems.Add("Basic HR Details");
            parentItems.Add("Qualifications");
            parentItems.Add("Professional Credentials");

            return parentItems;
        }

        //generate children items
        public Dictionary<string, List<string>> GenerateChildItems(List<string> parentItems, JsonValue basicDetailsData, JsonValue qualificationsData, JsonValue profCredentialsData, JsonValue employeeTypeData, JsonValue reportToData)
        {
            Dictionary<string, List<string>> childrenItems = new Dictionary<string, List<string>>();

            //GenerateUserBioInfoChildItems
            List<string> userBiodata = new List<string>();

            if (basicDetailsData.Count != 0)
            {
                var fullname = basicDetailsData[0]["HEMP_EMPLYE_NAME"] + "";
                var name = fullname.Split(' ');

                userBiodata.Add("FirstName:      " + name[0]); //spacing needed for alignment
                userBiodata.Add("LastName:       " + name[1]);
                userBiodata.Add("DOB:                 " + basicDetailsData[0]["HEMP_BRTH_DATE"]);

                if (basicDetailsData[0]["HEMP_SEX"] == "M") { userBiodata.Add("Sex:                   MALE"); }
                else if (basicDetailsData[0]["HEMP_SEX"] == "F") { userBiodata.Add("Sex:                   FEMALE"); }

                userBiodata.Add("Place Of Birth: " + basicDetailsData[0]["HEMP_BRTH_PLCE"]);

                if (basicDetailsData[0]["HEMP_MRTL_STS"] == "U") { userBiodata.Add("Marital Status: SINGLE"); }
                else if (basicDetailsData[0]["HEMP_MRTL_STS"] == "M") { userBiodata.Add("Marital Status: MARRIED"); }
                else if (basicDetailsData[0]["HEMP_MRTL_STS"] == "D") { userBiodata.Add("Marital Status: DIVORCED"); }
                else if (basicDetailsData[0]["HEMP_MRTL_STS"] == "S") { userBiodata.Add("Marital Status: SEPARATED"); }
                else if (basicDetailsData[0]["HEMP_MRTL_STS"] == "W") { userBiodata.Add("Marital Status: WIDOWED"); }
            }


            //GenerateUserContactChildItems
            List<string> userContact = new List<string>();
            if (basicDetailsData.Count != 0)
            {
                userContact.Add("Phone No:          " + basicDetailsData[0]["HEMP_MBLE"]);
                userContact.Add("Email:                 " + basicDetailsData[0]["HEMP_EMAIL"]);
                userContact.Add("Address:             " + basicDetailsData[0]["HEMP_PRMNT_ADRS1"]);
                userContact.Add("Next Of Kin:       " + basicDetailsData[0]["HEMP_NAME"]);
                userContact.Add("NOK Phone No: " + basicDetailsData[0]["HEMP_EMRGNCY_PHNE_NMBR"]);
                userContact.Add("NOK Address:    " + basicDetailsData[0]["HEMP_NXTKIN_ADDR"]);
            }

            //GenerateUserHRChildItems
            List<string> userHR = new List<string>();
            if (employeeTypeData.Count != 0)
            {
                if (reportToData.Count != 0)
                {
                    userHR.Add("Staff Type:           " + employeeTypeData[0]["HETY_DSCRPTN"]);
                }
                else
                {
                    userHR.Add("Staff Type: ");
                }
                //userHR.Add("Staff Type:          " + employeeTypeData[0]["HETY_DSCRPTN"]);
                userHR.Add("User ID:                " + new AppPreferences().GetValue(User.EmployeeNo));
                userHR.Add("Grade:                   " + basicDetailsData[0]["HGRD_DSCRPTN"]);
                userHR.Add("Date Employed:   " + basicDetailsData[0]["HEMP_JNG_DATE"]);
                userHR.Add("Confirm. Status: " + basicDetailsData[0]["HEMP_EMPLYMNT_STATUS"]);
                userHR.Add("Date Confirmed:  " + basicDetailsData[0]["HEMP_CNFRMTN_DATE"]);
                userHR.Add("Department:        " + basicDetailsData[0]["HDPR_DSCRPTN"]);

                if (reportToData.Count != 0)
                {
                    userHR.Add("Line Manager:     " + reportToData[0]["HEMP_EMPLYE_NAME"]);
                }
                else
                {
                    userHR.Add("Line Manager:     ");
                }
                //userHR.Add("Line Manager: " + reportToData[0]["HEMP_EMPLYE_NAME"]);
                userHR.Add("Username:           " + new AppPreferences().GetValue(User.UserId));
            }

            //GenerateUserQualificationChildItems
            List<string> userQualification = new List<string>();
            if (qualificationsData.Count != 0)
            {
                for (int i = 0; i < qualificationsData.Count; i++)
                {
                    string values = "Serial Number:     " + Convert.ToInt32(i + 1) + "\r\n";      //qualificationsData[i]["HEQL_SRL_NMBR"]
                    values += "Institution Name: " + qualificationsData[i]["HEQL_UNVRSTY"] + "\r\n";
                    values += "Course:                   " + qualificationsData[i]["HDSC_DSCRPTN"] + "\r\n";
                    values += "Grade:                     " + qualificationsData[i]["HEQL_DEGREE"] + "\r\n";
                    values += "Qual. Obtained:     " + qualificationsData[i]["HEQL_HQLF_QLFCTN_CODE"];
                    userQualification.Add(values);
                }
            }
            else
            {
                userQualification.Add("No Qualification has been Uploaded");
            }


            //GenerateUserProfessionalCredentialChildItems
            List<string> userProfCredentials = new List<string>();
            if (profCredentialsData.Count != 0)
            {
                for (int i = 0; i < profCredentialsData.Count; i++)
                {
                    string values = "Serial Number: 1" + Convert.ToInt32(i + 1) + "\r\n";
                    values += "Code:                  " + profCredentialsData[i]["HEPM_HPMB_PRFSNL_MBRSHP"] + "\r\n";
                    values += "Name:                 " + profCredentialsData[i]["HPMB_DSCRPTN"] + "\r\n";
                    values += "From Date:        " + profCredentialsData[i]["HEPM_DATE_FROM"] + "\r\n";
                    values += "To Date:             " + profCredentialsData[i]["HEPM_DATE_TO"];
                    userProfCredentials.Add(values);
                };
            }
            else
            {
                userProfCredentials.Add("No Credential has been Uploaded");
            }

            childrenItems[parentItems[0]] = userBiodata;
            childrenItems[parentItems[1]] = userContact;
            childrenItems[parentItems[2]] = userHR;
            childrenItems[parentItems[3]] = userQualification;
            childrenItems[parentItems[4]] = userProfCredentials;

            return childrenItems;

        }
                
        public Bitmap DecodeSampledBitmapFromResource(string path, int reqWidth, int reqHeight)
        {
            // First decode with inJustDecodeBounds=true to check dimensions
            var options = new BitmapFactory.Options
            {
                InJustDecodeBounds = true,
                InPreferredConfig = Bitmap.Config.Argb8888
            };
            BitmapFactory.DecodeFile(path, options);

            // Calculate inSampleSize
            options.InSampleSize = CalculateInSampleSize(options, reqWidth, reqHeight);

            // Decode bitmap with inSampleSize set
            options.InJustDecodeBounds = false;
            return BitmapFactory.DecodeFile(path, options);
        }
        
        public int CalculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            // Raw height and width of image
            int height = options.OutHeight;
            int width = options.OutWidth;
            int inSampleSize = 1;

            if (height > reqHeight || width > reqWidth)
            {

                int halfHeight = height / 2;
                int halfWidth = width / 2;

                // Calculate the largest inSampleSize value that is a power of 2
                // and keeps both
                // height and width larger than the requested height and width.
                while ((halfHeight / inSampleSize) > reqHeight
                        && (halfWidth / inSampleSize) > reqWidth)
                {
                    inSampleSize *= 2;
                }
            }

            return inSampleSize;
        }

        public override void OnBackPressed()
        {
            MyOnBackPressed();
        }
    }


    public class MyExpandableListAdapter : BaseExpandableListAdapter
    {
        protected Activity Context { get; set; }
        protected List<string> ParentGroup { get; set; }
        protected Dictionary<string, List<string>> ChildGroup { get; set; }
        
        public MyExpandableListAdapter(Activity context, List<string> parentGroup, Dictionary<string, List<string>> childGroup1)
        {
            Context = context;
            ParentGroup = parentGroup;
            ChildGroup = childGroup1;
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


            convertView = convertView ?? Context.LayoutInflater.Inflate(Resource.Layout.ChildItemUserProfile, null);
            TextView tvwChild = convertView.FindViewById<TextView>(Resource.Id.tvwChildUserProfile);
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

            convertView = convertView ?? Context.LayoutInflater.Inflate(Resource.Layout.ParentItemUserProfile, null);
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
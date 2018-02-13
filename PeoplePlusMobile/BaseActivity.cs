using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.Content.PM;
using Android;
using System.Net.Http;
using Firebase.Iid;

namespace PeoplePlusMobile
{
    [Activity(Label = "PeoplePlus", Icon = "@drawable/neptunelogo", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class BaseActivity : Activity
    {
        protected static Bitmap bmp;        //in memory storage for user's profile picture

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                base.SetContentView(Resource.Layout.BaseLayout);

                TextView tvw = FindViewById<TextView>(Resource.Id.tvwWelcome);
                tvw.Text = "Welcome " + new AppPreferences().GetValue(User.Name);

                ImageView ProfileImage = FindViewById<ImageView>(Resource.Id.profileImage);
                if (bmp == null)
                {
                    byte[] picByte = await GenerateProfilePic();
                    bmp = BitmapFactory.DecodeByteArray(picByte, 0, picByte.Length);
                    ProfileImage.SetImageBitmap(bmp);           //if the asynchronous call completes late
                }
                ProfileImage.SetImageBitmap(bmp);

                ProfileImage.Click += (sender, e) =>
                {
                    FragmentTransaction ft = FragmentManager.BeginTransaction();
                    MyDialogFragment newFragment = new MyDialogFragment(this);

                    // Pass it with the Bundle class
                    Bundle bundle = new Bundle();
                    MemoryStream stream = new MemoryStream();
                    bmp?.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                    byte[] picByte = stream.ToArray();
                    bundle.PutByteArray("imgId", picByte);

                    newFragment.Arguments = bundle;

                    //Show the Fragment
                    newFragment.Show(ft, "dialog");

                    newFragment.imageChanged += (s, bmpEv) =>
                    {
                        bmp = bmpEv;
                        ProfileImage.SetImageBitmap(bmp);
                    };
                };
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        //set content of child activities
        public override void SetContentView(int id)
        {
            LayoutInflater inflater = Application.Context.GetSystemService(LayoutInflaterService) as LayoutInflater;
            var linBase = FindViewById<LinearLayout>(Resource.Id.linBase);
            View view = inflater.Inflate(id, linBase);
        }

        //generate profile pic
        public async virtual Task<byte[]> GenerateProfilePic()
        {
            byte[] picByte;

            string url = Values.ApiRootAddress + "UserProfile/GetUserImage?Id=" + new AppPreferences().GetValue(User.EmployeeNo);

            try
            {
                using (Stream imageStream = await new DataApi().GetImageAsync(url))
                {
                    byte[] buffer = new byte[1024 * 1024];
                    int count = imageStream.Read(buffer, 0, buffer.Length);
                    picByte = new byte[count];
                    for (int i = 0; i < count; i++)
                    {
                        picByte[i] = buffer[i];
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Log();
                //use default profile pic
                using (MemoryStream ms = new MemoryStream())
                {
                    Bitmap DefaultProfPic = BitmapFactory.DecodeResource(Resources, Resource.Drawable.defaultprofile);
                    DefaultProfPic.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
                    picByte = ms.ToArray();
                }
            }

            return picByte;
        }

        //menu list
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Layout.AppMenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        //action to take for each option selected
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.home:
                    StartActivity(typeof(HomeActivity));
                    break;

                case Resource.Id.leaveRequest:
                    StartActivity(typeof(LeaveRequestActivity));
                    break;

                case Resource.Id.leaveExtension:
                    StartActivity(typeof(LeaveExtensionActivity));
                    break;

                case Resource.Id.leaveRecall:
                    StartActivity(typeof(LeaveRecallActivity));
                    break;

                case Resource.Id.medicalRequest:
                    StartActivity(typeof(MedicalRequestActivity));
                    break;

                case Resource.Id.trainingRequest:
                    StartActivity(typeof(TrainingRequestActivity));
                    break;

                case Resource.Id.absenteeism:
                    StartActivity(typeof(AbsenteeismActivity));
                    break;

                case Resource.Id.wkListViewer:
                    StartActivity(typeof(WorkListActivity));
                    break;

                case Resource.Id.userProfile:
                    StartActivity(typeof(UserProfileActivity));
                    break;

                case Resource.Id.logout:
                    Toast.MakeText(this, Values.WaitingMsg, ToastLength.Short).Show();
                    PreLogOutActions().ContinueWith((response) =>
                    {
                        RunOnUiThread(() => {
                            object[] result = response.Result;
                            if ((bool)result[0])
                                StartActivity(typeof(MainActivity));
                            else
                                Toast.MakeText(this, (string)result[1], ToastLength.Long).Show();
                        });
                    });
                    break;

                default:
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        //actions to be taken before logout
        private async Task<object[]> PreLogOutActions()
        {
            object[] status = new object[2];

            string restUrl = Values.ApiRootAddress + "Notification/PostPreLogout";
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "CompId", new AppPreferences().GetValue(User.CompId) },
                        { "EmpNo", new AppPreferences().GetValue(User.EmployeeNo) },
                        { "UserId", new AppPreferences().GetValue(User.UserId) },
                        { "DeviceId", FirebaseInstanceId.Instance.Token }
                    });

            dynamic response = await new DataApi().PostAsync(restUrl, content);

            if (IsJsonObject(response))
            {
                AppPreferences pref = new AppPreferences();
                pref.RemoveValue(User.UserId);
                pref.RemoveValue(User.Password);
                bmp = null;
                status[0] = true;
                status[1] = "";
            }
            else
            {
                status[0] = false;
                status[1] = (string)response;
            }

            return status;
        }

        //go back to home page OnBackPressed
        protected virtual void MyOnBackPressed()
        {
            base.OnBackPressed();
            StartActivity(typeof(HomeActivity));
        }

        //generate list for a spinner
        protected virtual List<string> GetAttributeList(dynamic dataTableDict, string attribute, string initial = "Please Select...", bool setInitial = true)
        {
            List<string> values = new List<string>();

            if (setInitial) values.Add(initial);

            for (int i = 0; i < dataTableDict.Count; i++)
            {
                values.Add(dataTableDict[i][attribute]);
            }

            return values;
        }

        //to test validity of DataApi response as JSON object
        protected virtual bool IsJsonObject(dynamic response)
        {
            return DataApi.IsJsonObject(response);
        }

        //converts a date string to datetime or returns null
        public virtual DateTime? ConvertStringToDate(string dateStr)
        {
            return new CommonMethodsClass().ConvertStringToDate(dateStr);
        }

        //converts a datetime to date string
        public virtual string ConvertStringToDate(DateTime date)
        {
            return new CommonMethodsClass().ConvertDateToString(date);
        }

        //check if the application has been granted a permission
        public bool HasAccess(string permission, PermissionRequestCode reqCode = PermissionRequestCode.Default, string rationale = "Permision is necessary for optimal functionality", ToastLength toastLength = ToastLength.Short)
        {
            if ((int)Build.VERSION.SdkInt >= 23)
            {
                if (CheckSelfPermission(permission) != Permission.Granted)
                {
                    if (ShouldShowRequestPermissionRationale(permission))
                        Toast.MakeText(this, rationale, toastLength).Show();

                    RequestPermissions(new string[1] { permission }, (int)reqCode);
                    return false;
                }
            }

            return true;
        }

        //handle response of permission request
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            var messages = GetPermissionMessages((PermissionRequestCode)requestCode);

            if (grantResults[0] == Permission.Granted)
                Toast.MakeText(this, messages["granted"], ToastLength.Short).Show();
            else
            {
                if (!ShouldShowRequestPermissionRationale(permissions[0]))      //handle "Never ask again" option
                    Toast.MakeText(this, messages["neverAsk"], ToastLength.Short).Show();
                else
                    Toast.MakeText(this, messages["denied"], ToastLength.Short).Show();
            }
        }

        //generate permission messages
        public Dictionary<string, string> GetPermissionMessages(PermissionRequestCode reqCode = PermissionRequestCode.Default)
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();

            string granted = "Permission Granted";
            string denied = "Permission Denied";
            string neverAsk = "Permission can only be granted from device application settings";

            switch (reqCode)
            {
                case PermissionRequestCode.Storage:
                    neverAsk = "Storage permission (for image orientation) can only be granted from device application settings";
                    break;
            }

            if (reqCode != PermissionRequestCode.Default)
            {
                granted = reqCode + " " + granted;
                denied = reqCode + " " + denied;
            }

            messages.Add("granted", granted);
            messages.Add("denied", denied);
            messages.Add("neverAsk", neverAsk);

            return messages;
        }
    }

    public enum PermissionRequestCode
    {
        Default,
        Storage
    }


    //dialog fragment class for profile picture
    public class MyDialogFragment : DialogFragment
    {
        public event EventHandler<Bitmap> imageChanged;
        ImageView imgView;
        public Bitmap croppedBitmap;
        private static int maxBitmapSize = 2 * (int)Math.Pow(10, 7);

        private BaseActivity activity;

        public MyDialogFragment(BaseActivity activity)
        {
            this.activity = activity;
        }

        enum MediaType { Gallery, Camera }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(DialogFragmentStyle.NoTitle, 0);   //, Android.Resource.Style.ThemeDeviceDefaultLight);

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            View v = inflater.Inflate(Resource.Layout.ProfilePicDialog, container, false);

            imgView = v.FindViewById<ImageView>(Resource.Id.imageView1);

            //Get image from arguments and set it to the ImageView
            byte[] imgPassed = Arguments.GetByteArray("imgId");
            Bitmap bmp = BitmapFactory.DecodeByteArray(imgPassed, 0, imgPassed.Length);
            imgView.SetImageBitmap(bmp);

            ImageButton FABGallery = v.FindViewById<ImageButton>(Resource.Id.fab1);

            FABGallery.Click += (s, e) =>
            {
                var imageIntent = new Intent(Intent.ActionPick, MediaStore.Images.Media.ExternalContentUri);
                StartActivityForResult(Intent.CreateChooser(imageIntent, "Select photo"), (int)MediaType.Gallery);
            };

            ImageButton FABCamera = v.FindViewById<ImageButton>(Resource.Id.fab2);

            FABCamera.Click += (s, e) =>
            {
                Intent intent = new Intent(MediaStore.ActionImageCapture);
                StartActivityForResult(intent, (int)MediaType.Camera);
            };


            ImageButton FABUpload = v.FindViewById<ImageButton>(Resource.Id.fab3);

            FABUpload.Click += async (s, e) =>
            {
                if (croppedBitmap == null)
                {
                    Toast.MakeText(Activity, "Image has not been changed", ToastLength.Short).Show();
                    return;
                }

                //convert bitmap to stream
                using (MemoryStream stream = new MemoryStream())
                {
                    croppedBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                    byte[] imageByte = stream.ToArray();

                    string uri = Values.ApiRootAddress + "UserProfile/PutImage?empNo=" + new AppPreferences().GetValue(User.EmployeeNo) +
                    "&empName=" + new AppPreferences().GetValue(User.UserId) + "&compId=" + new AppPreferences().GetValue(User.CompId);
                    try
                    {
                        FABUpload.Enabled = false;
                        dynamic json = await new DataApi().PutAsync(uri, new System.Net.Http.ByteArrayContent(imageByte));

                        bool success = DataApi.IsJsonObject(json);
                        if (success)
                        {
                            if (json["ErrorStatus"] == 0)
                            {
                                imageChanged?.Invoke(this, croppedBitmap);
                                Toast.MakeText(Activity, "Uploaded successfully", ToastLength.Short).Show();
                            }
                            else
                            {
                                Toast.MakeText(Activity, "Upload failed", ToastLength.Short).Show();
                            }
                        }
                        else
                            Toast.MakeText(Activity, (string)json, ToastLength.Short).Show();
                    }
                    catch (Exception ex)
                    {
                        ex.Log();
                        Toast.MakeText(Activity, "An error occured while uploading picture", ToastLength.Short).Show();
                    }
                    FABUpload.Enabled = true;
                    Dismiss();
                }
            };


            return v;
        }

        public override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                try
                {
                    Bitmap bitmap;
                    if (requestCode == (int)MediaType.Gallery)          //if gallery button was clicked
                        bitmap = MediaStore.Images.Media.GetBitmap(Activity.ContentResolver, data.Data);
                    else                                               //if camera button was clicked
                        bitmap = (Bitmap)data.Extras.Get("data");

                    if (bitmap.ByteCount <= maxBitmapSize)
                        MyCropImage(bitmap, data.Data);
                    else
                        Toast.MakeText(Activity, "Image size is too large", ToastLength.Short).Show();
                }
                catch (Exception ex)
                {
                    ex.Log();
                    Toast.MakeText(Activity, "An error occured while loading the image", ToastLength.Short).Show();
                }
            }
        }

        public void MyCropImage(Bitmap bitmap, Android.Net.Uri uri)
        {
            string path = "";
            int rotation = 0;
            try
            {
                if (activity.HasAccess(Manifest.Permission.ReadExternalStorage, PermissionRequestCode.Storage, "Grant accesss to ensure correct image orientation"))
                    path = GetPathToImage(uri);

                Android.Media.ExifInterface exif = new Android.Media.ExifInterface(path);
                Android.Media.Orientation orientation = (Android.Media.Orientation)exif.GetAttributeInt(Android.Media.ExifInterface.TagOrientation, 0);
                switch (orientation)
                {
                    case Android.Media.Orientation.Rotate90: rotation = 90; break;
                    case Android.Media.Orientation.Rotate180: rotation = 180; break;
                    case Android.Media.Orientation.Rotate270: rotation = 270; break;
                    default: rotation = 0; break;
                }
            }
            catch (Exception ex) { ex.Log(); }

            using (bitmap)
            using (Matrix matrix = new Matrix())
            {
                matrix.PreRotate(rotation);

                int width1 = bitmap.Width;
                int height1 = bitmap.Height;
                int diffX = 0, diffY = 0;
                if (width1 > height1)
                {
                    diffX = (width1 - height1) / 2;
                    width1 = height1;
                }
                else
                {
                    diffY = (height1 - width1) / 2;
                    height1 = width1;
                }

                croppedBitmap = Bitmap.CreateBitmap(bitmap, diffX, diffY, width1, height1, matrix, true);
                croppedBitmap = Bitmap.CreateScaledBitmap(croppedBitmap, 250, 250, true);
                imgView.SetImageBitmap(croppedBitmap);
            }
        }

        protected string GetPathToImage(Android.Net.Uri uri)
        {
            string path = "";

            Android.Database.ICursor cursor = Activity.ContentResolver.Query(uri, null, null, null, null);
            cursor.MoveToFirst();
            string document_id = cursor.GetString(0);
            document_id = document_id.Substring(document_id.LastIndexOf(":") + 1);

            cursor = Activity.ContentResolver.Query(MediaStore.Images.Media.ExternalContentUri,
                null, MediaStore.Images.Media.InterfaceConsts.Id + " = ? ", new string[] { document_id }, null);
            cursor.MoveToFirst();
            path = cursor.GetString(cursor.GetColumnIndex(MediaStore.Images.Media.InterfaceConsts.Data));
            cursor.Dispose();

            return path;
        }
    }
}
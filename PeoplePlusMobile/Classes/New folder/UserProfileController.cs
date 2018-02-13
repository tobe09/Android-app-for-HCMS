using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.VisualBasic;

namespace PeoplePlusWebApi.Controllers
{
    public class UserProfileController : ApiController
    {
        private DataProvider.UserProfile _usrProfileObj = new DataProvider.UserProfile();
        public DataProvider.UserProfile UsrProfileObj
        {
            get { return _usrProfileObj; }
        }

        
        public HttpResponseMessage GetBasicDetails(string compId, int empNo)
        {
            try
            {
                DataTable dt = UsrProfileObj.GetSpecificRecords(compId,empNo);
                DataTable dt2 = UsrProfileObj.GetUserQualification(compId, empNo);
                DataTable dt3 = UsrProfileObj.GetUserProfCredential(compId, empNo);

                if (dt == null || dt2 == null || dt3 == null)
                    return Request.CreateResponse(HttpStatusCode.NoContent);

                var empTyeCode = dt.Rows[0]["HEMP_HETY_EMPLYE_TYPE_COD"] == null ? "" : dt.Rows[0]["HEMP_HETY_EMPLYE_TYPE_COD"].ToString();
                var supviorNo = dt.Rows[0]["HEMP_HEMP_EMPLYE_NMBR"] == null ? 0 : Convert.ToInt32(dt.Rows[0]["HEMP_HEMP_EMPLYE_NMBR"]);
                DataTable dt4 = UsrProfileObj.GetEmployeeType(empTyeCode);
                DataTable dt5 = UsrProfileObj.GetEmployeeReportedTo(supviorNo);

                if (dt4 == null || dt5 == null)
                    return Request.CreateResponse(HttpStatusCode.NoContent);

                var output = new { BasicDetails = dt, Qualifications = dt2, ProfessionalCredentials = dt3, EmployeeType = dt4, ReportTo = dt5 };
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }
            catch (Exception ex)
            {
                ex.Log();
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
        }

        
        public HttpResponseMessage GetUserImage(int Id)
        {
            try
            {
                HttpResponseMessage response = new HttpResponseMessage();

                DataTable dt = UsrProfileObj.GetUserImage(Id);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent);
                }

                var src = dt.Rows[0]["IMAGE_DATA"] == null ? "~\\Resource\\UploadedImages\\default.jpg" : dt.Rows[0]["IMAGE_DATA"].ToString();

                //src = HttpContext.Current.Server.MapPath(src);
                src = "E:\\Wisdom Essien\\PeoplePlusFolder\\2017 Solutions\\October 2017\\PeoplePlus\\PeoplePlus" + src.Remove(0, 1);

                Byte[] imageByte;
                try
                {
                    imageByte = File.ReadAllBytes(src);
                }
                catch (Exception)
                {
                    //src = HttpContext.Current.Server.MapPath("~\\Resource\\UploadedImages\\default.jpg");
                    src = "E:\\Wisdom Essien\\PeoplePlusFolder\\2017 Solutions\\October 2017\\PeoplePlus\\PeoplePlus\\Resource\\UploadedImages\\default.jpg";
                    imageByte = File.ReadAllBytes(src);
                }

                MemoryStream ms = new MemoryStream(imageByte);

                response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(ms);
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpg");

                return response;
            }
            catch (Exception ex)
            {
                ex.Log();
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
        }

        
        public HttpResponseMessage PutImage(int empNo)
        {
            try
            {
                //CREATE A SESSION ID FROM CURRENT DATE AND TIME CONCATENATED WITH EMP NO
                string sessionid = DateTime.Now.ToString();
                sessionid = sessionid.Replace("\\", "").Replace("/", "").Replace(" ", "").Replace(":", ""); //removes slash and the spaces
                sessionid += empNo;

                //GET PREVIOUS IMAGE LINK FROM THE DB
                DataTable dt = UsrProfileObj.GetUserImage(empNo);
                if (dt == null || dt.Rows.Count == 0)
                    return Request.CreateResponse(HttpStatusCode.OK, new { ErrorStatus = 1 });

                var src = dt.Rows[0]["IMAGE_DATA"] == null ? "~\\Resource\\UploadedImages\\default.jpg" : dt.Rows[0]["IMAGE_DATA"].ToString();


                //check whether read image link is for default image or not
                string filename;
                string[] output;
                if (src.ToLower().Contains("default"))
                    filename = sessionid + DateTime.Now.Millisecond.ToString() + ".jpg";
                else
                {
                    output = src.Split('/');
                    filename = output[output.GetUpperBound(output.Rank - 1)];
                }

                string path = "E:\\Wisdom Essien\\PeoplePlusFolder\\2017 Solutions\\October 2017\\PeoplePlus\\PeoplePlus\\Resource\\UploadedImages\\";
                //string path = HttpContext.Current.Server.MapPath("~/Resource/UploadedImages/");
                
                string fullpath = path + filename;

                //Request.Content.LoadIntoBufferAsync().Wait();

                Stream stream = Request.Content.ReadAsStreamAsync().Result;
				Image image = Image.FromStream(stream);
				image.Save(fullpath);

                object[] varSavePhoto = new object[5];
                varSavePhoto[0] = 0; //ID not necessary
                varSavePhoto[1] = "~/Resource/UploadedImages/" + filename;
                varSavePhoto[2] = ""; //FILE TYPE not necessary
                varSavePhoto[3] = empNo;
                varSavePhoto[4] = empNo;


                var Result = DataCreator.ExecuteProcedure("UPDATE_PASSPORT", varSavePhoto);


                if (Result == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ErrorStatus = 0 });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ErrorStatus = 1 });
                }
            }
            catch (Exception ex)
            {
                ex.Log();
                return Request.CreateResponse(HttpStatusCode.OK, new { ErrorStatus = 1 });
            }
            
        }
    }
}

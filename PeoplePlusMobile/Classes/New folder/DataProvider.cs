using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.IO;

namespace PeoplePlusWebApi
{
    public class DataProvider
    {
        public string RootAddress { get { return Values.RootAddress; } }
        public static string Query { get; set; }

        /// <summary>
        /// Returns a datatable which matches the prescribed query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static DataTable GetDataTable(string query)
        {
            DataTable dt = new DataTable();
            using (OracleConnection con = new OracleConnection(new DataProvider().GetConnectionString()))
            {
                using (OracleCommand cmd = new OracleCommand(query, con))
                {
                    con.Open();
                    OracleDataAdapter adp = new OracleDataAdapter(cmd);
                    adp.Fill(dt);
                    return dt;
                }
            }
        }

        // To generate the connection string
        internal string GetConnectionString()
        {
            TextReader ts = new StreamReader(RootAddress + "App_Data\\cosmos.ini");
            string allText = ts.ReadToEnd();
            allText = allText.Replace(" ", "");
            string[] allContent = allText.Split('\n','\r');

            string dataSource = DecryptText(allContent[0].Split('=')[1]);
            string userId = DecryptText(allContent[2].Split('=')[1]);
            string password = DecryptText(allContent[4].Split('=')[1]);

            return "Data Source=" + dataSource + ";User ID=" + userId + ";Password=" + password + ";Unicode=True;Persist Security Info=True;";
        }

        //To encrypt text
        public string EncryptText(string textClearText)
        {
            String clearValue = "";
            for (int i = 0; i < textClearText.Length; i++)
            {
                clearValue += (char)((int)(textClearText[i]) + 10);
            }
            return clearValue;
        }

        //to decrypt text
        public string DecryptText(string textCipherText)
        {
            String encryptedValue = "";
            for (int i = 0; i < textCipherText.Length; i++)
            {
                encryptedValue += (char)((int)(textCipherText[i]) - 10);
            }
            return encryptedValue;
        }

        /// <summary>
        /// Protect a database against input parameter performing an sql injection attack
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static bool sqlProtect(string param)
        {
            bool safe = true;
            if (param.Contains("'") || param.Contains("--")) safe = false;
            return safe;
        }

        //NOT USED, automatically done by asp.net web api
        //convert an sql table to dictionary array
        public Dictionary<string, object>[] ConvertToDictionaryArray(DataTable dt)
        {
            int count = dt.Rows.Count;
            Dictionary<string, object>[] values = new Dictionary<string, object>[count];        //an array of values

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Dictionary<string, object> value = new Dictionary<string, object>();
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    string columnName = dt.Columns[j].ColumnName.ToString();
                    object content = dt.Rows[i][j];
                    value.Add(columnName, content);                         //add column name and content to dictionary
                }
                values[i] = value;                                          //add value to value array
            }

            return values;
        }

        public class Login
        {
            public DataTable GetUserDetails(string username)
            {
                Query = "select a.comp_aid,a.HEMP_EMPLYE_NMBR,a.HEMP_NEW_EMPLYE_NMBR,HEMP_EMPLYE_NAME,a.hemp_brth_date, b.USER_PASSWORD, a.HEMP_TITLE, " +
                    "a.HEMP_FLCT_LCTN_CODE,a.HEMP_HDPR_DPRTMNT_CODE, a.HEMP_LV_GRP_CODE,a.HEMP_HDDT_HGRD_GRDE_CODE, a.hemp_pin_nmbr,a.hemp_nssf_nmbr," +
                    "a.hemp_acnt_nmbr, b.ROLE_ID,a.HEMP_EMAIL from h_emplye_mstr a,HRM_USER b  where a.comp_aid = b.comp_aid and " +
                    "a.HEMP_EMPLYE_NMBR = b.EMP_AID and  b.USER_AID ='" + username + "' and b.USER_STATUS='ACTIVE'";

                return GetDataTable(Query);
            }

            public DataTable GetComputerCode(string compId)
            {
                Query = "select COMP_NAME from GM_COMP_HD where COMP_AID='" + compId + "'";

                return GetDataTable(Query);
            }

            public DataTable GetAccountYear(string compId)
            {
                Query = "select acc_year,from_date,to_date from gm_year where comp_aid= '" + compId + "' and cur_year = 'Y'";

                return GetDataTable(Query);
            }

            public DataTable GetCompanyEmail(string compId)
            {
                Query = "select EMAIL_ADDR from gm_comp_hd where comp_aid= '" + compId + "'";

                return GetDataTable(Query);
            }

            public DataTable GetExtraData(int empNo)
            {
                Query = " SELECT a.flct_dscrptn,b.hdpr_dscrptn, c.hdsg_dscrptn, d.HGRD_DSCRPTN  FROM f_lctn_mstr a, h_emplye_mstr t, h_dprtmnt_mstr b, " +
                    "h_dsgntn_mstr c, h_grde_mstr d  WHERE T.HEMP_FLCT_LCTN_CODE=A.FLCT_LCTN_CODE  AND T.HEMP_HDPR_DPRTMNT_CODE=B.HDPR_DPRTMNT_CODE  " +
                    "AND T.HEMP_HDDT_HDSG_DSGNTN_COD=C.HDSG_DSGNTN_CODE  AND T.HEMP_HDDT_HGRD_GRDE_CODE=D.HGRD_GRDE_CODE   AND t.hemp_emplye_nmbr =" +
                    empNo + "";

                return GetDataTable(Query);
            }

            public DataTable GetLicenseDate(string compId)
            {
                Query = "select EXPIRY_DATE from gm_lcnce_info where COMP_AID='" + compId + "'";

                return GetDataTable(Query);
            }
        }

        public class MedicalRequest
        {
            public DataTable GetHospitals(string compId)
            {
                Query = "select HOSP_NAME \"NAME\", HOSP_ID \"CODE\" from H_HSPT_RGST WHERE COMP_AID = '" + compId + "'";

                return GetDataTable(Query);
            }

            public DataTable GetLastReqNo(string compId)
            {
                Query = "select nvl(max(substr(REQ_NO,3,6)),0) \"MAXVAL\" from H_MDCL_RSQT WHERE COMP_AID = '" + compId + "'";

                return GetDataTable(Query);
            }

            public DataTable GetMedicalReq2(string compId, string newReqNo, int empNo)
            {
                Query = "SELECT NVL(RQST_ID,1) \"VAL\" FROM H_MDCL_RSQT WHERE COMP_AID ='" + compId + "' AND REQ_NO = '" + newReqNo + "' AND REQ_EMP_ID =" + empNo;

                return GetDataTable(Query);
            }

            public DataTable GetMedicalRequestAppraisal(string compId, string requestId)
            {
                Query = "select b.appr_emp_aid , d.hemp_emplye_name approver, t.REQ_EMP_ID, c.hemp_emplye_name requester, d.hemp_email apprvr_email, " +
                       " c.hemp_email rqstr_email from H_MDCL_RSQT t, ss_approval_employees b, h_emplye_mstr c, h_emplye_mstr d" +
                       " where t.comp_aid = b.comp_aid and t.comp_aid = c.comp_aid and t.comp_aid = d.comp_aid and t.rqst_id = b.rqst_id" +
                       " and c.hemp_emplye_nmbr = t.REQ_EMP_ID and d.hemp_emplye_nmbr = b.appr_emp_aid" +
                       " and t.rqst_id = " + requestId + " and t.comp_aid ='" + compId + "'";

                return GetDataTable(Query);
            }

            public DataTable GetLeaveApprovalMail(string compId)
            {
                Query = "select t.mail_svr_name from  gm_comp_hd t where t.comp_aid='" + compId + "'";

                return GetDataTable(Query);
            }
        }

        public class TrainingRequest
        {
            public DataTable ListOfTrainings(string ComCode)
            {
                Query = "SELECT A.HTPR_TRNG_PRGRM_CODE TrainingCode, B.HTRM_TRNG_DSCRPTN Description,A.HTPR_DRTN Duration, " +
                " A.HTPR_VENUE Venue,DECODE(B.HTRM_TYPE, 'G','GENERAL','S', 'SPECIFIC') Type ,A.HTPR_SRL_NMBR TrnSno " +
                " FROM H_TRNG_PRGRM_MSTR A, H_TRNG_MSTR B WHERE A.COMP_AID='" + ComCode + "'  AND A.HTPR_TRNG_PRGRM_CODE= B.HTRM_TRNG_CODE";

                return GetDataTable(Query);
            }

            public DataTable SubordinateEmpBaseOnTraining(int userAID, string TrngCode, string ComCode)
            {
                Query = "SELECT  HEMP_NEW_EMPLYE_NMBR EmployeeNumber ,HEMP_EMPLYE_NAME Name, HEMP_EMPLYE_NMBR StaffID, HDPR_DSCRPTN Department, HDSG_DSCRPTN Designation, HGRD_DSCRPTN Grade " +
                " FROM H_EMPLYE_MSTR A, H_DPRTMNT_MSTR B, H_DSGNTN_MSTR C, H_GRDE_MSTR D WHERE A.COMP_AID='" + ComCode + "' " +
                " AND HEMP_HDDT_HGRD_GRDE_CODE IN (SELECT HTRD_GRD_CODE FROM H_TRNG_DTLS WHERE COMP_AID='" + ComCode + "' AND HTRD_TRNG_CODE='" + TrngCode + "') AND HEMP_RSGNTN_DATE IS NULL " +
                " AND HEMP_EMPLYE_NMBR NOT IN (SELECT HTNM_HEMP_EMPLYE_NMBR FROM H_TRNG_NMNTN WHERE COMP_AID='" + ComCode + "' AND HTNM_TRNG_PRGRM_CODE='" + TrngCode + "') " +
                " AND HEMP_HEMP_EMPLYE_NMBR='" + userAID + "' " +
                " AND HEMP_HDDT_HGRD_GRDE_CODE = D.HGRD_GRDE_CODE AND HEMP_HDDT_HDSG_DSGNTN_COD = C.HDSG_DSGNTN_CODE " +
                " AND HEMP_HDPR_DPRTMNT_CODE = B.HDPR_DPRTMNT_CODE ORDER BY HEMP_EMPLYE_NAME ";

                return GetDataTable(Query);
            }

            public DataTable TrainingReqId(string CompCode, string TrnCode, int EmpNo, int TranSN)
            {
                Query = " SELECT RQST_ID FROM H_TRNG_NMNTN WHERE  COMP_AID = '" + CompCode + "' And HTNM_HEMP_EMPLYE_NMBR = " + EmpNo +
                            " AND HTNM_TRNG_PRGRM_CODE ='" + TrnCode + "' AND    HTNM_TRNG_SRL_NMBR = " + TranSN;

                return GetDataTable(Query);
            }

            public int DeleteFromSSApprovalTable(string CompCode, object intGlobReqestID)
            {
                Query = "DELETE H_TRNG_NMNTN WHERE COMP_AID='" + CompCode + "' AND RQST_ID=" + intGlobReqestID;

                return DataCreator.ExecuteSQL(Query);
            }

            public DataTable SelectSSGrievGLBMail(string CompCode, object GLBRqstID)
            {
                Query = " select b.appr_emp_aid , d.hemp_emplye_name approver, t.htnm_hemp_emplye_nmbr, c.hemp_emplye_name requester, d.hemp_email apprvr_email, " +
                                " c.hemp_email rqstr_email from H_TRNG_NMNTN t, ss_approval_employees b, h_emplye_mstr c, h_emplye_mstr d " +
                                " where t.comp_aid = b.comp_aid and t.comp_aid = d.comp_aid and t.comp_aid = c.comp_aid and t.rqst_id = b.rqst_id " +
                        " and c.hemp_emplye_nmbr = t.HTNM_HEMP_EMPLYE_NMBR and d.hemp_emplye_nmbr = b.appr_emp_aid" +
                        " and t.rqst_id = " + GLBRqstID + " and t.comp_aid ='" + CompCode + "'";

                return GetDataTable(Query);
            }
        }

        public class MedicalApproval
        {
            public DataTable MedApprovalValidate(string compId, string empNo)
            {
                Query = "Select APPR_EMP_AID from SS_APPROVAL_EMPLOYEES where COMP_AID='" + compId + "' and  APPR_EMP_AID=" + empNo + " and  RQST_CODE= 'MDR'";

                return GetDataTable(Query);
            }

            public DataTable MedApprovalRequests(string compId, string empNo, string accYear)
            {
                Query = "select A.REQ_NO, A.REQ_EMP_ID, B.HEMP_EMPLYE_NAME, A.Req_Reason, a.req_hosp_id, c.hosp_name, TO_CHAR(A.REQ_DATE, 'DD-MON-YYYY') \"Date\", " +
                    "A.RQST_ID, A.RQST_CUR_STAGE, to_char(round(nvl(d.H_CUR_RMN_LMT,0),2),'fm999,999,999,999.00') as limitbal, " +
                    "to_char(round(nvl(e.bill,0),2),'fm999,999,999,999.00') used FROM h_mdcl_rsqt A, H_EMPLYE_MSTR B,h_hspt_rgst c,h_mdcl_rmn_lmt d, " +
                "(select u.comp_aid,u.emp_id,sum(u.bill_amount) bill from h_hspt_invc_mstr t,h_hspt_invc_dtls u where t.comp_aid = u.comp_aid " +
                "and t.hosp_id = u.hosp_id  and t.invoice_no = u.invoice_no and t.start_date >= (select s.from_date from gm_year s where s.comp_aid = '" + compId +
                "' and s.acc_year = '" + accYear + "' and s.cur_year = 'Y') and t.start_date <= (select s.to_date from gm_year s where s.comp_aid = '" + compId +
                "' and s.acc_year = '" + accYear + "' and s.cur_year = 'Y') and t.invoice_status = 'U' group by u.comp_aid,u.emp_id ) e " +
                " WHERE A.COMP_AID=B.COMP_AID and A.COMP_AID =C.COMP_AID AND B.COMP_AID=C.COMP_AID and A.COMP_AID = '" + compId +
                "' AND A.REQ_EMP_ID = B.HEMP_EMPLYE_NMBR and a.req_hosp_id=c.hosp_id AND A.RQST_ID IN (SELECT RQST_ID FROM SS_APPROVAL_EMPLOYEES WHERE COMP_AID='" + compId +
                "' AND RQST_CODE='MDR' AND APPR_EMP_AID=" + empNo + ") and a.comp_aid = d.comp_aid(+) and a.req_emp_id = d.H_EMPLYE_NMBR(+) and d.H_ACC_YEAR(+) = '" + accYear +
                "' and a.comp_aid = e.comp_aid(+) and a.req_emp_id = e.emp_id(+)";

                return GetDataTable(Query);
            }

            public DataTable MedApprovalStatus(string compId, string reqId, string approveeNo)
            {
                Query = "select REQ_STATUS,RQST_CUR_STAGE from h_mdcl_rsqt  WHERE COMP_AID ='" + compId + "' and RQST_ID =" + reqId +
                    " AND RQST_CUR_STAGE = (select SS_APPRM_STGS  FROM SS_APPR_MSTR_HD A,SS_APPR_MSTR_DT B WHERE A.COMP_AID = B.COMP_AID " +
                    " And A.COMP_AID = '" + compId + "'  AND A.SS_APPRM_CODE = B.SS_APPRM_CODE AND A.SS_RQSTM_CODE = B.SS_RQSTM_CODE" +
                    " And B.SS_RQSTM_CODE = 'MDR' AND B.SS_DESG_CODE = (SELECT HEMP_HDDT_HDSG_DSGNTN_COD  FROM H_EMPLYE_MSTR" +
                    " WHERE COMP_AID ='" + compId + "' AND HEMP_EMPLYE_NMBR =" + approveeNo + "))";

                return GetDataTable(Query);
            }

            public DataTable MedApprovalValidators(string compId, string reqId)
            {
                Query = "select b.appr_emp_aid , d.hemp_emplye_name approver, t.req_emp_id, c.hemp_emplye_name requester, d.hemp_email apprvr_email, " +
                       " c.hemp_email rqstr_email from H_MDCL_RSQT t, ss_approval_employees b, h_emplye_mstr c, h_emplye_mstr d" +
                       " where t.comp_aid = b.comp_aid and t.comp_aid = d.comp_aid and t.comp_aid = c.comp_aid and t.rqst_id = b.rqst_id" +
                       " and c.hemp_emplye_nmbr = t.REQ_EMP_ID and d.hemp_emplye_nmbr = b.appr_emp_aid" +
                       " and t.rqst_id = " + reqId + " and t.comp_aid ='" + compId + "'";

                return GetDataTable(Query);
            }
        }

        public class Approval
        {
            public DataTable GetWorkListViewer(string compId, int empNo)
            {
                Query = "  SELECT   t.alert_id \"Alert_Id\",  DECODE(t.alert_type,   'R',  'Approval',  'A', 'Anniversary', 'N', 'Notification') \"Type\", " +
                        "TO_CHAR(t.crtd_date, 'dd/Mm/yyyy') \"Date\",  t.alert_message \"Description\",  SUBSTR(t.alert_message, 1, 25) || '...' \"Description\", " +
                        "t.alert_status \"Flag\", t.mdfd_date \"Modify Date\",t.crtd_by \"Created By\", 'Q' \"Rs Status\", alert_code \"Alert Code\", " +
                        "ALERT_TIMES \"Times\", alert_recipient \"Recipient\", RQST_ID \"REQUEST\",DECODE (U.FORM_ID, 16, 'Medical', 12, 'Leave', 27, 'Training') \"Approval_Type\" "+
                        " FROM gm_alerts t, ss_alertid_map u WHERE t.comp_aid = '" +
                        compId + "' AND t.alert_status = 'F' AND(t.alert_recipient = " + empNo + " OR t.alert_recipient IS NULL) AND(t.alert_id, " + empNo + ") NOT IN" +
                        "(SELECT   A.alert_id, A.alert_recipient FROM   gm_alerts_seen A, GM_ALERTS B WHERE   A.alert_id = B.ALERT_ID  AND B.ALERT_CODE LIKE 'NOTE%')" +
                        " AND TO_DATE (NVL(t.ALERT_EXPIRY_DATE, SYSDATE), 'dd-mm-yyyy') >=  TO_DATE(SYSDATE, 'dd-mm-yyyy') AND(t.alert_code LIKE 'NOTE%' " +
                        "OR t.alert_code LIKE 'APR%') and T.ALERT_ID=U.ALERT_ID and t.alert_type='R' ORDER BY t.crtd_date";

                return GetDataTable(Query);
            }

            //MEDICAL
            public DataTable MedApprovalStatus(string compId, int reqId, int approveeNo)
            {
                Query = "select REQ_STATUS,RQST_CUR_STAGE from h_mdcl_rsqt  WHERE COMP_AID ='" + compId + "' and RQST_ID =" + reqId +
                    " AND RQST_CUR_STAGE = (select SS_APPRM_STGS  FROM SS_APPR_MSTR_HD A,SS_APPR_MSTR_DT B WHERE A.COMP_AID = B.COMP_AID " +
                    " And A.COMP_AID = '" + compId + "'  AND A.SS_APPRM_CODE = B.SS_APPRM_CODE AND A.SS_RQSTM_CODE = B.SS_RQSTM_CODE" +
                    " And B.SS_RQSTM_CODE = 'MDR' AND B.SS_DESG_CODE = (SELECT HEMP_HDDT_HDSG_DSGNTN_COD  FROM H_EMPLYE_MSTR" +
                    " WHERE COMP_AID ='" + compId + "' AND HEMP_EMPLYE_NMBR =" + approveeNo + "))";

                return GetDataTable(Query);
            }
            
            public DataTable MedApprovalValidators(string compId, int reqId)
            {
                Query = "select b.appr_emp_aid , d.hemp_emplye_name approver, t.req_emp_id, c.hemp_emplye_name requester, d.hemp_email apprvr_email, " +
                       " c.hemp_email rqstr_email from H_MDCL_RSQT t, ss_approval_employees b, h_emplye_mstr c, h_emplye_mstr d" +
                       " where t.comp_aid = b.comp_aid and t.comp_aid = d.comp_aid and t.comp_aid = c.comp_aid and t.rqst_id = b.rqst_id" +
                       " and c.hemp_emplye_nmbr = t.REQ_EMP_ID and d.hemp_emplye_nmbr = b.appr_emp_aid" +
                       " and t.rqst_id = " + reqId + " and t.comp_aid ='" + compId + "'";

                return GetDataTable(Query);
            }

            public DataTable MedApprovalValidate(string compId, string empNo)
            {
                Query = "Select APPR_EMP_AID from SS_APPROVAL_EMPLOYEES where COMP_AID='" + compId + "' and  APPR_EMP_AID=" + empNo + " and  RQST_CODE= 'MDR'";

                return GetDataTable(Query);
            }

            public DataTable MedApprovalRequest(string compId, string accYear, string reqId)
            {
                Query = "select A.REQ_NO, A.REQ_EMP_ID, B.HEMP_EMPLYE_NAME, A.Req_Reason, a.req_hosp_id, c.hosp_name, TO_CHAR(A.REQ_DATE, 'DD-MON-YYYY') \"Date\", " +
                    "A.RQST_ID, A.RQST_CUR_STAGE, to_char(round(nvl(d.H_CUR_RMN_LMT,0),2),'fm999,999,999,999.00') as limitbal, " +
                    "to_char(round(nvl(e.bill,0),2),'fm999,999,999,999.00') used FROM h_mdcl_rsqt A, H_EMPLYE_MSTR B,h_hspt_rgst c,h_mdcl_rmn_lmt d, " +
                "(select u.comp_aid,u.emp_id,sum(u.bill_amount) bill from h_hspt_invc_mstr t,h_hspt_invc_dtls u where t.comp_aid = u.comp_aid " +
                "and t.hosp_id = u.hosp_id  and t.invoice_no = u.invoice_no and t.start_date >= (select s.from_date from gm_year s where s.comp_aid = '" + compId +
                "' and s.acc_year = '" + accYear + "' and s.cur_year = 'Y') and t.start_date <= (select s.to_date from gm_year s where s.comp_aid = '" + compId +
                "' and s.acc_year = '" + accYear + "' and s.cur_year = 'Y') and t.invoice_status = 'U' group by u.comp_aid,u.emp_id ) e " +
                " WHERE A.COMP_AID=B.COMP_AID and A.COMP_AID =C.COMP_AID AND B.COMP_AID=C.COMP_AID and A.COMP_AID = '" + compId +
                "' AND A.REQ_EMP_ID = B.HEMP_EMPLYE_NMBR and a.req_hosp_id=c.hosp_id AND A.RQST_ID=" + reqId + " and a.comp_aid = d.comp_aid(+) and a.req_emp_id = d.H_EMPLYE_NMBR(+) and d.H_ACC_YEAR(+) = '" + accYear +
                "' and a.comp_aid = e.comp_aid(+) and a.req_emp_id = e.emp_id(+) and a.rqst_id=" + reqId;

                return GetDataTable(Query);
            }

            //LEAVE
            public DataTable GetLeaveCurrentStage(string compId, int reqId, int empNo)
            {
                Query = "select LV_STS, RQST_CUR_STAGE from H_EMP_LV_RQST  WHERE COMP_AID ='" + compId + "' and RQST_ID =" + reqId + " AND H_EMPLYE_NMBR =" + empNo;

                return GetDataTable(Query);
            }

            public DataTable GetLeaveApprovalStage1(string compId, int reqId)
            {
                Query = "select b.appr_emp_aid , d.hemp_emplye_name approver, t.H_EMPLYE_NMBR, c.hemp_emplye_name requester, d.hemp_email apprvr_email, " +
                                " c.hemp_email rqstr_email from H_EMP_LV_RQST t, ss_approval_employees b, h_emplye_mstr c, h_emplye_mstr d" +
                                " where t.comp_aid = b.comp_aid and t.comp_aid = d.comp_aid and t.comp_aid = c.comp_aid and t.RQST_ID = b.rqst_id" +
                                " and c.hemp_emplye_nmbr = t.H_EMPLYE_NMBR and d.hemp_emplye_nmbr = b.appr_emp_aid" +
                                " and t.rqst_id = " + reqId + " and t.comp_aid ='" + compId + "'";

                return GetDataTable(Query);
            }

            public DataTable LeaveApprovalSelfMe(string compId, int approveeNo)
            {
                Query = "select  d.hemp_email apprvr_email from  h_emplye_mstr d where d.comp_aid = '" + compId + "' and d.HEMP_EMPLYE_NMBR = " + approveeNo;

                return GetDataTable(Query);
            }

            public DataTable GetWorkDays(string compId)
            {
                Query = "SELECT HDSC_DAY_CODE \"WorkDays\" FROM H_DAYS_SYSTEM_CAL WHERE COMP_AID ='" + compId + "'";

                return GetDataTable(Query);
            }

            public DataTable GetHolidayDates()
            {
                Query = "SELECT HPHD_REC_HOL_DAY || ' ' || HPHD_REC_HOL_MONTH \"RecHols\" FROM H_REC_HOL_DAYS";

                return GetDataTable(Query);
            }

            public DataTable GetNonRecurringHols()
            {
                Query = "SELECT HPHD_NREC_HOL_DATE \"NonRecHols\" FROM H_NREC_HOL_DAYS ";

                return GetDataTable(Query);
            }

            public DataTable GetPayHolidayStatus(string compId)
            {
                Query= "select t.value \"Value\" from gm_options t where t.code='PAY_HOLIDAY_DAYS' and t.comp_aid = '" + compId + "'";

                return GetDataTable(Query);
            }

            public DataTable LvlApprovalValidate(string compId, string empNo)
            {
                Query = "Select APPR_EMP_AID from SS_APPROVAL_EMPLOYEES where COMP_AID='" + compId + "' and  APPR_EMP_AID=" + empNo + " and  RQST_CODE= 'LVR'";

                return GetDataTable(Query);
            }

            public DataTable LvlApprovalRequest(string compId, string reqId)
            {
                Query = "select A.H_EMPLYE_NMBR \"EmpNo\", B.HEMP_EMPLYE_NAME \"EmpName\",A.H_LV_CODE \"LvCode\",C.H_LEAVE_DSCRPTN \"LvDesc\"," +
                    "TO_CHAR(A.P_DATE_FROM,'DD-MON-RRRR') \"StrtDt\", TO_CHAR(A.P_DATE_TO, 'DD-MON-RRRR') \"EndDt\",A.P_NO_OF_DAYS \"NoOfDays\"," +
                    "A.LV_RSN \"Reason\",A.RQS_LV_ALLW \"ReqAllw\", A.H_RQSN_NMBR \"Rqst_No\", A.RQST_ID \"Rqst_Id\", A.RQST_CUR_STAGE \"CurStage\"," +
                    " A.LV_RST_MDFD_DATE \"ModDate\",null \"ExtNo\",A.LV_REL_OFF_NAME \"RelOff\",A.H_LV_RMN_DAYS \"RemDays\" FROM H_EMP_LV_RQST A, " +
                    "H_EMPLYE_MSTR B, H_LEAVE_TYPE_MSTR C WHERE A.H_EMPLYE_NMBR = B.HEMP_EMPLYE_NMBR AND A.COMP_AID=C.COMP_AID AND B.COMP_AID= C.COMP_AID " +
                    " AND A.H_LV_CODE=C.H_LEAVE_CODE AND B.COMP_AID = '" + compId + "' AND A.RQST_ID =" + reqId + " AND A.P_DATE_TO > SYSDATE  union select D.H_EMPLYE_NMBR \"EmpNo\", " +
                    "E.HEMP_EMPLYE_NAME \"EmpName\",D.H_LV_CODE \"LvCode\",F.H_LEAVE_DSCRPTN \"LvDesc\",TO_CHAR(D.N_DATE_FROM,'DD-MON-RRRR') \"StrtDt\", " +
                    "TO_CHAR(D.N_DATE_TO, 'DD-MON-RRRR') \"EndDt\",D.N_NO_OF_DAYS \"NoOfDays\",D.LV_RSN \"Reason\",null, D.H_RQST_NMBR \"Rqst_No\", " +
                    "D.RQST_ID \"Rqst_Id\", D.RQST_CUR_STAGE \"CurStage\", G.LV_RST_MDFD_DATE \"ModDate\",D.H_EXT_NMBR \"ExtNo\",G.LV_REL_OFF_NAME \"RelOff\", " +
                    "G.H_LV_RMN_DAYS \"RemDays\" FROM H_EMP_LV_EXTENSION D, H_EMPLYE_MSTR E, H_LEAVE_TYPE_MSTR F, H_EMP_LV_RQST G WHERE " +
                    "D.H_EMPLYE_NMBR = E.HEMP_EMPLYE_NMBR AND D.COMP_AID=F.COMP_AID AND E.COMP_AID= F.COMP_AID AND D.H_LV_CODE=F.H_LEAVE_CODE AND " +
                    "G.COMP_AID= F.COMP_AID AND E.COMP_AID = '" + compId + "' AND D.RQST_ID=" + reqId + " AND D.N_DATE_TO > SYSDATE ";

                return GetDataTable(Query);
            }

            //TRAINING
            public DataTable GetTrainiingCurrentStage(string compId, int reqId)
            {
                Query = "select HTNM_STS,RQST_CUR_STAGE from H_TRNG_NMNTN  WHERE COMP_AID ='" + compId + "' and RQST_ID =" + reqId;

                return GetDataTable(Query);
            }

            public DataTable SelectGrieveGl(string compId, int reqId)
            {
                Query = "select b.appr_emp_aid , d.hemp_emplye_name approver, t.emp_aid, c.hemp_emplye_name requester, d.hemp_email apprvr_email, " +
                            " c.hemp_email rqstr_email from h_emp_grievance t, ss_approval_employees b, h_emplye_mstr c, h_emplye_mstr d" +
                            " where t.comp_aid = b.comp_aid and t.comp_aid = d.comp_aid and t.comp_aid = c.comp_aid and t.glb_aid = b.rqst_id" +
                            " and c.hemp_emplye_nmbr = t.emp_aid and d.hemp_emplye_nmbr = b.appr_emp_aid" +
                            " and t.glb_aid = " + reqId + " and t.comp_aid ='" + compId + "'";

                return GetDataTable(Query);
            }

            public DataTable TrnApprovalValidate(string compId, string empNo)
            {
                Query = "Select APPR_EMP_AID from SS_APPROVAL_EMPLOYEES where COMP_AID='" + compId + "' and  APPR_EMP_AID=" + empNo + " and  RQST_CODE= 'TRN'";

                return GetDataTable(Query);
            }

            public DataTable TrnApprovalRequest(string compId, string reqId)
            {
                Query = "SELECT A.HTNM_TRNG_SRL_NMBR \"SrNo\",A.HTNM_TRNG_PRGRM_CODE \"Training\",B.HTRM_TRNG_DSCRPTN \"Training Description\"," +
                    "DECODE(A.HTNM_STS,'R', 'REQUEST') \"Status\", A.HTNM_HEMP_EMPLYE_NMBR \"Employee No\",C.HEMP_EMPLYE_NAME \"Employee Name\"," +
                    "A.HTNM_HEMP_EMPLYE_NOMIBY \"Nominating Empno\", D.HEMP_EMPLYE_NAME \"Nominating Emp Name\",A.HTNM_NMNTN_RSN \"Reason\"," +
                    "A.HTNM_MDFD_DATE \"Modified Date\", A.RQST_ID \"RequestID\", A.RQST_CUR_STAGE \"Current Stage\"  FROM H_TRNG_NMNTN A,H_TRNG_MSTR B," +
                    "H_EMPLYE_MSTR C,H_EMPLYE_MSTR D WHERE A.COMP_AID='" + compId + "' AND A.COMP_AID=B.COMP_AID  AND A.COMP_AID=C.COMP_AID " +
                    " AND A.COMP_AID=D.COMP_AID AND A.HTNM_TRNG_PRGRM_CODE=B.HTRM_TRNG_CODE AND A.HTNM_HEMP_EMPLYE_NMBR=C.HEMP_EMPLYE_NMBR " +
                    " AND A.HTNM_HEMP_EMPLYE_NOMIBY=D.HEMP_EMPLYE_NMBR  AND A.RQST_ID =" + reqId + " ORDER BY HTNM_TRNG_PRGRM_CODE";

                return GetDataTable(Query);
            }
        }

        public class UserProfile
        {
            public DataTable GetSpecificRecords(string compId, int empNo)
            {
                Query = "SELECT A.*, B.HGRD_DSCRPTN, D.HDPR_DSCRPTN ,D.HDPR_DSCRPTN,D.HDPR_DPRTMNT_CODE " + 
                    " FROM H_EMPLYE_MSTR A, H_GRDE_MSTR B, H_DPRTMNT_MSTR D WHERE A.HEMP_HDDT_HGRD_GRDE_CODE=B.HGRD_GRDE_CODE " +
                    " AND D.HDPR_DPRTMNT_CODE=A.HEMP_HDPR_DPRTMNT_CODE AND A.COMP_AID='" + compId + "' AND A.HEMP_EMPLYE_NMBR =" + empNo;

                return GetDataTable(Query);
            }

            public DataTable GetUserQualification(string compId, int empNo)
            {
                Query = "SELECT A.*, B.HDSC_DSCRPTN FROM H_EMPLYE_QLFCTN_DTL A, H_DSCPLNE_MSTR B " +
                        " WHERE A.HEQL_HDSC_DSCPLNE_CODE =B.HDSC_DSCPLNE_CODE  AND A.HEQL_HEMP_EMPLYE_NMBR='" + empNo + "' AND A.COMP_AID= '" + compId + "'";

                return GetDataTable(Query);
            }

            public DataTable GetUserProfCredential(string compId, int empNo)
            {
                Query = "SELECT A.*, B.HPMB_DSCRPTN FROM H_EMPLYE_PRFSNL_MBRSHP_DT A, H_PRFSNL_MBRSHP_MSTR B " +
                        " WHERE A.HEPM_HPMB_PRFSNL_MBRSHP=B.HPMB_PRFSNL_MBRSHP_CODE  AND  A.HEPM_HEMP_EMPLYE_NMBR='" + empNo + "' AND A.COMP_AID= '" + compId + "'";

                return GetDataTable(Query);
            }

            public DataTable GetEmployeeType(string empTyeCode)
            {
                Query = "SELECT HETY_EMPLYE_TYPE_CODE,HETY_DSCRPTN FROM H_EMPLYE_TYPE_MSTR WHERE HETY_EMPLYE_TYPE_CODE='" + empTyeCode + "'";

                return GetDataTable(Query);
            }

            public DataTable GetEmployeeReportedTo(int empNo)
            {
                Query = "SELECT HEMP_EMPLYE_NAME FROM H_EMPLYE_MSTR WHERE  HEMP_EMPLYE_NMBR='" + empNo + "'";

                return GetDataTable(Query);
            }
            
            public DataTable GetUserImage(int empNo)
            {
                Query = "Select IMAGE_DATA from PASSPORT WHERE USER_FK='" + empNo + "'";

                return GetDataTable(Query);
            }
        }

    }
}
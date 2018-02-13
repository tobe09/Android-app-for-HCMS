using System;

namespace PeoplePlusMobile
{
    public class User
    {
        //keys to access user data
        public static string CompId { get { return "CompId"; } }
        public static string CompName { get { return "CompName"; } }
        public static string RoleId { get { return "RoleId"; } }
        public static string UserId { get { return "UserId"; } }
        public static string Password { get { return "Password"; } }
        public static string Name { get { return "Name"; } }
        public static string Title { get { return "Title"; } }
        public static string DateOfBirth { get { return "DateOfBirth"; } }
        public static string AccountNo { get { return "AccountNo"; } }
        public static string Email { get { return "Email"; } }
        public static string DeptCode { get { return "DeptCode"; } }
        public static string GradeCode { get { return "GradeCode"; } }
        public static string EmployeeNo { get { return "EmployeeNo"; } }
        public static string LicenseDate { get { return "LicenseDate"; } }
        public static string AccountingYear { get { return "AccountingYear"; } }
        public static string Location { get { return "Location"; } }
        public static string LocationCode { get { return "LocationCode"; } }
        public static string Department { get { return "Department"; } }
        public static string Designation { get { return "Designation"; } }
        public static string Grade { get { return "Grade"; } }

        public static bool IsValidUser()
        {
            bool status = true;

            bool userIdStatus = string.IsNullOrEmpty(new AppPreferences().GetValue(UserId));
            bool passwordStatus = string.IsNullOrEmpty(new AppPreferences().GetValue(Password));
            if(userIdStatus || passwordStatus)
            {
                status = false;
            }

            return status;
        }
    }
}
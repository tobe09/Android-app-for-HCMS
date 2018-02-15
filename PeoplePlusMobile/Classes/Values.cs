namespace PeoplePlusMobile
{
    class Values
    {
        public const string ApiRootAddress = "http://192.168.0.4:8080/api/"; 
        //192.168.0.4:8080 (router)        //193.167.1.10:80 (dev machine)         //10.152.2.15:5000 (sharp proxy)

        public const string ErrorMsg = "An error has occured";

        public const string NetworkTimeoutMsg = "Network Timeout";

        public const string ServerErrorMsg = "Server side error has occured";

        public const string NoInternetMsg = "No Internet Connection";

        public const string StorageErrMsg = "System error has occured";

        public const int Delay = 20;        //in seconds

        public const string LoadingMsg = "Loading...";

        public const string WaitingMsg = "Please Wait...";

        public const string SuccessMsg = "Saved Successfully";
    }
}
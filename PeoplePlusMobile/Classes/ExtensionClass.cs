using Android.Widget;
using Android.Graphics;

namespace PeoplePlusMobile
{
    public static class ExtensionClass
    {
        public static void SuccessMsg(this TextView tvwMsg, string msg)
        {
            tvwMsg.SetTextColor(Color.Green);
            tvwMsg.Text = msg;
        }

        public static void ErrorMsg(this TextView tvwMsg, string msg)
        {
            tvwMsg.SetTextColor(Color.Red);
            tvwMsg.Text = msg;
        }

        public static void BasicMsg(this TextView tvwMsg, string msg)
        {
            tvwMsg.SetTextColor(Color.White);
            tvwMsg.Text = msg;
        }
    }
}
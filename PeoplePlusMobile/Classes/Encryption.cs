using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PeoplePlusMobile
{
    class Encryption
    {
        //to encrypt text
        public string EncryptText(string clearText)
        {
            string clearValue = "";
            for (int i = 0; i < clearText.Length; i++)
            {
                clearValue += (char)(clearText[i] + 10);
            }
            return clearValue;
        }
    }
}
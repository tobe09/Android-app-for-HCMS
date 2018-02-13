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
    }
}
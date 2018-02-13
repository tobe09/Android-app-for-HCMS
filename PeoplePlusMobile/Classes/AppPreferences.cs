using Android.Content;
using Android.Preferences;
using Android.App;
using System;
using System.Collections.Generic;

namespace PeoplePlusMobile
{
    public class AppPreferences
    {
        private ISharedPreferences sharedPrefs;
        private ISharedPreferencesEditor prefsEditor;


        public AppPreferences()
        {
            sharedPrefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            prefsEditor = sharedPrefs.Edit();
        }

        //save a string value
        public void SaveValue(string key, string value)
        {
            prefsEditor.PutString(key, value);
            prefsEditor.Commit();
        }

        //save a string value
        public void SaveValue(string key, List<string> value)
        {
            prefsEditor.PutStringSet(key, value);
            prefsEditor.Commit();
        }

        public string GetValue(string key)
        {
            return sharedPrefs.GetString(key, default(string));
        }

        public dynamic GetValue(string key, Type type)
        {
            dynamic value;

            if (type == typeof(string))
            {
                value = sharedPrefs.GetString(key, default(string));
            }
            else if (type == typeof(float))
            {
                value = sharedPrefs.GetFloat(key, default(float));
            }
            else if (type == typeof(int))
            {
                value = sharedPrefs.GetInt(key, default(int));
            }
            else
            {
                value = sharedPrefs.GetBoolean(key, default(bool));
            }

            return value;
        }

        public void RemoveValue(string key)
        {
            prefsEditor.Remove(key);
            prefsEditor.Commit();
        }

        ////save a float/double value
        //public void SaveValue(string key, float value)
        //{
        //    prefsEditor.PutFloat(key, value);
        //    prefsEditor.Commit();
        //}

        ////save a integer value
        //public void SaveValue(string key, int value)
        //{
        //    prefsEditor.PutInt(key, value);
        //    prefsEditor.Commit();
        //}

        ////save a boolean value
        //public void SaveValue(string key, bool value)
        //{
        //    prefsEditor.PutBoolean(key, value);
        //    prefsEditor.Commit();
        //}
    }
}
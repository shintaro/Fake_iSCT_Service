using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Diagnostics;

namespace FakeISCT
{
    class MyRegKey
    {
        private RegistryKey regkey;
        private EventLog eventLog;

        public MyRegKey()
        {
        }

        public MyRegKey(EventLog e)
        {
            eventLog = e;
        }

        public bool isKeyExist(string s)
        {
            try
            {
                regkey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(s, false);
            }
            catch (Exception e)
            {
                eventLog.WriteEntry("Error in isKeyExist() " + e.Message);
            }

            if (regkey == null)
            {
                eventLog.WriteEntry("Regisry Key doesn't exist");
                return false;
            }
            else
            {
                eventLog.WriteEntry("Regisry Key exists");
                regkey.Close();
                return true;
            }
        }

        public void setValues(string name, string key, string value)
        {
            string[] s = new string[] { key, value };
            try
            {
                regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(name);
            }
            catch (Exception e)
            {
                eventLog.WriteEntry("Cannot Create Registry Key " + e.Message);
                return;
            }
            try
            {
                regkey.SetValue("StringArray", s);
            }
            catch (Exception e)
            {
                eventLog.WriteEntry("Cannot Set Value " + e.Message);
                return;
            }

            regkey.Close();
        }

        public string[] readValue(string name)
        {
            try
            {
                regkey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(name, true);
            }
            catch (Exception e)
            {
                eventLog.WriteEntry("Cannot Open Registy Key " + e.Message);
                return null;
            }
            try
            {
                string[] s = (string[])regkey.GetValue("StringArray");
                regkey.Close();
                return s;
            }
            catch (Exception e)
            {
                eventLog.WriteEntry("Cannot Read Value " + e.Message);
                regkey.Close();
                return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using TaskScheduler;
using System.Net;

namespace FakeISCT
{
    public partial class FakeISCT : ServiceBase
    {
        private Thread workerThread = null;
        private String serverIPAddress;
        private MySocketClient socket;
        private TaskSchedulerManager task;
        private readonly int port = 6001;
        private int systemWakeTime = 60;
        private int systemSleepTime = 120;
        private MyRegKey regkey;
        private readonly string regKeyName = @"Software\FakeISCT\";

        public FakeISCT()
        {
            InitializeComponent();
            if (!System.Diagnostics.EventLog.SourceExists("FakeISCTSource"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "FakeISCTSource", "FakeISCTLog");
            }
            eventLog1.Source = "FakeICSTSource";
            eventLog1.Log = "FakeISCTLog";
            eventLog1.WriteEntry("FakeISCT start");
        }

        internal static class UnsafeNativeMethods
        {
            [DllImport("powrprof.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool SetSuspendState(
                [In, MarshalAs(UnmanagedType.I1)] bool Hibernate,
                [In, MarshalAs(UnmanagedType.I1)] bool ForceCritical,
                [In, MarshalAs(UnmanagedType.I1)] bool DisableWakeEvent);
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("In OnStart!!!");
            regkey = new MyRegKey(eventLog1);
            if (regkey.isKeyExist(regKeyName))
            {
                string[] s = regkey.readValue(regKeyName);
                try
                {
                    systemWakeTime = Int32.Parse(s[0]);
                    systemSleepTime = Int32.Parse(s[1]);
                }
                catch (Exception e)
                {
                    eventLog1.WriteEntry("Wrong Registry Value" + e.Message);
                }
            }
            else
            {
                regkey.setValues(regKeyName, systemWakeTime.ToString(), systemSleepTime.ToString());
            }

            if (isValidLocalIPAddress(getLocalIPAddress()))
            {
                eventLog1.WriteEntry("Got Valid IP Address");
                serverIPAddress = getServerIPAddress(getLocalIPAddress());
                task = new TaskSchedulerManager(eventLog1);

                if ((workerThread == null) || ((workerThread.ThreadState & (System.Threading.ThreadState.Unstarted | System.Threading.ThreadState.Stopped)) != 0))
                {
                    workerThread = new Thread(new ThreadStart(ServiceWorkerMethod));
                    workerThread.Start();
                }
                if (workerThread != null)
                {
                }
            }
            else
            {
                eventLog1.WriteEntry("Invalid local IP address");
            }
        }

        protected override void OnContinue()
        {
            eventLog1.WriteEntry("In OnContinue!!!");
        }

        protected override void OnPause()
        {
            eventLog1.WriteEntry("In OnPause!!!");
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("In OnStop!!!");
            // New in .NET Framework version 2.0.
            this.RequestAdditionalTime(4000);
            // Signal the worker thread to exit.
            if ((workerThread != null) && (workerThread.IsAlive))
            {
                //pause.Reset();
                Thread.Sleep(5000);
                workerThread.Abort();
            }
            if (workerThread != null)
            {
            }
            // Indicate a successful exit.
            this.ExitCode = 0;
        }

        public void ServiceWorkerMethod()
        {
            eventLog1.WriteEntry("Entering ServiceWorkerMethod");
            try
            {
                do
                {
                    eventLog1.WriteEntry("The system awake " + systemWakeTime.ToString() + " sec");
                    Thread.Sleep(systemWakeTime * 1000);

                    socket = new MySocketClient(eventLog1);
                    socket.open(getServerIPAddress(getLocalIPAddress()), port);
                    socket.sendSleepTime(systemSleepTime);
                    socket.close();

                    task.addWakeUpTask(systemSleepTime);
                    eventLog1.WriteEntry("The system is going to sleep " + systemSleepTime + "sec");
                    Thread.Sleep(3000);
                    UnsafeNativeMethods.SetSuspendState(false, false, false);
                }
                while (true);
            }
            catch (ThreadAbortException)
            {
                eventLog1.WriteEntry("Worker Thread is terminated");
            }
            eventLog1.WriteEntry("Exiting ServiceWorkerThread");
        }

        private static string getLocalIPAddress()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    return ip.ToString();
                }
            }
            return "";
        }

        private static bool isValidLocalIPAddress(string s)
        {
            return int.Parse(s.Substring(0, s.IndexOf('.'))) == 192 ? true : false;
        }

        private static String getServerIPAddress(String s)
        {
            return s.Substring(0, s.LastIndexOf('.')) + ".1";
        }
    }
}

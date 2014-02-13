using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using Microsoft.VisualBasic.ApplicationServices;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Method based on Single Instance Detection Sample http://msdn.microsoft.com/en-us/library/ms771662.aspx
    ///  Family.Show must be single instance to prevent temporary files being overwritten.
    ///
    /// Using VB bits to detect single instances and process accordingly:
    /// * OnStartup is fired when the first instance loads
    /// * OnStartupNextInstance is fired when the application is re-run again
    /// 
    /// NOTE: it is redirected to this instance thanks to IsSingleInstance 
    /// </summary>
    internal class SingleInstanceManager : WindowsFormsApplicationBase
    {
        private App _app;

        public SingleInstanceManager()
        {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs eventArgs)
        {
            // First time app is launched.
            _app = new App();
            _app.InitializeComponent();
            _app.ProcessArgs(eventArgs.CommandLine.ToArray(), true);            
            _app.Run();

            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            // Subsequent launches.
            base.OnStartupNextInstance(eventArgs);
            
            _app.Activate();
            _app.ProcessArgs(eventArgs.CommandLine.ToArray(), false);
        }

        [STAThread]
        static void Main(string[] args)
        {
            SingleInstanceManager manager = new SingleInstanceManager();
            manager.Run(args);
        }
    }
}

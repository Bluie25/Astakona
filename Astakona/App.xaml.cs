using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Astakona
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public LoggedInDetails LoggedInUser { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LoggedInUser = new LoggedInDetails();
        }

        public void ClearLoggedInUserData()
        {
            LoggedInUser = new LoggedInDetails();
        }
    }
}

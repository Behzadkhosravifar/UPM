using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;

namespace UPM
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Thread thCheckDB = new Thread(new ThreadStart(checkDB));
            thCheckDB.Start();
        }

        private delegate void closer();
        private void closeForm()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            try
            {
                if (this.Dispatcher.CheckAccess())
                {
                    this.Close();
                }
                else
                {
                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        new closer(closeForm));
                }
            }
            catch { }
        }

        private void checkDB()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.High;
            //
            // Create first connect to Database
            var db = new LINQtoSQLDataContext();
            db.Dispose();
            //
            // check pass
            if (new LINQ_UserPassDataContext().User_Passwords.ToArray().Length > 1)
                MainWindow.canShowPasswordWin = true;
            else
                MainWindow.canShowPasswordWin = false;
            //

            Thread.Sleep(3000);
            closeForm();
            Thread.CurrentThread.Abort();
        }
    }
}

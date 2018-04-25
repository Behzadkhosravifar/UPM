using System;
using System.Collections.Generic;
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

namespace DisconnectDB
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			this.InitializeComponent();

			// Insert code required on object creation below this point.
		}

        Thread thCopy;
		private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
            foreach (System.Diagnostics.Process clsProcess in System.Diagnostics.Process.GetProcesses())
            {
                //now we're going to see if any of the running processes
                //match the currently running processes by using the StartsWith Method,
                //this prevents us from including the .EXE for the process we're looking for.
                //. Be sure to not
                //add the .exe to the name you provide, i.e: NOTEPAD,
                //not NOTEPAD.EXE or false is always returned even if
                //notepad is running

                try
                {
                    if (clsProcess.ProcessName.StartsWith("UPM"))
                    {
                        //since we found the process we now need to use the
                        //Kill Method to kill the process. Remember, if you have
                        //the process running more than once, say IE open 4
                        //times the loop three way it is now will close all 4,
                        //if you want it to just close the first one it finds
                        //then add a return; after the Kill
                        clsProcess.Kill();
                        //process killed, return true
                    }
                }
                catch { }

            }
            thCopy = new Thread(new ThreadStart(StartCopy));
            thCopy.TrySetApartmentState(ApartmentState.MTA);
            thCopy.IsBackground = true;
            thCopy.Start();
		}

        private void StartCopy()
        {
            thCopy.Join(2000);
            try
            {
                string FileName = System.IO.File.ReadAllText("Copy.Log");
                System.IO.File.Copy("UPM.mdf", FileName, true);
                System.IO.File.Delete("Copy.Log");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source);
            }
            finally
            {
                thCopy.Join(2000);
                System.Diagnostics.Process.Start("UPM.exe");
                Application.Current.Dispatcher.InvokeShutdown();
                if (!Application.Current.Dispatcher.HasShutdownFinished)
                {
                    Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    Application.Current.Shutdown();
                }
            }
        }
	}
}
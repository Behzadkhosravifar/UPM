using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Linq;

namespace UPM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static bool canShowPasswordWin = false;
        private Storyboard AboutMouseEnter;
        private Storyboard removeBtnsGrid;
        private Storyboard AboutMouseLeave;
        private Storyboard removeBtnsGrid_Revers;
        private DispatcherTimer dispatcherTimer;
        private bool entryAbout = false;
        private string FileName = string.Empty;
        private System.Threading.Thread thImport;

        public MainWindow()
        {
            SplashScreen ss = new SplashScreen();
            ss.ShowDialog();
            if (canShowPasswordWin)
            {
                PasswordWin pw = new PasswordWin();
                pw.ShowDialog();
                if (!PasswordWin.correctPassword)
                    Application.Current.Shutdown();
            }
            this.InitializeComponent();
            // Insert code required on object creation below this point.
            AboutMouseEnter = (Storyboard)FindResource("AboutMouseEnter");
            removeBtnsGrid = (Storyboard)FindResource("removeBtnsGrid");
            AboutMouseLeave = (Storyboard)FindResource("AboutMouseLeave");
            AboutMouseLeave.Completed += new EventHandler(AboutMouseLeave_Completed);
            removeBtnsGrid_Revers = (Storyboard)FindResource("removeBtnsGrid_Revers");
            removeBtnsGrid_Revers.Completed += new EventHandler(removeBtnsGrid_Revers_Completed);
            //
            // initialize timer
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(800);
        }

        void removeBtnsGrid_Revers_Completed(object sender, EventArgs e)
        {
            removeBtnsGrid_Revers.Remove();
        }

        void AboutMouseLeave_Completed(object sender, EventArgs e)
        {
            AboutMouseLeave.Remove();
        }

        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnRestoreSize_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // TODO: Add event handler implementation here.
        }

        private void btnMinimize_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // Begin dragging the window
            // if mouse down on RectangleWin
            if (((e.GetPosition(this).Y < RectangleWinTop.ActualHeight - 10) ||
                (e.GetPosition(this).Y > RectangleWin.ActualHeight - 40)) &&
                (e.GetPosition(this).X < RectangleWin.ActualWidth - 5))
                this.DragMove();
        }

        #region Taskbar Buttons
        private void ThumbButtonInfo_Email_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("mailto:Nadia.Tavassoli@gmail.com");
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Stop); }
        }

        private void ThumbButtonInfo_Keys_Click(object sender, EventArgs e)
        {
            if (canShowPasswordWin)
            {
                PasswordWin pw = new PasswordWin();
                this.Hide();
                pw.ShowDialog();
                if (PasswordWin.correctPassword)
                {
                    this.Show();
                }
                else
                    Application.Current.Shutdown();
            }
        }

        private void ThumbButtonInfo_Close_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {

            // open URL

            Hyperlink source = sender as Hyperlink;

            if (source != null)
            {
                try
                {
                    System.Diagnostics.Process.Start(source.NavigateUri.ToString());
                }
                catch (Exception ex)
                { MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Stop); }
            }

        }

        private void pathAbout_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //
            // Begin About Animation
            if (!entryAbout)
            {
                dispatcherTimer.Start();
            }

        }
        private void pathAbout_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            dispatcherTimer.Stop();
        }
        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            AboutMouseEnter.Begin();
            removeBtnsGrid.Begin();
            entryAbout = true;
            dispatcherTimer.Stop();
        }

        private void canvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //
            // Remove About Animation
            if (entryAbout)
            {
                AboutMouseLeave.Begin();
                removeBtnsGrid_Revers.Begin();
                AboutMouseEnter.Remove();
                removeBtnsGrid.Remove();
                entryAbout = false;
            }
        }

        #region Menu Buttons
        //
        // Show User Accounts Window
        private void btn4_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UserAccountsWin uaw = new UserAccountsWin();
            uaw.ShowDialog();
        }
        //
        // Import Database file (*.mdf)
        private void btn3_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofdExportAll = new Microsoft.Win32.OpenFileDialog();
            try
            {
                ofdExportAll.Title = "Import Database file *.mdf";
                ofdExportAll.Filter = @"Database files|*.mdf";
                ofdExportAll.DefaultExt = "Backup.mdf";
                ofdExportAll.FileName = "Backup.mdf";
                ofdExportAll.CheckFileExists = true;
                ofdExportAll.CheckPathExists = true;

                if (ofdExportAll.ShowDialog() == true)
                {
                    // First Remove old database
                    btn1_Click(sender, e);
                    //
                    // import . . . 
                    try
                    {
                        FileName = ofdExportAll.FileName;
                        this.prbMain.Visibility = System.Windows.Visibility.Visible;
                        this.prbMain.Value = 0;
                        this.Cursor = Cursors.Wait;
                        //
                        // Restore .mdf file codes
                        thImport = new System.Threading.Thread(new System.Threading.ThreadStart(import));
                        thImport.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "اشکال در بازیابی اطلاعات");
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, ex.Source); return; }
        }
        //
        // Export Database file (*.mdf)
        private void btn2_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog sfdExportAll = new Microsoft.Win32.SaveFileDialog();
            try
            {
                sfdExportAll.Title = "Export All Database file *.mdf";
                sfdExportAll.Filter = @"Database files|*.mdf";
                sfdExportAll.DefaultExt = "Backup.mdf";
                sfdExportAll.FileName = "Backup.mdf";

                if (sfdExportAll.ShowDialog() == true)
                {
                    try
                    {
                        System.IO.File.WriteAllText("Copy.Log", sfdExportAll.FileName);
                        System.Diagnostics.Process.Start("DisconnectDB.exe");
                        Application.Current.Dispatcher.InvokeShutdown();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "اشکال در استخراج اطلاعات");
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, ex.Source); return; }
        }
        //
        // New Database and delete old data
        private void btn1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure to clear old Database data?",
                "Warning to New Database",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var originalDB = new LINQtoSQLDataContext();
                //
                // Delete All data from Database
                #region Delete Query
                string query = @"DELETE FROM dbo.Project_Student_Relation " +
                               @"DELETE FROM dbo.Student " +
                               @"DELETE FROM dbo.Project " +
                               @"DELETE FROM dbo.Professor " +
                               @"DELETE FROM dbo.Education_Degree " +
                               @"DELETE FROM dbo.Branch";
                originalDB.ExecuteQuery<object>(query);                    
                originalDB.SubmitChanges();

                //
                // manual delete data . . .
                //
                originalDB = new LINQtoSQLDataContext();
                //
                // @"DELETE FROM dbo.Project_Student_Relation "
                foreach (var db in originalDB.Project_Student_Relations.ToList())
                {
                    originalDB.Project_Student_Relations.DeleteOnSubmit(db);
                }
                originalDB.SubmitChanges();
                //
                // @"DELETE FROM dbo.Student "
                foreach (var db in originalDB.Students.ToList())
                {
                    originalDB.Students.DeleteOnSubmit(db);
                }
                originalDB.SubmitChanges();
                //
                // @"DELETE FROM dbo.Project "
                foreach (var db in originalDB.Projects.ToList())
                {
                    originalDB.Projects.DeleteOnSubmit(db);
                }
                originalDB.SubmitChanges();
                //
                // @"DELETE FROM dbo.Professor "
                foreach (var db in originalDB.Professors.ToList())
                {
                    originalDB.Professors.DeleteOnSubmit(db);
                }
                originalDB.SubmitChanges();
                //
                //  @"DELETE FROM dbo.Education_Degree "
                foreach (var db in originalDB.Education_Degrees.ToList())
                {
                    originalDB.Education_Degrees.DeleteOnSubmit(db);
                }
                originalDB.SubmitChanges();
                //
                //  @"DELETE FROM dbo.Branch"
                foreach (var db in originalDB.Branches.ToList())
                {
                    originalDB.Branches.DeleteOnSubmit(db);
                }
                originalDB.SubmitChanges();
                //
                // refresh db!!!
                originalDB.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues);
                #endregion
            }
        }
        #endregion

        #region Import Thread WorkDo
        private void import()
        {
            try
            {
                setProgressBarValue(1);
                #region Import Code
                #region Connect to Database
                var originalDB = new LINQtoSQLDataContext();
                var newDB = new LINQtoSQLDataContext(FileName);
                #endregion
                setProgressBarValue(10);
                //
                // Delete All Older data in Database
                #region Delete Query
                string query = @"DELETE FROM dbo.Project_Student_Relation " +
                               @"DELETE FROM dbo.Student " +
                               @"DELETE FROM dbo.Project " +
                               @"DELETE FROM dbo.Professor " +
                               @"DELETE FROM dbo.Education_Degree " +
                               @"DELETE FROM dbo.Branch";
                originalDB.ExecuteQuery<object>(query);
                originalDB.SubmitChanges();
                #endregion
                setProgressBarValue(20);
                //
                // Insert New Data in Database
                #region 1.  Education Branch DATA

                var BranchTable = newDB.Branches;
                foreach (var anyRow in BranchTable)
                {
                    originalDB.Branches.InsertOnSubmit(new Branch()
                    {
                        ID = anyRow.ID,
                        Branch_Name = anyRow.Branch_Name
                    });
                }
                originalDB.SubmitChanges();

                #endregion
                setProgressBarValue(30);

                #region 2.  Education Degree DATA

                var DegreeTable = newDB.Education_Degrees;
                foreach (var anyRow in DegreeTable)
                {
                    originalDB.Education_Degrees.InsertOnSubmit(new Education_Degree()
                    {
                        ID = anyRow.ID,
                        DegreeName = anyRow.DegreeName
                    });
                }
                originalDB.SubmitChanges();

                #endregion
                setProgressBarValue(40);

                #region 3.  Professor DATA

                var ProfessorTable = newDB.Professors;
                foreach (var anyRow in ProfessorTable)
                {
                    originalDB.Professors.InsertOnSubmit(new Professor()
                    {
                        ID = anyRow.ID,
                        FirstName = anyRow.FirstName,
                        LastName = anyRow.LastName,
                        Degree_ID = anyRow.Degree_ID,
                        Email = anyRow.Email,
                        PhoneNo = anyRow.PhoneNo,
                        Branch_ID = anyRow.Branch_ID
                    });
                }
                originalDB.SubmitChanges();

                #endregion
                setProgressBarValue(50);

                #region 4.  Student DATA

                var StudentTable = newDB.Students;
                foreach (var anyRow in StudentTable)
                {
                    originalDB.Students.InsertOnSubmit(new Student()
                    {
                        ID = anyRow.ID,
                        FirstName = anyRow.FirstName,
                        LastName = anyRow.LastName,
                        Student_Code = anyRow.Student_Code,
                        Branch_ID = anyRow.Branch_ID,
                        Degree_ID = anyRow.Degree_ID,
                        Phone_No = anyRow.Phone_No,
                        Email = anyRow.Email,
                        BirthDay = anyRow.BirthDay,
                        Entry_Year = anyRow.Entry_Year,
                        Entry_Term = anyRow.Entry_Term
                    });
                }
                originalDB.SubmitChanges();

                #endregion
                setProgressBarValue(60);

                #region 5.  Project DATA

                var ProjectTable = newDB.Projects;
                foreach (var anyRow in ProjectTable)
                {
                    originalDB.Projects.InsertOnSubmit(new Project()
                    {
                        ID = anyRow.ID,
                        ProposalText = anyRow.ProposalText,
                        Project_Name = anyRow.Project_Name,
                        Project_Professor_ID = anyRow.Project_Professor_ID,
                        Guide_Professor_ID = anyRow.Guide_Professor_ID,
                        Manager_Professor_ID = anyRow.Manager_Professor_ID,
                        Juror_Professor_ID = anyRow.Juror_Professor_ID,
                        Start_Date = anyRow.Start_Date,
                        End_Date = anyRow.End_Date,
                        Guide_Grade = anyRow.Guide_Grade,
                        Juror_Grade = anyRow.Juror_Grade
                    });
                }
                originalDB.SubmitChanges();

                #endregion
                setProgressBarValue(70);

                #region 6.  Project Student Relation DATA

                var PSRTable = newDB.Project_Student_Relations;
                foreach (var anyRow in PSRTable)
                {
                    originalDB.Project_Student_Relations.InsertOnSubmit(new Project_Student_Relation()
                    {
                        Project_ID = anyRow.Project_ID,
                        Student_ID = anyRow.Student_ID
                    });
                }
                originalDB.SubmitChanges();

                #endregion
                setProgressBarValue(80);

                #region Close Database Connections
                originalDB.Connection.Close();
                originalDB.Dispose();
                newDB.Connection.Close();
                newDB.Dispose();
                #endregion
                setProgressBarValue(100);
                #endregion
                //
                // end of work
                System.Threading.Thread.CurrentThread.Join(1000);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, ex.Source); }
            finally
            {
                //
                // Main Progress bar visibility change to hidden -------------
                #region this.prbMain.Visibility = System.Windows.Visibility.Hidden;
                if (!prbMain.Dispatcher.CheckAccess())
                {
                    prbMain.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                      new Action(delegate()
                      {
                          prbMain.Visibility = System.Windows.Visibility.Hidden;
                      }
                    ));
                }
                else
                {
                    prbMain.Visibility = System.Windows.Visibility.Hidden;
                }
                #endregion
                // -----------------------------------------------------------
                //
                #region this.Cursor = Cursors.Arrow;
                if (!prbMain.Dispatcher.CheckAccess())
                {
                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                      new Action(delegate()
                      {
                          this.Cursor = Cursors.Arrow;
                      }
                    ));
                }
                else
                {
                    this.Cursor = Cursors.Arrow;
                }
                #endregion
                //
                // Dispose this Thread
                System.Threading.Thread.CurrentThread.Abort();
            }
        }

        private void setProgressBarValue(int v)
        {
            if (!prbMain.Dispatcher.CheckAccess())
            {
                prbMain.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                  new Action(delegate()
                  {
                      prbMain.Value = v;
                  }
                ));
            }
            else
            {
                prbMain.Value = v;
            }
        }
        #endregion

        #region MainForm Buttons
        private void btnProfessor_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AddRemoveEditView_Form AREV_form = new AddRemoveEditView_Form(this.Top, this.Left, this.Width, this.Height, "Professor");
            AREV_form.ShowDialog();
        }

        private void btnStudent_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AddRemoveEditView_Form AREV_form = new AddRemoveEditView_Form(this.Top, this.Left, this.Width, this.Height, "Student");
            AREV_form.ShowDialog();
        }

        private void btnProject_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AddRemoveEditView_Form AREV_form = new AddRemoveEditView_Form(this.Top, this.Left, this.Width, this.Height, "Project");
            AREV_form.ShowDialog();
        }

        private void btnDegree_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EduDegree ed = new EduDegree();
            ed.ShowDialog();
        }

        private void btnBranch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EduBranch eb = new EduBranch();
            eb.ShowDialog();
        }
        #endregion
    }
}
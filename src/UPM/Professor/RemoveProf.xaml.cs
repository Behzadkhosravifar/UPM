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
using System.Linq;
using System.Windows.Threading;

namespace UPM
{
    /// <summary>
    /// Interaction logic for RemoveProf.xaml
    /// </summary>
    public partial class RemoveProf : Window
    {
        LINQtoSQLDataContext DB;
        private DispatcherTimer setColumnNames; // for set columns header after 400 millisecond.

        public RemoveProf()
        {
            this.InitializeComponent();

            setColumnNames = new DispatcherTimer();
            setColumnNames.Interval = new TimeSpan(400);
            setColumnNames.Tick += new EventHandler(setColumnNames_Tick);

            DB = new LINQtoSQLDataContext();
            setProfessorTable();
        }

        void setColumnNames_Tick(object sender, EventArgs e)
        {
            if (dgvProfessor.Columns.Count > 0)
            {
                dgvProfessor.Columns[0].Header = "ID";
                dgvProfessor.Columns[1].Header = "First Name";
                dgvProfessor.Columns[2].Header = "Last Name";
                dgvProfessor.Columns[3].Header = "Education Degree";
                dgvProfessor.Columns[4].Header = "Email Address";
                dgvProfessor.Columns[5].Header = "Phone No.";
                dgvProfessor.Columns[6].Header = "Education Branch";
            }

            setColumnNames.Stop();
        }

        #region Control Buttons
        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }
        private void btnRestoreSize_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Normal)
                this.WindowState = System.Windows.WindowState.Maximized;
            else
                this.WindowState = System.Windows.WindowState.Normal;
        }
        private void btnMinimize_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // Begin dragging the window
            // if mouse down on LayoutRoot
            if ((e.GetPosition(this).Y < 50) &&
                (e.GetPosition(this).X < LayoutRoot.ActualWidth))
                this.DragMove();
        }
        #endregion

        private void setProfessorTable()
        {
            dgvProfessor.ItemsSource = (from p in DB.Professors
                                        select new
                                        {
                                            p.ID,
                                            p.FirstName,
                                            p.LastName,
                                            p.Education_Degree.DegreeName,
                                            p.Email,
                                            p.PhoneNo,
                                            p.Branch.Branch_Name
                                        });

            setColumnNames.Start();
        }

        int LastID = -1;
        private void dgvProfessor_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgvProfessor.SelectedIndex >= 0)
            {
                //
                // First find record id in DB
                string id = "";
                string row = dgvProfessor.SelectedItem.ToString();
                char[] chRow = row.ToCharArray();
                for (int i = row.IndexOf("ID = ") + "ID = ".Length; i < row.Length; i++)
                {
                    if (char.IsDigit(chRow[i])) id += chRow[i].ToString();
                    else break;
                }

                if (int.TryParse(id, out LastID))
                {
                    //
                    // select record in DB by funded numID
                    var product = (from p in DB.Professors
                                   where p.ID == LastID
                                   select p).Single();
                    //
                    // fill the all textBox and comboBox by selected data
                    txtSelectedRow.Text =
                        string.Format("ID = {0}{8}First Name = {1}{8}Last Name = {2}{7}" +
                        "Phone No. = {3}{8}Email = {4}{7}Branch = {5}{8}Education Degree = {6}", 
                        product.ID, product.FirstName, product.LastName,
                        product.PhoneNo, product.Email, product.Branch.Branch_Name,
                        product.Education_Degree.DegreeName, Environment.NewLine, "    ,    ");
                }
            }
        }

        private void btnRemove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (LastID < 0)
            {
                MessageBox.Show("Please Select a Professor of list for edit it.", "Empty Selection");
                return;
            }
            else if (MessageBox.Show("Are you sure to delete the professor by this row information's?",
                "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    //
                    // select record in DB by funded numID
                    var product = (from p in DB.Professors
                                   where p.ID == LastID
                                   select p).Single();
                    //
                    // Lock to delete Permission of this record according by relation to another records
                    if ((from p in DB.Projects
                         where ((p.Guide_Professor_ID == LastID) || (p.Juror_Professor_ID == LastID)
                            || (p.Manager_Professor_ID == LastID) || (p.Project_Professor_ID == LastID))
                         select p).ToList().Count > 0) // do not permission to delete
                    {
                        MessageBox.Show(" You don't to delete this record!" +
                            "\n because this record have relation by" +
                            "\n another record of project Table.", "Not Permission");
                        return;
                    }
                    //
                    // delete this record codes
                    DB.Professors.DeleteOnSubmit((from p in DB.Professors
                                                  where p.ID == LastID
                                                  select p).Single());
                    //
                    // set the DB by new data
                    DB.SubmitChanges();
                    MessageBox.Show("The record removed Successfully!");
                    setProfessorTable();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, ex.Source); }
            }
        }
    }
}
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
	/// Interaction logic for RemoveStudent.xaml
	/// </summary>
	public partial class RemoveStudent : Window
	{
        LINQtoSQLDataContext DB;
        private DispatcherTimer setColumnNames; // for set columns header after 400 millisecond.

		public RemoveStudent()
		{
			this.InitializeComponent();

            setColumnNames = new DispatcherTimer();
            setColumnNames.Interval = new TimeSpan(400);
            setColumnNames.Tick += new EventHandler(setColumnNames_Tick);

            DB = new LINQtoSQLDataContext();
            setStudentTable();
		}

        private void setStudentTable()
        {
            dgvStudent.ItemsSource = (from s in DB.Students
                                      select new
                                      {
                                          s.ID,
                                          s.FirstName,
                                          s.LastName,
                                          s.Student_Code,
                                          s.Branch.Branch_Name,
                                          s.Education_Degree.DegreeName,
                                          s.BirthDay,
                                          s.Email,
                                          s.Phone_No,
                                          s.Entry_Year,
                                          EntryTerm = ((s.Entry_Term) ? "First Term" : "Second Term")
                                      });

            setColumnNames.Start();
        }

        void setColumnNames_Tick(object sender, EventArgs e)
        {
            if (dgvStudent.Columns.Count > 0)
            {
                #region Create Columns Header
                dgvStudent.Columns[0].Header = "ID";
                dgvStudent.Columns[1].Header = "First Name";
                dgvStudent.Columns[2].Header = "Last Name";
                dgvStudent.Columns[3].Header = "Student Code";
                dgvStudent.Columns[4].Header = "Education Branch";
                dgvStudent.Columns[5].Header = "Education Degree";
                dgvStudent.Columns[6].Header = "BirthDay";
                dgvStudent.Columns[7].Header = "Email Address";
                dgvStudent.Columns[8].Header = "Phone No.";
                dgvStudent.Columns[9].Header = "Entry Year";
                dgvStudent.Columns[10].Header = "Entry Term";
                #endregion
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

        int LastID = -1;
        private void btnRemove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (LastID < 0)
            {
                MessageBox.Show("Please Select a Student of list for edit it.", "Empty Selection");
                return;
            }
            else if (MessageBox.Show("Are you sure to delete the student by this row information's?",
                "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    //
                    // select record in DB by funded numID
                    var product = (from s in DB.Students
                                   where s.ID == LastID
                                   select s).Single();
                    //
                    // Lock to delete Permission of this record according by relation to another records
                    if ((from ps in DB.Project_Student_Relations
                         where ps.Student_ID == product.ID
                         select ps).ToList().Count > 0) // do not permission to delete
                    {
                        MessageBox.Show(" You don't to delete this record!" +
                            "\n because this record have relation by" +
                            "\n another record of project Table.", "Not Permission");
                        return;
                    }
                    //
                    // delete this record codes
                    DB.Students.DeleteOnSubmit((from s in DB.Students
                                                where s.ID == LastID
                                                select s).Single());
                    //
                    // set the DB by new data
                    DB.SubmitChanges();
                    MessageBox.Show("The record removed Successfully!");
                    setStudentTable();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, ex.Source); }
            }
        }

        private void dgvStudent_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgvStudent.SelectedIndex >= 0)
            {
                //
                // First find record id in DB
                string id = "";
                string row = dgvStudent.SelectedItem.ToString();
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
                    var product = (from s in DB.Students
                                   where s.ID == LastID
                                   select s).Single();
                    //
                    // fill the all textBox and comboBox by selected data
                    txtSelectedRow.Text =
                        string.Format("ID = {2}{1}First Name = {3}{1}Last Name = {4}{0}" +
                        "Student Code = {5}{1}Birthday = {8}{0}" +
                        "Phone No. = {6}{1}Email = {7}{0}" +
                        "Branch = {9}{1}Education Degree = {10}{0}" +
                        "Entry Year = {11}{1}Entry Term = {12} Term",
                        Environment.NewLine, "    ,    ",
                        product.ID, product.FirstName, product.LastName, product.Student_Code,
                        product.Phone_No, product.Email, product.BirthDay,
                        product.Branch.Branch_Name, product.Education_Degree.DegreeName,
                        product.Entry_Year, (product.Entry_Term) ? "First" : "Second");
                }
            }
        }
	}
}
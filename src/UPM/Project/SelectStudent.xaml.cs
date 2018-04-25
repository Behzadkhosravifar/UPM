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
	/// Interaction logic for SelectStudent.xaml
	/// </summary>
	public partial class SelectStudent : Window
	{
        LINQtoSQLDataContext DB;
        private DispatcherTimer setColumnNames; // for set columns header after 400 millisecond.
        private Student selectedStudent;
        public Student SelectedStudent
        {
            get { return selectedStudent; }
        }

        public SelectStudent()
        {
            this.InitializeComponent();

            setColumnNames = new DispatcherTimer();
            setColumnNames.Interval = new TimeSpan(400);
            setColumnNames.Tick += new EventHandler(setColumnNames_Tick);

            DB = new LINQtoSQLDataContext();
            setStudentTable();
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
                dgvStudent.Columns[4].Header = "Entry Year - Term";                
                dgvStudent.Columns[5].Header = "Phone No.";
                #endregion
            }

            setColumnNames.Stop();
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
                                          Entry = string.Format("{0} - {1}", 
                                                s.Entry_Year, 
                                                ((s.Entry_Term) ? "First Term" : "Second Term")),
                                          s.Phone_No
                                      });

            setColumnNames.Start();
        }

        #region textbox highlighting code
        List<Student> fundedStudents = new List<Student>();
        private void txtSearchStudentFName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (DB == null) return;
        	//
            // Search Student by Student First name
            fundedStudents = DB.Students.ToList();

            if (!string.IsNullOrEmpty(txtSearchStudentFName.Text) && txtSearchStudentFName.Text != "نام دانشجو")
            {
                fundedStudents = (from s in fundedStudents
                                  where s.FirstName.Contains(txtSearchStudentFName.Text)
                                  select s).ToList();
            }

            dgvStudent.ItemsSource = (from s in fundedStudents
                                      select new
                                      {
                                          s.ID,
                                          s.FirstName,
                                          s.LastName,
                                          s.Student_Code,
                                          Entry = string.Format("{0} - {1}",
                                                s.Entry_Year,
                                                ((s.Entry_Term) ? "First Term" : "Second Term")),
                                          s.Phone_No
                                      }).ToList(); 

            setColumnNames.Start();
        }

        private void txtSearchStudentLName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (DB == null) return;
            //
            // Search Student by Student Last Name
            fundedStudents = DB.Students.ToList();

            if (!string.IsNullOrEmpty(txtSearchStudentLName.Text) && txtSearchStudentLName.Text != "نام خانوادگی دانشجو")
            {
                fundedStudents = (from s in fundedStudents
                                  where s.LastName.Contains(txtSearchStudentLName.Text)
                                  select s).ToList();
            }

            dgvStudent.ItemsSource = (from s in fundedStudents
                                      select new
                                      {
                                          s.ID,
                                          s.FirstName,
                                          s.LastName,
                                          s.Student_Code,
                                          Entry = string.Format("{0} - {1}",
                                                s.Entry_Year,
                                                ((s.Entry_Term) ? "First Term" : "Second Term")),
                                          s.Phone_No
                                      }).ToList();

            setColumnNames.Start();
        }

        private void txtSearchStudentFName_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (((TextBox)sender).Text == "نام دانشجو")
            {
                ((TextBox)sender).Text = string.Empty;
            }
        }

        private void txtSearchStudentFName_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(((TextBox)sender).Text))
            {
                ((TextBox)sender).Text = "نام دانشجو";
            }
        }

        private void txtSearchStudentFName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (((TextBox)sender).Text == "نام دانشجو")
            {
                ((TextBox)sender).Text = string.Empty;
            }
        }

        private void txtSearchStudentFName_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
        	if (string.IsNullOrEmpty(((TextBox)sender).Text))
            {
                ((TextBox)sender).Text = "نام دانشجو";
            }
        }

        private void txtSearchStudentLName_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (((TextBox)sender).Text == "نام خانوادگی دانشجو")
            {
                ((TextBox)sender).Text = string.Empty;
            }
        }

        private void txtSearchStudentLName_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(((TextBox)sender).Text))
            {
                ((TextBox)sender).Text = "نام خانوادگی دانشجو";
            }
        }
        

        private void txtSearchStudentLName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (((TextBox)sender).Text == "نام خانوادگی دانشجو")
            {
                ((TextBox)sender).Text = string.Empty;
            }
        }

        private void txtSearchStudentLName_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(((TextBox)sender).Text))
            {
                ((TextBox)sender).Text = "نام خانوادگی دانشجو";
            }
        }

        #endregion

        private void btnOk_Click(object sender, System.Windows.RoutedEventArgs e)
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
                int ID;
                if (int.TryParse(id, out ID))
                {
                    //
                    // select record in DB by funded numID
                    selectedStudent = (from s in DB.Students
                                       where s.ID == ID
                                       select s).Single();
                }
            }
            else selectedStudent = null;
            this.Close();
        }

        private void btnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            selectedStudent = null;
            this.Close();
        }
    }
}
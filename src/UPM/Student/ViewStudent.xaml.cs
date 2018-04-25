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
	/// Interaction logic for ViewStudent.xaml
	/// </summary>
	public partial class ViewStudent : Window
	{
        LINQtoSQLDataContext DB;
        string previousText_EntryYear; // for Undo EntryYear texts, if it hasn't a numerical text!
        string previousText_StudentCode; // for Undo StudentCode texts, if it hasn't a numerical text!
        private DispatcherTimer setColumnNames; // for set columns header after 400 millisecond.

		public ViewStudent()
		{
			this.InitializeComponent();

            setColumnNames = new DispatcherTimer();
            setColumnNames.Interval = new TimeSpan(400);
            setColumnNames.Tick += new EventHandler(setColumnNames_Tick);

            DB = new LINQtoSQLDataContext();
            setStudentTable();

            // fill EducationBranch ComboBox by DB.Branches data
            foreach (var any in (from edb in DB.Branches select edb.Branch_Name).ToList())
                cmbEducationBranch.Items.Add(any);
            //
            // fill EducationDegree ComboBox by DB.Education_Degrees data
            foreach (var any in (from edd in DB.Education_Degrees select edd.DegreeName).ToList())
                cmbEducationDegree.Items.Add(any);
            //
            // fill EntryTerm ComboBox by 'First Term' and 'Second Term'
            cmbTerm.Items.Add("First Term");
            cmbTerm.Items.Add("Second Term");
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

        private void btnSearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // check empty textbox and combo box
            if (string.IsNullOrEmpty(txtFirstName.Text) &&
               string.IsNullOrEmpty(txtLastName.Text) &&
               string.IsNullOrEmpty(txtStudentCode.Text) &&
               string.IsNullOrEmpty(txtEntryYear.Text) &&
               cmbTerm.SelectedIndex < 0 &&
               cmbEducationBranch.SelectedIndex < 0 &&
               cmbEducationDegree.SelectedIndex < 0) // All are empty!
            {
                setStudentTable();
            }
            else // at least a field that is not empty!
            {
                bool combinedSearch = false;
                List<Student> fundedStudents = new List<Student>();
                //
                #region find according by First Name
                if (!string.IsNullOrEmpty(txtFirstName.Text))
                {
                    fundedStudents = (from s in DB.Students
                                      where s.FirstName.Contains(txtFirstName.Text)
                                      select s).ToList();
                    combinedSearch = true;
                }
                #endregion
                //
                #region find according by Last Name
                if (!string.IsNullOrEmpty(txtLastName.Text))
                {
                    //
                    // Whether was already find something? Combined Search
                    if (combinedSearch) // yes
                    {
                        //
                        // search in funded professors ...
                        fundedStudents = (from s in fundedStudents
                                          where s.LastName.Contains(txtLastName.Text)
                                          select s).ToList();
                    }
                    else // no
                    {
                        // new search in DB
                        fundedStudents = (from s in DB.Students
                                          where s.LastName.Contains(txtLastName.Text)
                                          select s).ToList();
                        combinedSearch = true;
                    }
                }
                #endregion
                //
                #region find according by Student Code
                if (!string.IsNullOrEmpty(txtStudentCode.Text))
                {
                    //
                    // Whether was already find something? Combined Search
                    if (combinedSearch) // yes
                    {
                        //
                        // search in funded professors ...
                        fundedStudents = (from s in fundedStudents
                                          where s.Student_Code == decimal.Parse(txtStudentCode.Text)
                                          select s).ToList();
                    }
                    else // no
                    {
                        // new search in DB
                        fundedStudents = (from s in DB.Students
                                          where s.Student_Code == decimal.Parse(txtStudentCode.Text)
                                          select s).ToList();
                        combinedSearch = true;
                    }
                }
                #endregion
                //
                #region find according by Entry Year
                if (!string.IsNullOrEmpty(txtEntryYear.Text))
                {
                    //
                    // Whether was already find something? Combined Search
                    if (combinedSearch) // yes
                    {
                        //
                        // search in funded professors ...
                        fundedStudents = (from s in fundedStudents
                                          where s.Entry_Year == decimal.Parse(txtEntryYear.Text)
                                          select s).ToList();
                    }
                    else // no
                    {
                        // new search in DB
                        fundedStudents = (from s in DB.Students
                                          where s.Entry_Year == decimal.Parse(txtEntryYear.Text)
                                          select s).ToList();
                        combinedSearch = true;
                    }
                }
                #endregion
                //
                #region find according by Entry Term
                if (cmbTerm.SelectedIndex >= 0 &&
                    cmbTerm.SelectedIndex < cmbTerm.Items.Count)
                {
                    bool firstSelected = (cmbTerm.SelectedItem.ToString() == "First Term");
                    //
                    // Whether was already find something? Combined Search
                    if (combinedSearch) // yes
                    {
                        //
                        // search in funded professors ...
                        fundedStudents = (from s in fundedStudents
                                          where s.Entry_Term == firstSelected
                                          select s).ToList();
                    }
                    else // no
                    {
                        // new search in DB
                        fundedStudents = (from s in DB.Students
                                          where s.Entry_Term == firstSelected
                                          select s).ToList();
                        combinedSearch = true;
                    }
                }
                #endregion
                //
                #region find according by Education Degree
                if (cmbEducationDegree.SelectedIndex >= 0 &&
                    cmbEducationDegree.SelectedIndex < cmbEducationDegree.Items.Count)
                {
                    //
                    // Whether was already find something? Combined Search
                    if (combinedSearch) // yes
                    {
                        //
                        // search in funded professors ...
                        fundedStudents = (from s in fundedStudents
                                          where s.Education_Degree.DegreeName == (string)cmbEducationDegree.SelectedItem
                                          select s).ToList();
                    }
                    else // no
                    {
                        // new search in DB
                        fundedStudents = (from s in DB.Students
                                          where s.Education_Degree.DegreeName == (string)cmbEducationDegree.SelectedItem
                                          select s).ToList();
                        combinedSearch = true;
                    }
                }
                #endregion
                //
                #region find according by Education Branch
                if (cmbEducationBranch.SelectedIndex >= 0 &&
                    cmbEducationBranch.SelectedIndex < cmbEducationBranch.Items.Count)
                {
                    //
                    // Whether was already find something? Combined Search
                    if (combinedSearch) // yes
                    {
                        //
                        // search in funded professors ...
                        fundedStudents = (from s in fundedStudents
                                          where s.Branch.Branch_Name == (string)cmbEducationBranch.SelectedItem
                                          select s).ToList();
                    }
                    else // no
                    {
                        // new search in DB
                        fundedStudents = (from s in DB.Students
                                          where s.Branch.Branch_Name == (string)cmbEducationBranch.SelectedItem
                                          select s).ToList();
                        combinedSearch = true;
                    }
                }
                #endregion
                //
                // set DataGridView DataSource:
                dgvStudent.ItemsSource = (from s in fundedStudents
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
                                          }).ToList();

                setColumnNames.Start();
            }
        }

        private void btnShowAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            txtStudentCode.Text = "";
            txtEntryYear.Text = "";
            txtFirstName.Text = "";
            txtLastName.Text = "";
            cmbTerm.Text = "";
            cmbEducationBranch.Text = "";
            cmbEducationDegree.Text = "";

            setStudentTable();
        }

        /// <summary>
        /// Check Numerical TextBox text and max length is 4 digit
        /// </summary>
        /// <param name="sender">EntryYear TextBox</param>
        /// <param name="e">Text of TextBox</param>
        private void txtEntryYear_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(((TextBox)sender).Text))
                previousText_EntryYear = "";
            else
            {
                decimal num = 0;
                bool success = decimal.TryParse(((TextBox)sender).Text, out num);
                if (success & num >= 0 & ((TextBox)sender).Text.Length < 5)
                {
                    ((TextBox)sender).Text.Trim();
                    previousText_EntryYear = ((TextBox)sender).Text;
                }
                else
                {
                    ((TextBox)sender).Text = previousText_EntryYear;
                    ((TextBox)sender).SelectionStart = ((TextBox)sender).Text.Length;
                }
            }
        }

        /// <summary>
        /// Check Numerical TextBox text!
        /// </summary>
        /// <param name="sender">StudentCode TextBox</param>
        /// <param name="e">Text of TextBox</param>
        private void txtStudentCode_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(((TextBox)sender).Text))
                previousText_StudentCode = "";
            else
            {
                decimal num = 0;
                bool success = decimal.TryParse(((TextBox)sender).Text, out num);
                if (success & num >= 0)
                {
                    ((TextBox)sender).Text.Trim();
                    previousText_StudentCode = ((TextBox)sender).Text;
                }
                else
                {
                    ((TextBox)sender).Text = previousText_StudentCode;
                    ((TextBox)sender).SelectionStart = ((TextBox)sender).Text.Length;
                }
            }
        }
	}
}
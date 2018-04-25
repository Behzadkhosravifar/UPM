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
using Arash.PersianDateControls;
using System.Windows.Threading;

namespace UPM
{
	/// <summary>
	/// Interaction logic for AddStudent.xaml
	/// </summary>
	public partial class AddStudent : Window
	{
        LINQtoSQLDataContext DB;
        string previousText_PhoneNo; // for Undo PhoneNo. texts, if it hasn't a numerical text!
        string previousText_EntryYear; // for Undo EntryYear texts, if it hasn't a numerical text!
        string previousText_StudentCode; // for Undo StudentCode texts, if it hasn't a numerical text!
        private DispatcherTimer setColumnNames; // for set columns header after 400 millisecond.

		public AddStudent()
		{
			this.InitializeComponent();
            
            dpEnBirthday.GotFocus += new RoutedEventHandler(dpEnBirthday_GotFocus);
            dpEnBirthday.SelectedDateChanged += new EventHandler<SelectionChangedEventArgs>(dpEnBirthday_SelectedDateChanged);
            dpFaDatePicker.GotFocus += new RoutedEventHandler(dpFaDatePicker_GotFocus);
            dpFaDatePicker.SelectedDateChanged += new RoutedEventHandler(dpFaDatePicker_SelectedDateChanged);

            setColumnNames = new DispatcherTimer();
            setColumnNames.Interval = new TimeSpan(400);
            setColumnNames.Tick += new EventHandler(setColumnNames_Tick);

            DB = new LINQtoSQLDataContext();
            setStudentTable();

            previousText_PhoneNo = txtPhoneNo.Text;

            // fill EducationBranch ComboBox by DB.Branches data
            foreach (var any in (from edb in DB.Branches select edb.Branch_Name).ToList())
                cmbEducationBranch.Items.Add(any);
            //
            // fill EducationDegree ComboBox by DB.Education_Degrees data
            foreach (var any in (from edd in DB.Education_Degrees select edd.DegreeName).ToList())
                cmbEducationDegree.Items.Add(any);

            //
            // set DatePicker date to Today.
            dpEnBirthday.SelectedDate = DateTime.Today;
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

        #region Calendar Codes
        void dpFaDatePicker_SelectedDateChanged(object sender, RoutedEventArgs e)
        {
            dpEnBirthday.SelectedDate = dpFaDatePicker.SelectedDate.ToDateTime();
            rbtnFaDatePicker.IsChecked = true;
            rbtnEnDatePicker.IsChecked = false;            
        }

        void dpFaDatePicker_GotFocus(object sender, RoutedEventArgs e)
        {
            rbtnFaDatePicker.IsChecked = true;
            rbtnEnDatePicker.IsChecked = false;
        }

        void dpEnBirthday_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpEnBirthday.SelectedDate.HasValue)
                dpFaDatePicker.SelectedDate = new Arash.PersianDate((DateTime)dpEnBirthday.SelectedDate);
            rbtnFaDatePicker.IsChecked = false;
            rbtnEnDatePicker.IsChecked = true;
        }

        void dpEnBirthday_GotFocus(object sender, RoutedEventArgs e)
        {
            rbtnFaDatePicker.IsChecked = false;
            rbtnEnDatePicker.IsChecked = true;
        }
        #endregion

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

        private void btnAdd_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (txtFirstName.Text != string.Empty && 
                txtLastName.Text != string.Empty &&
                txtStudentCode.Text != string.Empty && 
                txtEntryYear.Text != string.Empty &&
                cmbEducationBranch.SelectedIndex >= 0 && 
                cmbEducationDegree.SelectedIndex >= 0)
            {
                if (!txtEmail.Text.ValidateEmail() && !string.IsNullOrEmpty(txtEmail.Text)) // not valid email address
                {
                    MessageBox.Show("The Entry Email Address is not Validate!", "InValid DataContext");
                    txtEmail.Focus();
                    return;
                }
                try
                {
                    // find selected branch ID of comboBox from Database and return it.
                    int branch = (from b in DB.Branches
                                  where b.Branch_Name == (string)cmbEducationBranch.SelectedItem
                                  select b.ID).Single();
                    //
                    // find selected Education degree ID of comboBox from Database and return it.
                    int degree = (from d in DB.Education_Degrees
                                  where d.DegreeName == (string)cmbEducationDegree.SelectedItem
                                  select d.ID).Single();

                    decimal num;
                    Student s = new Student()
                    {
                        FirstName = txtFirstName.Text,
                        LastName = txtLastName.Text,
                        Student_Code = (decimal.TryParse(txtStudentCode.Text, out num) ? num : 0),
                        Entry_Year = (decimal.TryParse(txtEntryYear.Text, out num) ? num : 0),
                        Entry_Term = ((bool)rbtnFirstTerm.IsChecked) ? true : false,
                        Email = txtEmail.Text,
                        Branch_ID = branch,
                        Degree_ID = degree,
                        Phone_No = txtPhoneNo.Text,
                        BirthDay = (((bool)rbtnFaDatePicker.IsChecked) ? dpFaDatePicker.SelectedDate.ToString() :
                            (dpEnBirthday.SelectedDate.HasValue) ? dpEnBirthday.SelectedDate.Value.ToShortDateString() : "")
                    };

                    DB.Students.InsertOnSubmit(s);
                    DB.SubmitChanges();

                    rbtnFirstTerm.IsChecked = true;
                    txtStudentCode.Text = "";
                    txtEntryYear.Text = "";
                    txtEmail.Text = "";
                    txtFirstName.Text = "";
                    txtLastName.Text = "";
                    txtPhoneNo.Text = "";
                    cmbEducationBranch.Text = "";
                    cmbEducationDegree.Text = "";

                    txtFirstName.Focus();

                    setStudentTable();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, ex.Source); }
            }
            else
            {
                MessageBox.Show("Please, First fill the All TextBox and ComboBox", "Empty TextBox or ComboBox");
                return;
            }
        }

        #region Check Validate Code
        private void txtEmail_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (txtEmail.Text == "")
            {
                txtEmailValidate.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (txtEmail.Text.ValidateEmail())
            {
                txtEmailValidate.Text = "The Entry Email Address is Valid.";
                txtEmailValidate.Foreground = Brushes.Green;
                txtEmailValidate.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                txtEmailValidate.Text = "The Entry Email Address is Not Valid!";
                txtEmailValidate.Foreground = Brushes.Red;
                txtEmailValidate.Visibility = System.Windows.Visibility.Visible;
            }
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
                ushort num = 0;
                bool success = ushort.TryParse(((TextBox)sender).Text, out num);
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
                UInt64 num = 0;
                bool success = UInt64.TryParse(((TextBox)sender).Text, out num);
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

        /// <summary>
        /// Check Numerical TextBox text!
        /// </summary>
        /// <param name="sender">PhoneNo. TextBox</param>
        /// <param name="e">Text of TextBox</param>
        private void txtPhoneNo_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(((TextBox)sender).Text))
                previousText_PhoneNo = "";
            else
            {
                UInt64 num = 0;
                bool success = UInt64.TryParse(((TextBox)sender).Text, out num);
                if (success & num >= 0)
                {
                    ((TextBox)sender).Text.Trim();
                    previousText_PhoneNo = ((TextBox)sender).Text;
                }
                else
                {
                    ((TextBox)sender).Text = previousText_PhoneNo;
                    ((TextBox)sender).SelectionStart = ((TextBox)sender).Text.Length;
                }
            }
        }
        #endregion
    }
}
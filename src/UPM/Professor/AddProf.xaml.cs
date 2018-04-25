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
	/// Interaction logic for AddProf.xaml
	/// </summary>
	public partial class AddProf : Window
	{
        LINQtoSQLDataContext DB;
        string previousText; // for Undo PhoneNo. texts, if it hasn't a numerical text!
        private DispatcherTimer setColumnNames; // for set columns header after 400 millisecond.

		public AddProf()
		{
			this.InitializeComponent();

            setColumnNames = new DispatcherTimer();
            setColumnNames.Interval = new TimeSpan(400);
            setColumnNames.Tick += new EventHandler(setColumnNames_Tick);

            DB = new LINQtoSQLDataContext();
            setProfessorTable();

            previousText = txtPhoneNo.Text;
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

        private void btnAdd_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (txtFirstName.Text != string.Empty && txtLastName.Text != "" &&
                cmbEducationBranch.SelectedIndex>=0 && cmbEducationDegree.SelectedIndex>=0)
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

                    decimal phone;
                    Professor p = new Professor()
                    {
                        FirstName = txtFirstName.Text,
                        LastName = txtLastName.Text,
                        Email = txtEmail.Text,
                        Branch_ID = branch,
                        Degree_ID = degree,
                        PhoneNo = (decimal.TryParse(txtPhoneNo.Text, out phone) ? phone : new Nullable<decimal>())
                    };
                    DB.Professors.InsertOnSubmit(p);
                    DB.SubmitChanges();
                    txtEmail.Text = "";
                    //txtEmailValidate.Visibility = false;
                    txtFirstName.Text = "";
                    txtLastName.Text = "";
                    txtPhoneNo.Text = "";
                    cmbEducationBranch.Text = "";
                    cmbEducationDegree.Text = "";
                    txtFirstName.Focus();

                    setProfessorTable();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, ex.Source); }
            }
            else
            {
                MessageBox.Show("Please, First fill the All TextBox and ComboBox", "Empty TextBox or ComboBox");
                return;
            }
        }

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

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // fill EducationBranch ComboBox by DB.Branches data
            foreach (var any in (from edb in DB.Branches select edb.Branch_Name).ToList())
                cmbEducationBranch.Items.Add(any);
            //
            // fill EducationDegree ComboBox by DB.Education_Degrees data
            foreach (var any in (from edd in DB.Education_Degrees select edd.DegreeName).ToList())
                cmbEducationDegree.Items.Add(any);
        }

        /// <summary>
        /// Check Numerical TextBox text!
        /// </summary>
        /// <param name="sender">PhoneNo. TextBox</param>
        /// <param name="e">Text of TextBox</param>
        private void txtPhoneNo_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(((TextBox)sender).Text))
                previousText = "";
            else
            {
                UInt64 num = 0;
                bool success = UInt64.TryParse(((TextBox)sender).Text, out num);
                if (success & num >= 0)
                {
                    ((TextBox)sender).Text.Trim();
                    previousText = ((TextBox)sender).Text;
                }
                else
                {
                    ((TextBox)sender).Text = previousText;
                    ((TextBox)sender).SelectionStart = ((TextBox)sender).Text.Length;
                }
            }
        }
	}
}
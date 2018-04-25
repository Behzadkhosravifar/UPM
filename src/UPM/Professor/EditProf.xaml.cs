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
    /// Interaction logic for EditProf.xaml
    /// </summary>
    public partial class EditProf : Window
    {
        LINQtoSQLDataContext DB;
        string previousText; // for Undo PhoneNo. texts, if it hasn't a numerical text!
        private DispatcherTimer setColumnNames; // for set columns header after 400 millisecond.

        public EditProf()
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
                    txtFirstName.Text = product.FirstName;
                    txtLastName.Text = product.LastName;
                    txtPhoneNo.Text = (product.PhoneNo.HasValue) ? product.PhoneNo.Value.ToString() : "";
                    txtEmail.Text = product.Email;
                    cmbEducationBranch.Text = product.Branch.Branch_Name;
                    cmbEducationDegree.Text = product.Education_Degree.DegreeName;
                }
            }
        }

        private void btnEdit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (txtFirstName.Text != string.Empty && txtLastName.Text != "" &&
                cmbEducationBranch.SelectedIndex >= 0 && cmbEducationDegree.SelectedIndex >= 0)
            {
                if (!txtEmail.Text.ValidateEmail() && !string.IsNullOrEmpty(txtEmail.Text)) // not valid email address
                {
                    MessageBox.Show("The Entry Email Address is not Validate!", "InValid DataContext");
                    txtEmail.Focus();
                    return;
                }
                if (LastID < 0)
                {
                    MessageBox.Show("Please Select a Professor of list for edit it.", "Empty Selection");
                    return;
                }
                try
                {
                    // find selected branch of comboBox from Database and return it.
                    Branch branch = (from b in DB.Branches
                                 where b.Branch_Name == (string)cmbEducationBranch.SelectedItem
                                 select b).Single();
                    //
                    // find selected Education degree of comboBox from Database and return it.
                    Education_Degree degree = (from d in DB.Education_Degrees
                                               where d.DegreeName == (string)cmbEducationDegree.SelectedItem
                                               select d).Single();

                    //
                    // select record in DB by funded numID
                    var product = (from p in DB.Professors
                                   where p.ID == LastID
                                   select p).Single();
                    //
                    // Edit ...
                    decimal phone;
                    product.FirstName = txtFirstName.Text;
                    product.LastName = txtLastName.Text;
                    product.Email = txtEmail.Text;
                    product.Branch = branch;
                    product.Education_Degree = degree;
                    product.PhoneNo = (decimal.TryParse(txtPhoneNo.Text, out phone) ? phone : new Nullable<decimal>());
                    
                    //
                    // set the DB by new data
                    DB.SubmitChanges();
                    MessageBox.Show("The record changed Successfully!");
                    setProfessorTable();

                    //
                    // Clear TextBox.Text
                    txtEmail.Text = "";
                    txtFirstName.Text = "";
                    txtLastName.Text = "";
                    txtPhoneNo.Text = "";
                    cmbEducationBranch.Text = "";
                    cmbEducationDegree.Text = "";
                    LastID = -1;
                    dgvProfessor.UnselectAll();
                    txtFirstName.Focus();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, ex.Source); }
            }
            else
            {
                MessageBox.Show("Please, First fill the All TextBox and ComboBox", "Empty TextBox or ComboBox");
                return;
            }
        }
    }
}
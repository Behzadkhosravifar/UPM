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

namespace UPM
{
	/// <summary>
	/// Interaction logic for EduDegree.xaml
	/// </summary>
    public partial class EduDegree : Window
    {
        LINQtoSQLDataContext DB;
        public EduDegree()
        {
            this.InitializeComponent();

            DB = new LINQtoSQLDataContext();
            setEduDegreeTable();
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

        private void setEduDegreeTable()
        {
            dgvEducationDegree.ItemsSource = (from edd in DB.Education_Degrees
                                              select new
                                              {
                                                  edd.ID,
                                                  edd.DegreeName
                                              }).ToList();
            if (dgvEducationDegree.Columns.Count > 0)
            {
                dgvEducationDegree.Columns[1].Header = "Education Degree";
                dgvEducationDegree.Columns[0].Header = "ID";
            }
        }

        private void btnAdd_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (txtEducationDegree.Text != string.Empty)
            {
                if (txtEducationDegree.Text.Length > 100)
                {
                    MessageBox.Show("Number of characters written is too large!", "Overflow Error");
                    txtEducationDegree.Focus();
                    return;
                }
                Education_Degree ed = new Education_Degree() { DegreeName = this.txtEducationDegree.Text };
                DB.Education_Degrees.InsertOnSubmit(ed);
                DB.SubmitChanges();
                txtEducationDegree.Text = "";
                setEduDegreeTable();
            }
            else
            {
                MessageBox.Show("Please, First fill the Education Degree Name TextBox", "Empty TextBox");
                txtEducationDegree.Focus();
            }
        }

        private void btnEdit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (txtEducationDegree.Text != string.Empty)
            {
                if (txtEducationDegree.Text.Length > 100)
                {
                    MessageBox.Show("Number of characters written is too large!", "Overflow Error");
                    txtEducationDegree.Focus();
                    return;
                }
                if (dgvEducationDegree.SelectedIndex > -1 &&
                    dgvEducationDegree.SelectedIndex < dgvEducationDegree.Items.Count)
                {
                    //
                    // First find record id in DB
                    string id = "";
                    string row = dgvEducationDegree.Items[dgvEducationDegree.SelectedIndex].ToString();
                    char[] chRow = row.ToCharArray();
                    for (int i = row.IndexOf("ID = ") + "ID = ".Length; i < row.Length; i++)
                    {
                        if (char.IsDigit(chRow[i])) id += chRow[i].ToString();
                        else break;
                    }
                    int numID;
                    if (int.TryParse(id, out numID))
                    {
                        //
                        // select record in DB by funded numID
                        var product = (from p in DB.Education_Degrees
                                       where p.ID == numID
                                       select p).Single();
                        //
                        // Edit ...
                        product.DegreeName = txtEducationDegree.Text;
                        //
                        // Clear TextBox.Text
                        txtEducationDegree.Text = "";
                        //
                        // set the DB by new data
                        try
                        {
                            DB.SubmitChanges();
                            MessageBox.Show("The record changed Successfully!");
                            setEduDegreeTable();
                        }
                        catch (Exception ex) { MessageBox.Show(ex.Message, ex.Source); }
                    }
                    else
                        MessageBox.Show("Can not Parse the ID to integer value!");
                }
                else
                    MessageBox.Show("Please select a record of list to being edit!");
            }
            else
            {
                MessageBox.Show("Please, First fill the Education Degree Name TextBox", "Empty TextBox");
                txtEducationDegree.Focus();
            }
        }

        private void btnRemove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (dgvEducationDegree.SelectedIndex > -1 &&
                dgvEducationDegree.SelectedIndex < dgvEducationDegree.Items.Count)
            {
                //
                // First find record id in DB
                string id = "";
                string row = dgvEducationDegree.Items[dgvEducationDegree.SelectedIndex].ToString();
                char[] chRow = row.ToCharArray();
                for (int i = row.IndexOf("ID = ") + "ID = ".Length; i < row.Length; i++)
                {
                    if (char.IsDigit(chRow[i])) id += chRow[i].ToString();
                    else break;
                }
                int numID;
                if (int.TryParse(id, out numID))
                {
                    //
                    // select record in DB by funded numID
                    var product = (from p in DB.Education_Degrees
                                   where p.ID == numID
                                   select p).Single();
                    //
                    // Lock to delete Permission of this record according by relation to another records
                    if ((from p in DB.Professors
                         where p.Degree_ID == numID
                         select p).ToList().Count > 0) // do not permission to delete
                    {
                        MessageBox.Show(" You don't to delete this record!" +
                            "\n because this record have relation by" +
                            "\n another record of professor Table.", "Not Permission");
                        return;
                    }
                    else if ((from s in DB.Students
                              where s.Degree_ID == numID
                              select s).ToList().Count > 0) // do not permission to delete
                    {
                        MessageBox.Show(" You don't to delete this record!" +
                            "\n because this record have relation by" +
                            "\n another record of student Table.", "Not Permission");
                        return;
                    }
                    try
                    {
                        //
                        // delete this record codes
                        DB.Education_Degrees.DeleteOnSubmit((from ed in DB.Education_Degrees
                                                             where ed.ID == numID
                                                             select ed).Single());
                        //
                        // set the DB by new data
                        DB.SubmitChanges();
                        MessageBox.Show("The record removed Successfully!");
                        setEduDegreeTable();
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message, ex.Source); }
                }
                else
                    MessageBox.Show("Can not Parse the ID to integer value!");
            }
            else
                MessageBox.Show("Please select a record of list to being edit!");
        }
    }
}
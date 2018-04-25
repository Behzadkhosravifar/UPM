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
	/// Interaction logic for EduBranch.xaml
	/// </summary>
	public partial class EduBranch : Window
	{
        LINQtoSQLDataContext DB;
		public EduBranch()
		{
			this.InitializeComponent();

            DB = new LINQtoSQLDataContext();
            setEduBranchTable();
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

        private void setEduBranchTable()
        {
            dgvEducationBranch.ItemsSource = (from edb in DB.Branches
                                              select new
                                              {
                                                  edb.ID,
                                                  edb.Branch_Name
                                              }).ToList();
            if (dgvEducationBranch.Columns.Count > 0)
            {
                dgvEducationBranch.Columns[1].Header = "Education Branch";
                dgvEducationBranch.Columns[0].Header = "ID";
            }
        }

        private void btnAdd_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (txtEducationBranch.Text != string.Empty)
            {
                if (txtEducationBranch.Text.Length > 100)
                {
                    MessageBox.Show("Number of characters written is too large!", "Overflow Error");
                    txtEducationBranch.Focus();
                    return;
                }
                Branch eb = new Branch() { Branch_Name = this.txtEducationBranch.Text };
                DB.Branches.InsertOnSubmit(eb);
                DB.SubmitChanges();
                txtEducationBranch.Text = "";
                setEduBranchTable();
            }
            else
            {
                MessageBox.Show("Please, First fill the Education Branch Name TextBox", "Empty TextBox");
                txtEducationBranch.Focus();
            }
        }

        private void btnEdit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (txtEducationBranch.Text != string.Empty)
            {
                if (txtEducationBranch.Text.Length > 100)
                {
                    MessageBox.Show("Number of characters written is too large!", "Overflow Error");
                    txtEducationBranch.Focus();
                    return;
                }
                if (dgvEducationBranch.SelectedIndex > -1 &&
                    dgvEducationBranch.SelectedIndex < dgvEducationBranch.Items.Count)
                {
                    //
                    // First find record id in DB
                    string id = "";
                    string row = dgvEducationBranch.Items[dgvEducationBranch.SelectedIndex].ToString();
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
                        var product = (from p in DB.Branches
                                       where p.ID == numID
                                       select p).Single();
                        //
                        // Edit ...
                        product.Branch_Name = txtEducationBranch.Text;
                        //
                        // Clear TextBox.Text
                        txtEducationBranch.Text = "";
                        //
                        // set the DB by new data
                        try
                        {
                            DB.SubmitChanges();
                            MessageBox.Show("The record changed Successfully!");
                            setEduBranchTable();
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
                MessageBox.Show("Please, First fill the Education Branch Name TextBox", "Empty TextBox");
                txtEducationBranch.Focus();
            }
        }

        private void btnRemove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (dgvEducationBranch.SelectedIndex > -1 &&
                dgvEducationBranch.SelectedIndex < dgvEducationBranch.Items.Count)
            {
                //
                // First find record id in DB
                string id = "";
                string row = dgvEducationBranch.Items[dgvEducationBranch.SelectedIndex].ToString();
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
                    var product = (from p in DB.Branches
                                   where p.ID == numID
                                   select p).Single();
                    //
                    // Lock to delete Permission of this record according by relation to another records
                    if ((from p in DB.Professors
                         where p.Branch_ID == numID
                         select p).ToList().Count > 0) // do not permission to delete
                    {
                        MessageBox.Show(" You don't to delete this record!" +
                            "\n because this record have relation by" +
                            "\n another record of professor Table.", "Not Permission");
                        return;
                    }
                    else if ((from s in DB.Students
                              where s.Branch_ID == numID
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
                        DB.Branches.DeleteOnSubmit((from ed in DB.Branches
                                                    where ed.ID == numID
                                                    select ed).Single());
                        //
                        // set the DB by new data
                        DB.SubmitChanges();
                        MessageBox.Show("The record removed Successfully!");
                        setEduBranchTable();
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
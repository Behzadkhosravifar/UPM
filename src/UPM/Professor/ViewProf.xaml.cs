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
	/// Interaction logic for ViewProf.xaml
	/// </summary>
	public partial class ViewProf : Window
	{
        LINQtoSQLDataContext DB;
        private DispatcherTimer setColumnNames; // for set columns header after 400 millisecond.

		public ViewProf()
		{
			this.InitializeComponent();

            setColumnNames = new DispatcherTimer();
            setColumnNames.Interval = new TimeSpan(400);
            setColumnNames.Tick += new EventHandler(setColumnNames_Tick);

            DB = new LINQtoSQLDataContext();
            setProfessorTable();

            // fill EducationBranch ComboBox by DB.Branches data
            foreach (var any in (from edb in DB.Branches select edb.Branch_Name).ToList())
                cmbEducationBranch.Items.Add(any);
            //
            // fill EducationDegree ComboBox by DB.Education_Degrees data
            foreach (var any in (from edd in DB.Education_Degrees select edd.DegreeName).ToList())
                cmbEducationDegree.Items.Add(any);
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
               (cmbEducationBranch.SelectedIndex < 0 ||
               cmbEducationBranch.SelectedIndex >= cmbEducationBranch.Items.Count) &&
               (cmbEducationDegree.SelectedIndex < 0 ||
               cmbEducationDegree.SelectedIndex >= cmbEducationDegree.Items.Count)) // All are empty!
            {
                setProfessorTable();
            }
            else // at least a field that is not empty!
            {
                bool combinedSearch = false;
                List<Professor> fundedProfessors = new List<Professor>();
                //
                #region find according by First Name
                if (!string.IsNullOrEmpty(txtFirstName.Text))
                {
                    fundedProfessors = (from p in DB.Professors
                                        where p.FirstName.Contains(txtFirstName.Text)
                                        select p).ToList();
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
                        fundedProfessors = (from p in fundedProfessors
                                            where p.LastName.Contains(txtLastName.Text)
                                            select p).ToList();
                    }
                    else // no
                    {
                        // new search in DB
                        fundedProfessors = (from p in DB.Professors
                                            where p.LastName.Contains(txtLastName.Text)
                                            select p).ToList();
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
                        fundedProfessors = (from p in fundedProfessors
                                            where p.Education_Degree.DegreeName == (string)cmbEducationDegree.SelectedItem
                                            select p).ToList();
                    }
                    else // no
                    {
                        // new search in DB
                        fundedProfessors = (from p in DB.Professors
                                            where p.Education_Degree.DegreeName == (string)cmbEducationDegree.SelectedItem
                                            select p).ToList();
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
                        fundedProfessors = (from p in fundedProfessors
                                            where p.Branch.Branch_Name == (string)cmbEducationBranch.SelectedItem
                                            select p).ToList();
                    }
                    else // no
                    {
                        // new search in DB
                        fundedProfessors = (from p in DB.Professors
                                            where p.Branch.Branch_Name == (string)cmbEducationBranch.SelectedItem
                                            select p).ToList();
                        combinedSearch = true;
                    }
                }
                #endregion
                //
                // set DataGridView DataSource:
                dgvProfessor.ItemsSource = (from p in fundedProfessors
                                            select new
                                            {
                                                p.ID,
                                                p.FirstName,
                                                p.LastName,
                                                p.Education_Degree.DegreeName,
                                                p.Email,
                                                p.PhoneNo,
                                                p.Branch.Branch_Name
                                            }).ToList();

                setColumnNames.Start();
            }
        }

        private void btnShowAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            txtFirstName.Text = "";
            txtLastName.Text = "";
            cmbEducationBranch.Text = "";
            cmbEducationDegree.Text = "";
            setProfessorTable();
        }
	}
}
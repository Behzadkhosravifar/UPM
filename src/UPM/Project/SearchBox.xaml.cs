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
	/// Interaction logic for SearchBox.xaml
	/// </summary>
	public partial class SearchBox : Window
	{
        LINQtoSQLDataContext DB;
        private DispatcherTimer setColumnNames; // for set columns header after 400 millisecond.

        ViewPrj ProjectViewForm; // Show/Hide Project View Forms
        bool ProjectViewFormClosed = true;

		public SearchBox()
		{
			this.InitializeComponent();

            //
            // Create Project View Form by any relations . . . 
            //
            ProjectViewForm = new ViewPrj();
            ProjectViewForm.tgbtnView = this.tgbtnProjectView;
            ProjectViewForm.Closed += new EventHandler(ProjectViewForm_Closed);

            setColumnNames = new DispatcherTimer();
            setColumnNames.Interval = new TimeSpan(400);
            setColumnNames.Tick += new EventHandler(setColumnNames_Tick);

            DB = new LINQtoSQLDataContext();
            setProjectTable();
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

        #region Show/Hide Project View Forms Code

        private void tgbtnProjectView_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ProjectViewFormClosed)
            {
                ProjectViewFormClosed = false;
                ProjectViewForm = new ViewPrj();
                ProjectViewForm.tgbtnView = this.tgbtnProjectView;
                ProjectViewForm.Closed += new EventHandler(ProjectViewForm_Closed);
            }
            ProjectViewForm.Show();
        }
        void ProjectViewForm_Closed(object sender, EventArgs e)
        {
            ProjectViewFormClosed = true;
        }
        private void tgbtnProjectView_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            ProjectViewForm.Hide();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ProjectViewFormClosed)
            {
                ProjectViewForm.Close();
            }
        } 
        #endregion

        void setColumnNames_Tick(object sender, EventArgs e)
        {
            if (dgvProjects.Columns.Count > 0)
            {
                dgvProjects.Columns[0].Header = "Project ID";
                dgvProjects.Columns[1].Header = "Project Title";
                dgvProjects.Columns[2].Header = "Start Date";
                dgvProjects.Columns[3].Header = "End Date";
                dgvProjects.Columns[4].Header = "Grade";
            }

            setColumnNames.Stop();
        }

        private void setProjectTable()
        {
            dgvProjects.ItemsSource = (from p in DB.Projects
                                        select new
                                        {
                                            p.ID,
                                            p.Project_Name,
                                            p.Start_Date,
                                            p.End_Date,
                                            Grade = p.Guide_Grade + p.Juror_Grade
                                        });

            setColumnNames.Start();
        }

        private void btnShowAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            txtProfessorID.Text = "";
            txtProfessorFirstName.Text = "";
            txtProfessorLastName.Text = "";
            chkGuideProfessor.IsChecked = false;
            chkJurorProfessor.IsChecked = false;
            chkManagerProfessor.IsChecked = false;
            chkProjectProfessor.IsChecked = false;

            txtStudentID.Text = "";
            txtStudentCode.Text = "";
            txtStudentFirstName.Text = "";
            txtStudentLastName.Text = "";

            txtProjectID.Text = "";
            txtProjectTitle.Text = "";
            txtProposalKeyword1.Text = "";
            txtProposalKeyword2.Text = "";
            txtProposalKeyword3.Text = "";
            chkIncludeStartDate.IsChecked = false;
            chkIncludeEndDate.IsChecked = false;

            Search();
            btnSearch.Content = "Close Result";
            dgvProjects.Visibility = System.Windows.Visibility.Visible;

        }

        private void btnSearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (btnSearch.Content.ToString() == "Search")
            {
                Search();
                btnSearch.Content = "Close Result";
                dgvProjects.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                btnSearch.Content = "Search";
                dgvProjects.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void Search()
        {
            // check empty textbox
            if (string.IsNullOrEmpty(txtProfessorFirstName.Text) &&
                string.IsNullOrEmpty(txtProfessorID.Text) &&
                string.IsNullOrEmpty(txtProfessorLastName.Text) &&
                string.IsNullOrEmpty(txtStudentID.Text) &&
                string.IsNullOrEmpty(txtStudentCode.Text) &&
                string.IsNullOrEmpty(txtStudentFirstName.Text) &&
                string.IsNullOrEmpty(txtStudentLastName.Text) &&
                string.IsNullOrEmpty(txtProjectID.Text) &&
                string.IsNullOrEmpty(txtProjectTitle.Text) &&
                string.IsNullOrEmpty(txtProposalKeyword1.Text) &&
                string.IsNullOrEmpty(txtProposalKeyword2.Text) &&
                string.IsNullOrEmpty(txtProposalKeyword3.Text) &&
                !(bool)chkIncludeStartDate.IsChecked &&
                !(bool)chkIncludeEndDate.IsChecked) // All are empty!
            {
                setProjectTable();
            }
            else // at least a field that is not empty!
            {
                bool combinedSearch = false;
                List<Project> fundedProjects = new List<Project>();
                //
                #region Search by Professor Info . . . +
                #region Project Professor
                if ((bool)chkProjectProfessor.IsChecked)
                {
                    #region find according by Professor ID
                    if (!string.IsNullOrEmpty(txtProfessorID.Text))
                    {
                        fundedProjects.appendToList((from p in DB.Projects
                                                     where p.Project_Professor_ID == int.Parse(txtProfessorID.Text)
                                                     select p).ToList());
                        combinedSearch = true;
                    }
                    #endregion
                    #region find according by Professor First Name
                    if (!string.IsNullOrEmpty(txtProfessorFirstName.Text))
                    {
                        //
                        // Whether was already find something? Combined Search
                        if (combinedSearch) // yes
                        {
                            //
                            // search in funded projects ...
                            fundedProjects = (from p in fundedProjects
                                              where p.ProjectProfessor.FirstName.Contains(txtProfessorFirstName.Text)
                                              select p).ToList();
                        }
                        else // no
                        {
                            // new search in DB
                            fundedProjects.appendToList((from p in DB.Projects
                                                         where p.ProjectProfessor.FirstName.Contains(txtProfessorFirstName.Text)
                                                         select p).ToList());
                            combinedSearch = true;
                        }
                    }
                    #endregion
                    #region find according by Professor Last Name
                    if (!string.IsNullOrEmpty(txtProfessorLastName.Text))
                    {
                        //
                        // Whether was already find something? Combined Search
                        if (combinedSearch) // yes
                        {
                            //
                            // search in funded projects ...
                            fundedProjects = (from p in fundedProjects
                                              where p.ProjectProfessor.LastName.Contains(txtProfessorLastName.Text)
                                              select p).ToList();
                        }
                        else // no
                        {
                            // new search in DB
                            fundedProjects.appendToList((from p in DB.Projects
                                                         where p.ProjectProfessor.LastName.Contains(txtProfessorLastName.Text)
                                                         select p).ToList());
                            combinedSearch = true;
                        }
                    }
                    #endregion
                }
                #endregion
                //
                combinedSearch = false;
                //
                #region Guide Professor
                if ((bool)chkGuideProfessor.IsChecked)
                {
                    #region find according by Professor ID
                    if (!string.IsNullOrEmpty(txtProfessorID.Text))
                    {
                        fundedProjects.appendToList((from p in DB.Projects
                                                     where p.Guide_Professor_ID == int.Parse(txtProfessorID.Text)
                                                     select p).ToList());
                        combinedSearch = true;
                    }
                    #endregion
                    #region find according by Professor First Name
                    if (!string.IsNullOrEmpty(txtProfessorFirstName.Text))
                    {
                        //
                        // Whether was already find something? Combined Search
                        if (combinedSearch) // yes
                        {
                            //
                            // search in funded projects ...
                            fundedProjects = (from p in fundedProjects
                                              where p.GuideProfessor.FirstName.Contains(txtProfessorFirstName.Text)
                                              select p).ToList();
                        }
                        else // no
                        {
                            // new search in DB
                            fundedProjects.appendToList((from p in DB.Projects
                                                         where p.GuideProfessor.FirstName.Contains(txtProfessorFirstName.Text)
                                                         select p).ToList());
                            combinedSearch = true;
                        }
                    }
                    #endregion
                    #region find according by Professor Last Name
                    if (!string.IsNullOrEmpty(txtProfessorLastName.Text))
                    {
                        //
                        // Whether was already find something? Combined Search
                        if (combinedSearch) // yes
                        {
                            //
                            // search in funded projects ...
                            fundedProjects = (from p in fundedProjects
                                              where p.GuideProfessor.LastName.Contains(txtProfessorLastName.Text)
                                              select p).ToList();
                        }
                        else // no
                        {
                            // new search in DB
                            fundedProjects.appendToList((from p in DB.Projects
                                                         where p.GuideProfessor.LastName.Contains(txtProfessorLastName.Text)
                                                         select p).ToList());
                            combinedSearch = true;
                        }
                    }
                    #endregion
                }
                #endregion
                //
                combinedSearch = false;
                //
                #region Manager Professor
                if ((bool)chkManagerProfessor.IsChecked)
                {
                    #region find according by Professor ID
                    if (!string.IsNullOrEmpty(txtProfessorID.Text))
                    {
                        fundedProjects.appendToList((from p in DB.Projects
                                                     where p.Manager_Professor_ID == int.Parse(txtProfessorID.Text)
                                                     select p).ToList());
                        combinedSearch = true;
                    }
                    #endregion
                    #region find according by Professor First Name
                    if (!string.IsNullOrEmpty(txtProfessorFirstName.Text))
                    {
                        //
                        // Whether was already find something? Combined Search
                        if (combinedSearch) // yes
                        {
                            //
                            // search in funded projects ...
                            fundedProjects = (from p in fundedProjects
                                              where p.ManagerProfessor.FirstName.Contains(txtProfessorFirstName.Text)
                                              select p).ToList();
                        }
                        else // no
                        {
                            // new search in DB
                            fundedProjects.appendToList((from p in DB.Projects
                                                         where p.ManagerProfessor.FirstName.Contains(txtProfessorFirstName.Text)
                                                         select p).ToList());
                            combinedSearch = true;
                        }
                    }
                    #endregion
                    #region find according by Professor Last Name
                    if (!string.IsNullOrEmpty(txtProfessorLastName.Text))
                    {
                        //
                        // Whether was already find something? Combined Search
                        if (combinedSearch) // yes
                        {
                            //
                            // search in funded projects ...
                            fundedProjects = (from p in fundedProjects
                                              where p.ManagerProfessor.LastName.Contains(txtProfessorLastName.Text)
                                              select p).ToList();
                        }
                        else // no
                        {
                            // new search in DB
                            fundedProjects.appendToList((from p in DB.Projects
                                                         where p.ManagerProfessor.LastName.Contains(txtProfessorLastName.Text)
                                                         select p).ToList());
                            combinedSearch = true;
                        }
                    }
                    #endregion
                }
                #endregion
                //
                combinedSearch = false;
                //
                #region Juror Professor
                if ((bool)chkJurorProfessor.IsChecked)
                {
                    #region find according by Professor ID
                    if (!string.IsNullOrEmpty(txtProfessorID.Text))
                    {
                        fundedProjects.appendToList((from p in DB.Projects
                                                     where p.Juror_Professor_ID == int.Parse(txtProfessorID.Text)
                                                     select p).ToList());
                        combinedSearch = true;
                    }
                    #endregion
                    #region find according by Professor First Name
                    if (!string.IsNullOrEmpty(txtProfessorFirstName.Text))
                    {
                        //
                        // Whether was already find something? Combined Search
                        if (combinedSearch) // yes
                        {
                            //
                            // search in funded projects ...
                            fundedProjects = (from p in fundedProjects
                                              where p.JurorProfessor.FirstName.Contains(txtProfessorFirstName.Text)
                                              select p).ToList();
                        }
                        else // no
                        {
                            // new search in DB
                            fundedProjects.appendToList((from p in DB.Projects
                                                         where p.JurorProfessor.FirstName.Contains(txtProfessorFirstName.Text)
                                                         select p).ToList());
                            combinedSearch = true;
                        }
                    }
                    #endregion
                    #region find according by Professor Last Name
                    if (!string.IsNullOrEmpty(txtProfessorLastName.Text))
                    {
                        //
                        // Whether was already find something? Combined Search
                        if (combinedSearch) // yes
                        {
                            //
                            // search in funded projects ...
                            fundedProjects = (from p in fundedProjects
                                              where p.JurorProfessor.LastName.Contains(txtProfessorLastName.Text)
                                              select p).ToList();
                        }
                        else // no
                        {
                            // new search in DB
                            fundedProjects.appendToList((from p in DB.Projects
                                                         where p.JurorProfessor.LastName.Contains(txtProfessorLastName.Text)
                                                         select p).ToList());
                            combinedSearch = true;
                        }
                    }
                    #endregion
                }
                #endregion
                //
                combinedSearch = false;
                #endregion
                //
                #region Search by Student Info . . . +
                #region find according by Student ID
                if (!string.IsNullOrEmpty(txtStudentID.Text))
                {
                    fundedProjects.appendToList((from psr in DB.Project_Student_Relations
                                                 where psr.Student_ID == int.Parse(txtStudentID.Text)
                                                 select psr.Project).ToList());
                    combinedSearch = true;
                }
                #endregion
                #region find according by Student Code
                if (!string.IsNullOrEmpty(txtStudentCode.Text))
                {
                    fundedProjects.appendToList((from psr in DB.Project_Student_Relations
                                                 where psr.Student.Student_Code == int.Parse(txtStudentCode.Text)
                                                 select psr.Project).ToList());
                }
                #endregion
                #region find according by Student First Name
                if (!string.IsNullOrEmpty(txtStudentFirstName.Text))
                {
                    //
                    // Whether was already find something? Combined Search
                    if (combinedSearch) // yes
                    {
                        //
                        // search in funded projects ...
                        fundedProjects = (from p in fundedProjects
                                          from psr in p.Project_Student_Relations
                                          where psr.Student.FirstName.Contains(txtStudentFirstName.Text)
                                          select psr.Project).ToList();                                          
                    }
                    else // no
                    {
                        // new search in DB
                        fundedProjects.appendToList((from psr in DB.Project_Student_Relations
                                                     where psr.Student.FirstName.Contains(txtStudentFirstName.Text)
                                                     select psr.Project).ToList());
                        combinedSearch = true;
                    }
                }
                #endregion
                #region find according by Student Last Name
                if (!string.IsNullOrEmpty(txtStudentLastName.Text))
                {
                    //
                    // Whether was already find something? Combined Search
                    if (combinedSearch) // yes
                    {
                        //
                        // search in funded projects ...
                        fundedProjects = (from p in fundedProjects
                                          from psr in p.Project_Student_Relations
                                          where psr.Student.LastName.Contains(txtStudentLastName.Text)
                                          select psr.Project).ToList();
                    }
                    else // no
                    {
                        // new search in DB
                        fundedProjects.appendToList((from psr in DB.Project_Student_Relations
                                                     where psr.Student.LastName.Contains(txtStudentLastName.Text)
                                                     select psr.Project).ToList());
                        combinedSearch = true;
                    }
                }
                #endregion
                //
                combinedSearch = false;
                #endregion
                //
                #region Search by Project Info . . . 
                #region find according by Project ID
                if (!string.IsNullOrEmpty(txtProjectID.Text))
                {
                    fundedProjects.appendToList((from p in DB.Projects
                                                 where p.ID == int.Parse(txtProjectID.Text)
                                                 select p).ToList());
                    combinedSearch = true;
                }
                #endregion
                #region find according by Project Title
                if (!string.IsNullOrEmpty(txtProjectTitle.Text))
                {
                    //
                    // Whether was already find something? Combined Search
                    if (combinedSearch) // yes
                    {
                        //
                        // search in funded projects ...
                        fundedProjects = (from p in fundedProjects
                                          where p.Project_Name.Contains(txtProjectTitle.Text)
                                          select p).ToList();
                    }
                    else // no
                    {
                        // new search in DB
                        fundedProjects.appendToList((from p in DB.Projects
                                                     where p.Project_Name.Contains(txtProjectTitle.Text)
                                                     select p).ToList());
                        combinedSearch = true;
                    }
                }
                #endregion
                #region find according by Project Proposal Keywords 1
                if (!string.IsNullOrEmpty(txtProposalKeyword1.Text))
                {
                    //
                    // Whether was already find something? Combined Search
                    if (combinedSearch) // yes
                    {
                        //
                        // search in funded projects ...
                        fundedProjects = (from p in fundedProjects
                                          where p.ProposalText.Contains(txtProposalKeyword1.Text)
                                          select p).ToList();
                    }
                    else // no
                    {
                        // new search in DB
                        fundedProjects.appendToList((from p in DB.Projects
                                                     where p.ProposalText.Contains(txtProposalKeyword1.Text)
                                                     select p).ToList());
                        combinedSearch = true;
                    }
                }
                #endregion
                #region find according by Project Proposal Keywords 2
                if (!string.IsNullOrEmpty(txtProposalKeyword2.Text))
                {
                    //
                    // Whether was already find something? Combined Search
                    if (combinedSearch) // yes
                    {
                        //
                        // search in funded projects ...
                        fundedProjects = (from p in fundedProjects
                                          where p.ProposalText.Contains(txtProposalKeyword2.Text)
                                          select p).ToList();
                    }
                    else // no
                    {
                        // new search in DB
                        fundedProjects.appendToList((from p in DB.Projects
                                                     where p.ProposalText.Contains(txtProposalKeyword2.Text)
                                                     select p).ToList());
                        combinedSearch = true;
                    }
                }
                #endregion
                #region find according by Project Proposal Keywords 3
                if (!string.IsNullOrEmpty(txtProposalKeyword3.Text))
                {
                    //
                    // Whether was already find something? Combined Search
                    if (combinedSearch) // yes
                    {
                        //
                        // search in funded projects ...
                        fundedProjects = (from p in fundedProjects
                                          where p.ProposalText.Contains(txtProposalKeyword3.Text)
                                          select p).ToList();
                    }
                    else // no
                    {
                        // new search in DB
                        fundedProjects.appendToList((from p in DB.Projects
                                                     where p.ProposalText.Contains(txtProposalKeyword3.Text)
                                                     select p).ToList());
                        combinedSearch = true;
                    }
                }
                #endregion
                #region find according by Project Start Date Ranges
                if ((bool)chkIncludeStartDate.IsChecked)
                {
                    Arash.PersianDate startDateFrom = Arash.PersianDate.Parse(dpStartFrom.Text);
                    Arash.PersianDate startDateTo = Arash.PersianDate.Parse(dpStartTo.Text);
                    List<Project> lstBuffer = new List<Project>();
                    //
                    // Whether was already find something? Combined Search
                    if (combinedSearch) // yes
                    {
                        //
                        // search in funded projects ...
                        foreach (var p in fundedProjects)
                        {
                            if (Arash.PersianDate.Parse(p.Start_Date).CompareTo(startDateFrom) >= 0 &&
                                Arash.PersianDate.Parse(p.Start_Date).CompareTo(startDateTo) <= 0) 
                            {
                                lstBuffer.Add(p);
                            }
                        }
                        fundedProjects = lstBuffer;
                    }
                    else // no
                    {
                        // new search in DB
                        foreach (var p in DB.Projects)
                        {
                            if (Arash.PersianDate.Parse(p.Start_Date).CompareTo(startDateFrom) >= 0 &&
                                Arash.PersianDate.Parse(p.Start_Date).CompareTo(startDateTo) <= 0) 
                            {
                                lstBuffer.Add(p);
                            }
                        }
                        fundedProjects.appendToList(lstBuffer);
                        combinedSearch = true;
                    }
                }
                #endregion
                #region find according by Project End Date Ranges
                if ((bool)chkIncludeEndDate.IsChecked)
                {
                    Arash.PersianDate endDateFrom = Arash.PersianDate.Parse(dpEndFrom.Text);
                    Arash.PersianDate endDateTo = Arash.PersianDate.Parse(dpEndTo.Text);
                    List<Project> lstBuffer = new List<Project>();
                    //
                    // Whether was already find something? Combined Search
                    if (combinedSearch) // yes
                    {
                        //
                        // search in funded projects ...
                        foreach (var p in fundedProjects)
                        {
                            if (!string.IsNullOrEmpty(p.End_Date))
                            {
                                if (Arash.PersianDate.Parse(p.End_Date).CompareTo(endDateFrom) >= 0 &&
                                    Arash.PersianDate.Parse(p.End_Date).CompareTo(endDateTo) <= 0)
                                {
                                    lstBuffer.Add(p);
                                }
                            }
                            else lstBuffer.Add(p);
                        }
                        fundedProjects = lstBuffer;
                    }
                    else // no
                    {
                        // new search in DB
                        foreach (var p in DB.Projects)
                        {
                            if (!string.IsNullOrEmpty(p.End_Date))
                            {
                                if (Arash.PersianDate.Parse(p.End_Date).CompareTo(endDateFrom) >= 0 &&
                                    Arash.PersianDate.Parse(p.End_Date).CompareTo(endDateTo) <= 0)
                                {
                                    lstBuffer.Add(p);
                                }
                            }
                            else lstBuffer.Add(p);
                        }
                        fundedProjects.appendToList(lstBuffer);
                        combinedSearch = true;
                    }
                }
                #endregion
                #endregion
                //
                // End of work ...
                //
                #region Set DataGridView DataSource
                dgvProjects.ItemsSource = (from p in fundedProjects
                                           select new
                                           {
                                               p.ID,
                                               p.Project_Name,
                                               p.Start_Date,
                                               p.End_Date,
                                               Grade = p.Guide_Grade + p.Juror_Grade
                                           }).ToList();

                setColumnNames.Start();
                #endregion
            }
        }

        int LastID = -1;
        private void dgvProjects_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgvProjects.SelectedIndex >= 0)
            {
                //
                // First find record id in DB
                string id = "";
                string row = dgvProjects.SelectedItem.ToString();
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
                    var selectedProject = (from p in DB.Projects
                                           where p.ID == LastID
                                           select p).Single();
                    //
                    // fill the all textBox by selected data
                    if ((bool)tgbtnProjectView.IsChecked) // Then Project View Form is Opened!!!
                    {
                        ProjectViewForm.ThisProject = selectedProject;
                    }
                }
            }
        }
	}

    public static class AppendList
    {
        public static void appendToList(this System.Collections.Generic.List<Project> T1, System.Collections.Generic.List<Project> T2)
        {
            if (T1.GetType() == T2.GetType())
            {
                foreach (var t in T2)
                {
                    if (!T1.Contains(t)) // if T2 items is not exist in T1 list's.
                    {
                        T1.Add(t);
                    }
                }
            }
        }
    }
}
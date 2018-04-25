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
	/// Interaction logic for ViewPrj.xaml
	/// </summary>
	public partial class ViewPrj : Window
	{
        public System.Windows.Controls.Primitives.ToggleButton tgbtnView;  

        Student stu1; // 1st project student information
        Student stu2; // 2th project student information
        Student stu3; // 3th project student information

        Project thisProject; // save founded project

        public Project ThisProject
        {
            set 
            { 
                thisProject = value;
                clearAllFields();
                stu1 = thisProject.Project_Student_Relations[0].Student;
                stu2 = thisProject.Project_Student_Relations.Count >= 2 ? thisProject.Project_Student_Relations[1].Student : null;
                stu3 = thisProject.Project_Student_Relations.Count >= 3 ? thisProject.Project_Student_Relations[2].Student : null;
                RefreshPage();
            }
        }

        private void RefreshPage()
        {
            //
            // Project Info
            //
            txtProjectTitle.Text = thisProject.Project_Name;
            txtProjectProposal.Text = thisProject.ProposalText;
            txtProjectCODE.Text = thisProject.ID.ToString();
            txtGuideProfessorGrade.Text = thisProject.Guide_Grade.HasValue ? 
                thisProject.Guide_Grade.Value.ToString() : "0";
            txtJurorProfessorGrade.Text = thisProject.Juror_Grade.HasValue ? 
                thisProject.Juror_Grade.Value.ToString() : "0";
            dpStudentStartDate.Text = thisProject.Start_Date;
            dpProjectEndDate.Text = thisProject.End_Date;
            //
            // Professor Info
            //
            setProfessorsNote();
            //
            // Student Info
            //
            if (stu1 != null)
            {
                //
                // Set Selected Student Row
                //
                txtStudentCode1.Text = stu1.Student_Code.ToString();
                txtStudentEntry1.Text = string.Format("{0} - {1}", stu1.Entry_Year, (stu1.Entry_Term) ? "مهر" : "بهمن");
                txtStudentFLName1.Text = stu1.FirstName + " " + stu1.LastName;
                txtStudentPhoneNo1.Text = stu1.Phone_No;
            }

            if (stu2 != null)
            {
                //
                // Set Selected Student Row
                //
                txtStudentCode2.Text = stu2.Student_Code.ToString();
                txtStudentEntry2.Text = string.Format("{0} - {1}", stu2.Entry_Year, (stu2.Entry_Term) ? "مهر" : "بهمن");
                txtStudentFLName2.Text = stu2.FirstName + " " + stu2.LastName;
                txtStudentPhoneNo2.Text = stu2.Phone_No;
            }

            if (stu3 != null)
            {
                //
                // Set Selected Student Row
                //
                txtStudentCode3.Text = stu3.Student_Code.ToString();
                txtStudentEntry3.Text = string.Format("{0} - {1}", stu3.Entry_Year, (stu3.Entry_Term) ? "مهر" : "بهمن");
                txtStudentFLName3.Text = stu3.FirstName + " " + stu3.LastName;
                txtStudentPhoneNo3.Text = stu3.Phone_No;
            }
        }

		public ViewPrj()
		{
			this.InitializeComponent();

            txtProjectProposal.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
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

        private void txtJurorProfessorGrade_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            double num = 0;
            //
            // calculate sum of Grades
            num = string.IsNullOrEmpty(((TextBox)sender).Text) ? 0 : double.Parse(((TextBox)sender).Text);
            double num2 = (txtGuideProfessorGrade == null) ? 0 :
                (string.IsNullOrEmpty(txtGuideProfessorGrade.Text) ? 0 :
                double.Parse(txtGuideProfessorGrade.Text));
            txtAllGrade.Text = "نمره کل پروژه" + ": " + (num + num2).ToString();
        }

        private void txtGuideProfessorGrade_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            double num = 0;
            //
            // calculate sum of Grades
            num = string.IsNullOrEmpty(((TextBox)sender).Text) ? 0 : double.Parse(((TextBox)sender).Text);
            double num2 = (txtJurorProfessorGrade == null) ? 0 :
                (string.IsNullOrEmpty(txtJurorProfessorGrade.Text) ? 0 :
                double.Parse(txtJurorProfessorGrade.Text));
            txtAllGrade.Text = "نمره کل پروژه" + ": " + (num + num2).ToString();
        }

        private void setProfessorsNote()
        {
            string projectProfessor = (thisProject.ProjectProfessor == null) ? "" :
               string.Format("استاد پروژه =  نام: {0}    -    نام خانوادگی: {1}    -    تلفن: {2} ا",
                    thisProject.ProjectProfessor.FirstName, thisProject.ProjectProfessor.LastName, thisProject.ProjectProfessor.PhoneNo);
            projectProfessor = string.IsNullOrEmpty(projectProfessor) ? "" : projectProfessor.Substring(0, projectProfessor.Length - 1);

            string managerProfessor = (thisProject.ManagerProfessor == null) ? "" :
               string.Format("مدیر گروه =  نام: {0}    -    نام خانوادگی: {1}    -    تلفن: {2} ا",
                    thisProject.ManagerProfessor.FirstName, thisProject.ManagerProfessor.LastName, thisProject.ManagerProfessor.PhoneNo);
            managerProfessor = string.IsNullOrEmpty(managerProfessor) ? "" : managerProfessor.Substring(0, managerProfessor.Length - 1);

            string guideProfessor = (thisProject.GuideProfessor == null) ? "" :
               string.Format("استاد راهنما =  نام: {0}    -    نام خانوادگی: {1}    -    تلفن: {2} ا",
                    thisProject.GuideProfessor.FirstName, thisProject.GuideProfessor.LastName, thisProject.GuideProfessor.PhoneNo);
            guideProfessor = string.IsNullOrEmpty(guideProfessor) ? "" : guideProfessor.Substring(0, guideProfessor.Length - 1);

            string jurorProfessor = (thisProject.JurorProfessor == null) ? "" :
               string.Format("استاد داور =  نام: {0}    -    نام خانوادگی: {1}    -    تلفن: {2} ا",
                    thisProject.JurorProfessor.FirstName, thisProject.JurorProfessor.LastName, thisProject.JurorProfessor.PhoneNo);
            jurorProfessor = string.IsNullOrEmpty(jurorProfessor) ? "" : jurorProfessor.Substring(0, jurorProfessor.Length - 1);

            txtProfessors.Text = string.Format("\n{0}\n{1}\n{2}\n{3}",
                projectProfessor, managerProfessor, guideProfessor, jurorProfessor);
        }

        private void clearAllFields()
        {
            txtProjectTitle.Text = "";
            txtProjectProposal.Text = "";
            txtGuideProfessorGrade.Text = "0";
            txtJurorProfessorGrade.Text = "0";
            dpStudentStartDate.Text = "";
            dpProjectEndDate.Text = "";

            txtStudentCode1.Text = "";
            txtStudentEntry1.Text = "";
            txtStudentFLName1.Text = "";
            txtStudentPhoneNo1.Text = "";
            txtStudentCode2.Text = "";
            txtStudentEntry2.Text = "";
            txtStudentFLName2.Text = "";
            txtStudentPhoneNo2.Text = "";
            txtStudentCode3.Text = "";
            txtStudentEntry3.Text = "";
            txtStudentFLName3.Text = "";
            txtStudentPhoneNo3.Text = "";
            txtProfessors.Text = "";

            stu1 = null;
            stu2 = null;
            stu3 = null;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            tgbtnView.IsChecked = false;
        }
	}
}
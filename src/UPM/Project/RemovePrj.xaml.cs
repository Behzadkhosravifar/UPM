﻿using System;
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
	/// Interaction logic for RemovePrj.xaml
	/// </summary>
	public partial class RemovePrj : Window
	{
        LINQtoSQLDataContext DB;
        Student stu1; // 1st project student information
        Student stu2; // 2th project student information
        Student stu3; // 3th project student information
        Professor profJuror; // Juror Professor information
        Professor profGuide; // Guide Professor information
        Professor profManager; // Manager Professor information
        Professor profProject; // Project Professor information

        Project thisProject; // save founded project

		public RemovePrj()
		{
			this.InitializeComponent();

            DB = new LINQtoSQLDataContext();

            txtProjectProposal.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            txtJurorProfessorGrade.Text = "5";
            txtGuideProfessorGrade.Text = "15";

            dpProjectEndDate.SelectedDate = new Arash.PersianDate(DateTime.Today);
            dpStudentStartDate.SelectedDate = new Arash.PersianDate(DateTime.Today);
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
            string projectProfessor = (profProject == null) ? "" :
               string.Format("استاد پروژه =  نام: {0}    -    نام خانوادگی: {1}    -    تلفن: {2} ا",
                    profProject.FirstName, profProject.LastName, profProject.PhoneNo);
            projectProfessor = string.IsNullOrEmpty(projectProfessor) ? "" : projectProfessor.Substring(0, projectProfessor.Length - 1);

            string managerProfessor = (profManager == null) ? "" :
               string.Format("مدیر گروه =  نام: {0}    -    نام خانوادگی: {1}    -    تلفن: {2} ا",
                    profManager.FirstName, profManager.LastName, profManager.PhoneNo);
            managerProfessor = string.IsNullOrEmpty(managerProfessor) ? "" : managerProfessor.Substring(0, managerProfessor.Length - 1);

            string guideProfessor = (profGuide == null) ? "" :
               string.Format("استاد راهنما =  نام: {0}    -    نام خانوادگی: {1}    -    تلفن: {2} ا",
                    profGuide.FirstName, profGuide.LastName, profGuide.PhoneNo);
            guideProfessor = string.IsNullOrEmpty(guideProfessor) ? "" : guideProfessor.Substring(0, guideProfessor.Length - 1);

            string jurorProfessor = (profJuror == null) ? "" :
               string.Format("استاد داور =  نام: {0}    -    نام خانوادگی: {1}    -    تلفن: {2} ا",
                    profJuror.FirstName, profJuror.LastName, profJuror.PhoneNo);
            jurorProfessor = string.IsNullOrEmpty(jurorProfessor) ? "" : jurorProfessor.Substring(0, jurorProfessor.Length - 1);

            txtProfessors.Text = string.Format("\n{0}\n{1}\n{2}\n{3}",
                projectProfessor, managerProfessor, guideProfessor, jurorProfessor);
        }

        private void btnRemoveProject_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                //
                // Remove project from DB
                //
                // first find this project 
                if (thisProject != null // before funded!!!
                        && MessageBox.Show("Are you sure to delete this project from database" +
                        "\r\nby any relation with another data tables?",
                        "Warning to lost data!", MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes) 
                {
                    //
                    // remove any relation by this project and any student!
                    foreach (var anyRelate in (from p_s_r in DB.Project_Student_Relations
                                               where p_s_r.Project_ID == thisProject.ID
                                               select p_s_r).ToList())
                    {
                        DB.Project_Student_Relations.DeleteOnSubmit(anyRelate);
                    }
                    DB.SubmitChanges();
                    //
                    // Now delete this project self data . . . 
                    DB.Projects.DeleteOnSubmit(thisProject);
                    DB.SubmitChanges();                    
                    //
                    // Remove Completed!
                    MessageBox.Show("Project Successfully Removed.", "Successfully Completed!",
                         MessageBoxButton.OK, MessageBoxImage.Information);

                    clearAllFields();
                    txtProjectCODE.Text = "کد پروژه";
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, ex.Source); }
        }

        #region Project ID textbox codes
        private void txtProjectCODE_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            uint num = 0;
            if (!string.IsNullOrEmpty(((TextBox)sender).Text))
            {
                bool success = uint.TryParse(((TextBox)sender).Text, out num);
                if (success && num >= 0)
                {
                    ((TextBox)sender).Text.Trim();

                    BrushConverter bc = new BrushConverter();
                    Brush brush;
                    brush = (Brush)bc.ConvertFrom("#FF606060");
                    ((TextBox)sender).Foreground = brush;

                    findProjectByID(num);
                }
                else if (((TextBox)sender).Text == "کد پروژه")
                {
                    ((TextBox)sender).Foreground = new SolidColorBrush(Color.FromArgb(100, 40, 180, 25));
                    clearAllFields();
                }
                else
                {
                    try
                    {
                        ((TextBox)sender).Undo();
                    }
                    catch { ((TextBox)sender).Text = ""; }
                    ((TextBox)sender).SelectionStart = ((TextBox)sender).Text.Length;
                }
            }
        }

        private void txtProjectCODE_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (((TextBox)sender).Text == "کد پروژه")
                ((TextBox)sender).Text = "";
        }

        private void txtProjectCODE_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(((TextBox)sender).Text))
                ((TextBox)sender).Text = "کد پروژه";
        }

        private void txtProjectCODE_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (((TextBox)sender).Text == "کد پروژه")
                ((TextBox)sender).Text = "";
        }

        private void txtProjectCODE_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(((TextBox)sender).Text))
                ((TextBox)sender).Text = "کد پروژه";
        }
        #endregion

        private void findProjectByID(uint num)
        {
            clearAllFields();
            try
            {
                //
                // Search project by entry Code
                // 
                thisProject = (from p in DB.Projects
                               where p.ID == num
                               select p).Single(); // if not found then throw exception and go to catch { ... }

                // fill project information fields and clear other fields
                txtProjectTitle.Text = thisProject.Project_Name;
                txtProjectProposal.Text = thisProject.ProposalText;
                txtGuideProfessorGrade.Text = (thisProject.Guide_Grade.HasValue) ? thisProject.Guide_Grade.Value.ToString() : "0";
                txtJurorProfessorGrade.Text = (thisProject.Juror_Grade.HasValue) ? thisProject.Juror_Grade.Value.ToString() : "0";
                dpStudentStartDate.Text = thisProject.Start_Date;
                dpProjectEndDate.Text = thisProject.End_Date;
                // set project professors ----------------------------------
                profGuide = (from prof in DB.Professors
                             where prof.ID == thisProject.Guide_Professor_ID
                             select prof).SingleOrDefault();

                profProject = (from prof in DB.Professors
                               where prof.ID == thisProject.Project_Professor_ID
                               select prof).SingleOrDefault();

                profManager = (from prof in DB.Professors
                               where prof.ID == thisProject.Manager_Professor_ID
                               select prof).SingleOrDefault();

                profJuror = (thisProject.Juror_Professor_ID.HasValue) ?
                                (from p in DB.Professors
                                 where p.ID == thisProject.Juror_Professor_ID
                                 select p).SingleOrDefault() : null;

                setProfessorsNote();
                // -------------------------------------------------------------
                //
                // set project students ++++++++++++++++++++++++++++++++++++++++++
                Student[] students = (from ps in DB.Project_Student_Relations
                                      where ps.Project_ID == num
                                      select ps.Student).ToArray();

                stu1 = students[0];
                stu2 = (students.Length > 1) ? students[1] : null;
                stu3 = (students.Length > 2) ? students[2] : null;

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

                btnRemoveProject.IsEnabled = true;
            }
            catch
            {
                clearAllFields();
            }
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
            profGuide = null;
            profJuror = null;
            profManager = null;
            profProject = null;

            thisProject = null;
            btnRemoveProject.IsEnabled = false;
        }
	}
}
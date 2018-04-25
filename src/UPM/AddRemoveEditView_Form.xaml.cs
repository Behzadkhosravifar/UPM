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

namespace UPM
{
	/// <summary>
	/// Interaction logic for AddRemoveEditView_Form.xaml
	/// </summary>
	public partial class AddRemoveEditView_Form : Window
	{
		public AddRemoveEditView_Form(double top, double left, double width, double height, string title)
		{
			this.InitializeComponent();
			
			// Set this window in center and below of main window
            this.Window.Top = top + (height - this.Height - 100);
            this.Window.Left = left + ((width - this.Width) / 2) - 10;
            //
            // set windows title
            txtTitle.Text = title;
		}

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            //
            // drag window
            try { this.DragMove(); }
            catch { }
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	this.Close();
        }

        private void btnAdd_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            switch (txtTitle.Text)
            {
                case "Professor":
                    {
                        var newForm = new AddProf();
                        this.Hide();
                        newForm.ShowDialog();
                        this.ShowDialog();
                    }
                    break;
                case "Student":
                    {
                        var newForm = new AddStudent();
                        this.Hide();
                        newForm.ShowDialog();
                        this.ShowDialog();
                    }
                    break;
                case "Project":
                    {
                        var newForm = new AddPrj();
                        this.Hide();
                        newForm.ShowDialog();
                        this.ShowDialog();
                    }
                    break;
			}
        }

        private void btnRemove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            switch (txtTitle.Text)
            {
                case "Professor":
                    {
                        var newForm = new RemoveProf();
                        this.Hide();
                        newForm.ShowDialog();
                        this.ShowDialog();
                    }
                    break;
                case "Student":
                    {
                        var newForm = new RemoveStudent();
                        this.Hide();
                        newForm.ShowDialog();
                        this.ShowDialog();
                    }
                    break;
                case "Project":
                    {
                        var newForm = new RemovePrj();
                        this.Hide();
                        newForm.ShowDialog();
                        this.ShowDialog();
                    }
                    break;
            }
        }

        private void btnEdit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            switch (txtTitle.Text)
            {
                case "Professor":
                    {
                        var newForm = new EditProf();
                        this.Hide();
                        newForm.ShowDialog();
                        this.ShowDialog();
                    }
                    break;
                case "Student":
                    {
                        var newForm = new EditStudent();
                        this.Hide();
                        newForm.ShowDialog();
                        this.ShowDialog();
                    }
                    break;
                case "Project":
                    {
                        var newForm = new EditPrj();
                        this.Hide();
                        newForm.ShowDialog();
                        this.ShowDialog();
                    }
                    break;
			}
        }

        private void btnView_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            switch (txtTitle.Text)
            {
                case "Professor":
                    {
                        var newForm = new ViewProf();
                        this.Hide();
                        newForm.ShowDialog();
                        this.ShowDialog();
                    }
                    break;
                case "Student":
                    {
                        var newForm = new ViewStudent();
                        this.Hide();
                        newForm.ShowDialog();
                        this.ShowDialog();
                    }
                    break;
                case "Project":
                    {
                        var newForm = new SearchBox(); //---> ViewPrj();
                        this.Hide();
                        newForm.ShowDialog();
                        this.ShowDialog();
                    }
                    break;
			}
        }
	}
}

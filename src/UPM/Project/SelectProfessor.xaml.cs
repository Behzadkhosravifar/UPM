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
	/// Interaction logic for SelectProfessor.xaml
	/// </summary>
	public partial class SelectProfessor : Window
	{
        LINQtoSQLDataContext DB;
        private DispatcherTimer setColumnNames; // for set columns header after 400 millisecond.
        private Professor selectedProfessor;
        public Professor SelectedProfessor
        {
            get { return selectedProfessor; }
        }

		public SelectProfessor()
		{
			this.InitializeComponent();

            setColumnNames = new DispatcherTimer();
            setColumnNames.Interval = new TimeSpan(400);
            setColumnNames.Tick += new EventHandler(setColumnNames_Tick);

            DB = new LINQtoSQLDataContext();
            setProfessorTable();
		}

        void setColumnNames_Tick(object sender, EventArgs e)
        {
            if (dgvProfessor.Columns.Count > 0)
            {
                #region Create Columns Header
                dgvProfessor.Columns[0].Header = "ID";
                dgvProfessor.Columns[1].Header = "First Name";
                dgvProfessor.Columns[2].Header = "Last Name";
                dgvProfessor.Columns[3].Header = "Phone No.";
                #endregion
            }

            setColumnNames.Stop();
        }

        private void setProfessorTable()
        {
            dgvProfessor.ItemsSource = (from s in DB.Professors
                                      select new
                                      {
                                          s.ID,
                                          s.FirstName,
                                          s.LastName,
                                          s.PhoneNo
                                      });

            setColumnNames.Start();
        }

        #region textbox highlighting code
        List<Professor> fundedProfessors = new List<Professor>();
        private void txtSearchProfessorFName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (DB == null) return;
            //
            // Search Professor by Professor First name
            fundedProfessors = DB.Professors.ToList();

            if (!string.IsNullOrEmpty(txtSearchProfessorFName.Text) && txtSearchProfessorFName.Text != "نام استاد")
            {
                fundedProfessors = (from s in fundedProfessors
                                    where s.FirstName.Contains(txtSearchProfessorFName.Text)
                                    select s).ToList();
            }

            dgvProfessor.ItemsSource = (from s in fundedProfessors
                                        select new
                                        {
                                            s.ID,
                                            s.FirstName,
                                            s.LastName,
                                            s.PhoneNo
                                        }).ToList();

            setColumnNames.Start();
        }

        private void txtSearchProfessorLName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (DB == null) return;
            //
            // Search Professor by Professor Last Name
            fundedProfessors = DB.Professors.ToList();

            if (!string.IsNullOrEmpty(txtSearchProfessorLName.Text) && txtSearchProfessorLName.Text != "نام خانوادگی استاد")
            {
                fundedProfessors = (from s in fundedProfessors
                                  where s.LastName.Contains(txtSearchProfessorLName.Text)
                                  select s).ToList();
            }

            dgvProfessor.ItemsSource = (from s in fundedProfessors
                                        select new
                                        {
                                            s.ID,
                                            s.FirstName,
                                            s.LastName,
                                            s.PhoneNo
                                        }).ToList();

            setColumnNames.Start();
        }

        private void txtSearchProfessorFName_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (((TextBox)sender).Text == "نام استاد")
            {
                ((TextBox)sender).Text = string.Empty;
            }
        }

        private void txtSearchProfessorFName_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(((TextBox)sender).Text))
            {
                ((TextBox)sender).Text = "نام استاد";
            }
        }

        private void txtSearchProfessorFName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (((TextBox)sender).Text == "نام استاد")
            {
                ((TextBox)sender).Text = string.Empty;
            }
        }

        private void txtSearchProfessorFName_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(((TextBox)sender).Text))
            {
                ((TextBox)sender).Text = "نام استاد";
            }
        }

        private void txtSearchProfessorLName_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (((TextBox)sender).Text == "نام خانوادگی استاد")
            {
                ((TextBox)sender).Text = string.Empty;
            }
        }

        private void txtSearchProfessorLName_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(((TextBox)sender).Text))
            {
                ((TextBox)sender).Text = "نام خانوادگی استاد";
            }
        }

        private void txtSearchProfessorLName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (((TextBox)sender).Text == "نام خانوادگی استاد")
            {
                ((TextBox)sender).Text = string.Empty;
            }
        }

        private void txtSearchProfessorLName_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(((TextBox)sender).Text))
            {
                ((TextBox)sender).Text = "نام خانوادگی استاد";
            }
        }

        #endregion

        private void btnOk_Click(object sender, System.Windows.RoutedEventArgs e)
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
                int ID;
                if (int.TryParse(id, out ID))
                {
                    //
                    // select record in DB by funded numID
                    selectedProfessor = (from s in DB.Professors
                                       where s.ID == ID
                                       select s).Single();
                }
            }
            else selectedProfessor = null;
            this.Close();
        }

        private void btnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            selectedProfessor = null;
            this.Close();
        }
	}
}
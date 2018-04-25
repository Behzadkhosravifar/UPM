using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Security.Cryptography;

namespace UPM
{
    /// <summary>
    /// Interaction logic for UserAccountsWin.xaml
    /// </summary>
    public partial class UserAccountsWin : Window
    {
        public static string entryUserName = "";

        public UserAccountsWin()
        {
            InitializeComponent();
            //
            // Set Current UserName
            if (entryUserName == "") entryUserName = @"#admin#";
            lblUserName.Text = entryUserName;
        }

        #region Converter Code
        public string CreateMD5Hash(string input)
        {
            //Create a byte array from source data
            byte[] inputBytes = ASCIIEncoding.ASCII.GetBytes(input);
            //
            //Compute hash based on source data
            byte[] hashBytes = new MD5CryptoServiceProvider().ComputeHash(inputBytes);
            //
            // Convert the byte array to hexadecimal string
            //
            StringBuilder sOutput = new StringBuilder(hashBytes.Length);
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sOutput.Append(hashBytes[i].ToString("X2"));
                // To force the hex string to lower-case letters instead of
                // upper-case, use he following line instead:
                // sOutput.Append(hashBytes[i].ToString("x2")); 
            }
            return sOutput.ToString();
        }

        private List<int> changedRowsIndex = new List<int>();

        private void dgvUserPass_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            changedRowsIndex.Add(e.Row.GetIndex());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                foreach (int index in changedRowsIndex)
                {
                    User_Password userpass = (User_Password)dgvUserPass.Items.GetItemAt(index);
                    new LINQ_UserPassDataContext().EditUserPass(userpass.UserName,
                                                                userpass.Password,
                                                                userpass.PasswordHint,
                                                                userpass.Modifiers);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void exUserAccount_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (exPassword.IsExpanded)
            {
                exPassword.IsExpanded = false;
            }
        }

        private void exPassword_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (exUserAccount.IsExpanded)
            {
                exUserAccount.IsExpanded = false;
            }
        }
        #endregion

        #region Change Password Panel's Controls
        void txtPassHint_KeyUp(object sender, KeyEventArgs e)
        {
            if (txtPassHint.Text == "")
            {
                txtPassHint.Foreground = Brushes.DimGray;
                txtPassHint.Text = "Password Hint";
            }
        }

        void txtPassHint_KeyDown(object sender, KeyEventArgs e)
        {
            if (txtPassHint.Foreground == Brushes.DimGray)
            {
                txtPassHint.Text = string.Empty;
                txtPassHint.Foreground = Brushes.Black;
            }
        }

        void txtPassHint_MouseLeave(object sender, MouseEventArgs e)
        {
            if (txtPassHint.Text == "")
            {
                txtPassHint.Foreground = Brushes.DimGray;
                txtPassHint.Text = "Password Hint";
            }
        }

        private void txtPassHint_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	if (txtPassHint.Foreground == Brushes.DimGray)
            {
                txtPassHint.Text = string.Empty;
                txtPassHint.Foreground = Brushes.Black;
            }
        }

        private void btnChangePass_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (txtCurrentPass.Password != "" && txtNewPass.Password != "" & txtConfirmNewPass.Password != "")
            {
                if (txtConfirmNewPass.Password == txtNewPass.Password) // New Pass == Confirm New Pass
                {
                    string CurrentPassword = CreateMD5Hash(txtCurrentPass.Password);

                    string NewPassword = CreateMD5Hash(txtNewPass.Password);

                    var db = (from UP in new LINQ_UserPassDataContext().User_Passwords
                              where (UP.UserName.ToLower() == lblUserName.Text.ToLower()) && (UP.Password == CurrentPassword)
                              select UP).SingleOrDefault();

                    if (db != null) // Correct current pass
                    {
                        new LINQ_UserPassDataContext().EditUserPass(db.UserName, NewPassword, txtPassHint.Text, db.Modifiers);
                        MessageBox.Show("Change password successfully!");
                    }
                    else // Incorrect current pass
                    {
                        MessageBox.Show("Your password is incorrect!",
                            "Incorrect Password", MessageBoxButton.OK, MessageBoxImage.Stop);
                    }
                    // 
                    // clear any textBox
                    //
                    txtPassHint.Foreground = Brushes.DimGray;
                    txtPassHint.Text = "Password Hint";
                    txtNewPass.Password = string.Empty;
                    txtCurrentPass.Password = string.Empty;
                    txtConfirmNewPass.Password = string.Empty;
                    txtCurrentPass.Focus();
                }
                else
                {
                    MessageBox.Show("The passwords you typed do not match." +
                            " Please retype the new password in both boxes.",
                            "User Account Control Panel");
                    txtNewPass.Password = string.Empty;
                    txtConfirmNewPass.Password = string.Empty;
                    txtNewPass.Focus();
                }
            }
            else
            {
                MessageBox.Show("You have empty important textBoxes. Please fill all boxes.", "Empty TextBox Error");
            }
        }
        #endregion        

        #region Change Accounts Panel's Controls
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (txtUserName.Text != "" && txtPass.Password != "" && txtConfirmPass.Password != "")
            {
                if (txtPass.Password == txtConfirmPass.Password) // Pass == Confirm Pass
                {
                    var oldUser = (from old in new LINQ_UserPassDataContext().User_Passwords
                                   where old.UserName.ToLower() == txtUserName.Text.ToLower()
                                   select old).SingleOrDefault();
                    if (oldUser == null)
                    {
                        string Password = CreateMD5Hash(txtPass.Password);

                        new LINQ_UserPassDataContext().SaveUserPass(txtUserName.Text, Password, txtModifiers.Text, txtPasswordHint.Text);
                        MessageBox.Show("User by name (" + txtUserName.Text + ") successful added to program's login");
                        // 
                        // clear any textBox
                        //
                        txtModifiers.Text = string.Empty;
                        txtPasswordHint.Text = string.Empty;
                        txtConfirmPass.Password = string.Empty;
                        txtPass.Password = string.Empty;
                        txtUserName.Text = string.Empty;
                        txtUserName.Focus();
                    }
                    else // this user already added to User_Passwords database
                    {
                        MessageBox.Show("This user name is already added in user_passwords database! \rPlease enter other name"
                            , "Find Duplicate User Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtUserName.Text = string.Empty;
                        txtPass.Password = string.Empty;
                        txtConfirmPass.Password = string.Empty;
                        txtUserName.Focus();
                    }
                }
                else
                {
                    MessageBox.Show("The passwords you typed do not match." +
                            " Please retype the new password in both boxes.",
                            "User Account Control Panel");
                    txtConfirmPass.Password = string.Empty;
                    txtPass.Password = string.Empty;
                    txtPass.Focus();
                }
            }
            else
            {
                MessageBox.Show("You have empty important textBoxes. Please fill all boxes.", "Empty TextBox Error");
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (txtUserName.Text != "" && txtPass.Password != "")
            {
                string Password = CreateMD5Hash(txtPass.Password);

                var dbUP = (from user in new LINQ_UserPassDataContext().User_Passwords
                            where (user.UserName.ToLower() == txtUserName.Text.ToLower()) && (user.Password == Password)
                            select user.UserName).SingleOrDefault();
                if (dbUP != null)
                {
                    if (MessageBox.Show("Are you sure to delete user by name (" + txtUserName.Text + ") ?", "Deleting Question's"
                        , MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
                    {
                        new LINQ_UserPassDataContext().DeleteUserPass(dbUP);
                        txtUserName.Text = string.Empty;
                        txtPass.Password = string.Empty;
                        txtConfirmPass.Password = string.Empty;
                        txtPasswordHint.Text = string.Empty;
                        txtModifiers.Text = string.Empty;
                        txtUserName.Focus();
                    }
                }
                else
                {
                    MessageBox.Show("Your user name or password is incorrect");
                    txtUserName.Text = string.Empty;
                    txtPass.Password = string.Empty;
                    txtUserName.Focus();
                }
            }
            else
            {
                MessageBox.Show("You have empty important textBoxes. Please fill all boxes.", "Empty TextBox Error");
            }
        }

        bool Lock = true;
        private void btnUnLock_Click(object sender, RoutedEventArgs e)
        {
            if (Lock) // UnLock
            {
                if (txtUserName.Text != "" && txtPass.Password != "")
                {
                    var db = new LINQ_UserPassDataContext();
                    var arrAdmin = (from adminUser in db.User_Passwords
                                    where adminUser.UserName == @"#admin#"
                                    select adminUser).ToArray();
                    User_Password admin = new User_Password();
                    if (arrAdmin.Length > 0)
                        admin = arrAdmin[0];
                    else return;
                    string Password = CreateMD5Hash(txtPass.Password);

                    if (txtUserName.Text.ToLower() == admin.UserName.ToLower() && Password == admin.Password)
                    {
                        Lock = false;
                        ImageSourceConverter imgConvert = new ImageSourceConverter();
                        Uri uri = new Uri(@"Pictures\unlock.png", UriKind.Relative);
                        ImageSource imgSource = new BitmapImage(uri);
                        picLocker.Source = imgSource;
                        
                        dgvUserPass.ItemsSource = db.User_Passwords;
                        //
                        dgvUserPass.Columns[0].Header = "User Name";
                        dgvUserPass.Columns[0].Width = 120;
                        dgvUserPass.Columns[0].IsReadOnly = true;
                        //
                        dgvUserPass.Columns[1].Header = "Password";
                        dgvUserPass.Columns[1].Width = DataGridLength.Auto;
                        //
                        dgvUserPass.Columns[2].Header = "Password Hint";
                        dgvUserPass.Columns[2].Width = 100;
                        //
                        dgvUserPass.Columns[3].Header = "Modifiers";
                        dgvUserPass.Columns[3].Width = 100;
                        //
                        this.Width = 900;
                    }
                }
            }
            else // Lock
            {
                dgvUserPass.Columns.Clear();
                this.Width = 400;
                ImageSourceConverter imgConvert = new ImageSourceConverter();
                Uri uri = new Uri(@"Pictures\lock.png", UriKind.Relative);
                ImageSource imgSource = new BitmapImage(uri);
                picLocker.Source = imgSource;
                txtUserName.Text = string.Empty;
                txtPass.Password = string.Empty;
                txtConfirmPass.Password = string.Empty;
                txtPasswordHint.Text = string.Empty;
                txtModifiers.Text = string.Empty;
                txtUserName.Focus();
                Lock = true;
            }
        }

        private void txtOriginalPass_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (txtOriginalPass.Foreground == Brushes.DimGray)
            {
                txtOriginalPass.Text = "";
                txtOriginalPass.Foreground = Brushes.Black;
            }
        }

        private void txtOriginalPass_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (txtOriginalPass.Text == "")
            {
                txtOriginalPass.Foreground = Brushes.DimGray;
                txtOriginalPass.Text = "Original Password";
            }
        }

        private void txtOriginalPass_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (txtOriginalPass.Text == "")
            {
                txtOriginalPass.Foreground = Brushes.DimGray;
                txtOriginalPass.Text = "Original Password";
            }
        }

        private void txtOriginalPass_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (txtOriginalPass.Foreground == Brushes.DimGray)
            {
                txtOriginalPass.Text = "";
                txtOriginalPass.Foreground = Brushes.Black;
            }
        }

        private void btnConvert_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (txtOriginalPass.Text != "" && txtOriginalPass.Foreground == Brushes.Black)
            {
                txtHashCode.Foreground = Brushes.Black;
                txtHashCode.Text = CreateMD5Hash(txtOriginalPass.Text);
            }
        }
        #endregion
    }
}


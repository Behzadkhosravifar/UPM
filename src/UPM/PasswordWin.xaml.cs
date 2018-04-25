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
    /// Interaction logic for PasswordWin.xaml
    /// </summary>
    public partial class PasswordWin : Window
    {
        public static bool correctPassword = false;

        public PasswordWin()
        {
            InitializeComponent();
            #region Define Admin
            new LINQ_UserPassDataContext().SaveUserPass(@"#admin#",
                "6E68BCEE1160C9CE79643F96EE24AD95",
                "Administrator", "A13");
            #endregion
        }

        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            correctPassword = false;
            this.Close();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            this.DragMove();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            EnterPassword(txtUserName.Text, txtPass.Password);
        }

        private void txtUserName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                EnterPassword(txtUserName.Text, txtPass.Password);
            }
            else e.Handled = false;
        }

        private void EnterPassword(string user, string pass)
        {
            var dbUser = (from qUP in new LINQ_UserPassDataContext().User_Passwords
                          where qUP.UserName == user
                          select new { qUP.UserName, qUP.Password, qUP.PasswordHint }).SingleOrDefault();
            
            if (dbUser == null)
            {
                lblPassHint.Foreground = Brushes.Red;
                Hint = "User or Pass is incorrect";

                txtPass.Password = string.Empty;
                txtUserName.Text = string.Empty;
                txtUserName.Focus();
            }
            else
            {
                string PassCode = CreateMD5Hash(pass);

                if (dbUser.Password != PassCode)
                {
                    MessageBox.Show("Your Password is incorrect", "Password Error");
                    if (dbUser.PasswordHint != null)
                    {
                        lblPassHint.Foreground = Brushes.Black;
                        Hint = "Password Hint:   " + dbUser.PasswordHint;
                    }
                    txtPass.Password = string.Empty;
                    txtPass.Focus();
                }
                else
                {
                    correctPassword = true;
                    UserAccountsWin.entryUserName = dbUser.UserName;
                    this.Close();
                }
            }

        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            txtUserName.Focus();
        }

        private string Hint
        {
            set
            {
                lblPassHint.Visibility = System.Windows.Visibility.Visible;
                lblPassHint.Content = value;
            }
            get { return (string)lblPassHint.Content; }
        }

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
    }
}

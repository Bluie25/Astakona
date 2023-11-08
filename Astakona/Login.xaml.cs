using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Configuration;
using System.Data.SqlClient;

namespace Astakona
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public string connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

        public Login()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(this.connection))
                {
                    conn.Open();
                    string query = "SELECT * FROM Employees WHERE Username = @Username AND Password = @Password";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", UserLogin.Text);
                        cmd.Parameters.AddWithValue("@Password", UserPassword.Password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                LoggedInDetails LoggedInUser = new LoggedInDetails{
                                    EmployeeID = reader.GetInt32(reader.GetOrdinal("EmployeeID")),
                                    EmployeeUsername = reader.GetString(reader.GetOrdinal("Username"))
                                };
                                ((App)Application.Current).LoggedInUser = LoggedInUser;
                                Dashboard Dashboard = new Dashboard();
                                this.Close();
                                Dashboard.Show();
                            }
                            else
                                MessageBox.Show("Login failed. Please check your credentials.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }
    

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
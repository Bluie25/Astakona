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
                                    Name = reader.GetString(reader.GetOrdinal("EmployeeName")),
                                    Username = reader.GetString(reader.GetOrdinal("Username")),
                                    Password = reader.GetString(reader.GetOrdinal("Password")),
                                    Salary = reader.GetFloat(reader.GetOrdinal("Salary")),
                                    ManageAccounts = reader.GetBoolean(reader.GetOrdinal("ManageAccounts")),
                                    ManageSalaries = reader.GetBoolean(reader.GetOrdinal("ManageSalaries")),
                                    AddOrder = reader.GetBoolean(reader.GetOrdinal("AddOrder")),
                                    UpdateOrder = reader.GetBoolean(reader.GetOrdinal("UpdateOrder")),
                                    DeleteOrder = reader.GetBoolean(reader.GetOrdinal("DeleteOrder")),
                                    AddInventory = reader.GetBoolean(reader.GetOrdinal("AddInventory")),
                                    UpdateInventory = reader.GetBoolean(reader.GetOrdinal("UpdateInventory")),
                                    DeleteInventory = reader.GetBoolean(reader.GetOrdinal("DeleteInventory")),
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
                // Log the exception details, including the stack trace
                MessageBox.Show($"An error occurred: {ex.Message}\n\nStackTrace: {ex.StackTrace}");

                // Optionally, log inner exceptions if present
                Exception innerException = ex.InnerException;
                while (innerException != null)
                {
                    MessageBox.Show($"Inner Exception: {innerException.Message}\n\nStackTrace: {innerException.StackTrace}");
                    innerException = innerException.InnerException;
                }
            }
        }
    

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
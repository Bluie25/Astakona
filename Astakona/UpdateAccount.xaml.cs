using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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

namespace Astakona
{
    /// <summary>
    /// Interaction logic for UpdateAccount.xaml
    /// </summary>
    public partial class UpdateAccount : Window
    {
        private HubConnection _hubConnection;
        public string connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString + ";MultipleActiveResultSets=True";
        public string LBL;
        public int EmployeeID;

        public UpdateAccount(string Label)
        {
            InitializeComponent();
            PlaceholderLB.Content = Label;
            this.LBL = Label;
            LoadDetails();
            InitializeSignalR();
        }

        private void LoadDetails()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    conn.Open();

                    using (SqlCommand Query = new SqlCommand("SELECT * FROM Employees WHERE EmployeeID=@EmployeeID", conn))
                    {
                        Query.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = ((App)Application.Current).LoggedInUser.EmployeeID;
                        SqlDataReader Reader = Query.ExecuteReader();

                        while (Reader.Read())
                        {
                            if (this.LBL == "Username:")
                                PlaceholderTB.Text = Convert.ToString(Reader["Username"]);
                            else
                                PlaceholderTB.Text = Convert.ToString(Reader["Password"]);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                 .WithUrl("http://192.168.1.27:5210/Hubs")
                 .Build();

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            await _hubConnection.StartAsync();
        }

        private async void UpdateButtonClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PlaceholderTB.Text))
            {
                string ConnQuery;
                if (this.LBL == "Username:")
                    ConnQuery = "UPDATE Employees SET Username=@Value WHERE EmployeeID=@EmployeeID";
                else
                    ConnQuery = "UPDATE Employees SET Password=@Value WHERE EmployeeID=@EmployeeID";

                try
                {
                    using (SqlConnection conn = new SqlConnection(this.connection))
                    {
                        conn.Open();

                        using (SqlCommand Query = new SqlCommand(ConnQuery, conn))
                        {
                            Query.Parameters.Add("@Value", SqlDbType.NVarChar).Value = PlaceholderTB.Text;
                            Query.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = ((App)Application.Current).LoggedInUser.EmployeeID;
                            Query.ExecuteNonQuery();

                            if (this.LBL == "Username:")
                                ((App)Application.Current).LoggedInUser.Username = PlaceholderTB.Text;
                            else
                                ((App)Application.Current).LoggedInUser.Password = PlaceholderTB.Text;

                            if (_hubConnection != null)
                            {
                                await _hubConnection.InvokeAsync("SendAccountsPageUpdate");
                                await _hubConnection.StopAsync();
                            }

                            conn.Close();
                            this.Close();
                        }
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading orders: {ex.Message}\n{ex.StackTrace}");
                }
            }

            else
                MessageBox.Show($"Mohon mengisi!", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e) 
        {
            _hubConnection?.StopAsync();
            this.Close();
        }
    }
}

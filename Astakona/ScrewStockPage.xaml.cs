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
using static System.Net.WebRequestMethods;

namespace Astakona
{
    /// <summary>
    /// Interaction logic for ScrewStockPage.xaml
    /// </summary>
    public partial class ScrewStockPage : Window
    {
        private HubConnection _hubConnection;
        public string connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

        public ScrewStockPage()
        {
            InitializeComponent();
            LoadScrewsStock();
            InitializeSignalR();

        }

        private async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://192.168.1.3:5210/Hubs")
                .Build();

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            await _hubConnection.StartAsync();

            _hubConnection.On("ReceiveScrewUpdate", () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadScrewsStock();
                });
            });


        }

        public void LoadScrewsStock()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    conn.Open();
                    SqlCommand query = new SqlCommand("SELECT * FROM Screws", conn);
                    SqlDataReader reader = query.ExecuteReader();

                    while (reader.Read())
                    {
                        switch (Convert.ToInt32(reader["ScrewID"]))
                        {
                            case 1:
                                BigScrewTB.Text = Convert.ToString(reader["Stock"]);
                                break;
                            case 2:
                                SmallScrewTB.Text = Convert.ToString(reader["Stock"]);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void HomePageButton_Click(object sender, RoutedEventArgs e)
        {
            Dashboard Dashboard = new Dashboard();
            if (_hubConnection != null)
            {
                _hubConnection.StopAsync();
            }
            this.Close();
            Dashboard.Show();
        }

        private void OrderButtonClick(object sender, RoutedEventArgs e)
        {
            OrderPage orderPage = new OrderPage();
            this.Close();
            orderPage.Show();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).ClearLoggedInUserData();
            Login Login = new Login();
            this.Close();
            Login.Show();
        }

        private void BigScrewUpdateButton_Clicked(object sender, RoutedEventArgs e)
        {
            UpdateScrewStock UpdateScrewStock = new UpdateScrewStock(1);
            UpdateScrewStock.Show();
        }

        private void SmallScrewUpdateButton_Clicked(object sender, RoutedEventArgs e)
        {
            UpdateScrewStock UpdateScrewStock = new UpdateScrewStock(2);
            UpdateScrewStock.Show();
        }
    }
}

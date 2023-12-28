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
    public partial class MaterialPage : Window
    {
        private HubConnection _hubConnection;
        public string connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

        public MaterialPage()
        {
            InitializeComponent();
            LoadMaterialsStock();
            InitializeSignalR();

        }

        private async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://192.168.1.3:5210/Hubs")
                .Build();

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            await _hubConnection.StartAsync();

            _hubConnection.On("ReceiveMaterialsPageUpdate", () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadMaterialsStock();
                });
            });
        }

        public void LoadMaterialsStock()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    conn.Open();
                    SqlCommand query = new SqlCommand("SELECT * FROM Materials", conn);
                    SqlDataReader reader = query.ExecuteReader();

                    while (reader.Read())
                    {
                        switch (Convert.ToInt32(reader["MaterialID"]))
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

        private void HomeButtonClick(object sender, RoutedEventArgs e)
        {
            Dashboard Dashboard = new Dashboard();
            _hubConnection?.StopAsync();
            this.Close();
            Dashboard.Show();
        }

        private void OrderButtonClick(object sender, RoutedEventArgs e)
        {
            OrderPage OrderPage = new OrderPage();
            _hubConnection?.StopAsync();
            this.Close();
            OrderPage.Show();
        }

        private void DeliveryButtonClick(object sender, RoutedEventArgs e)
        {
            DeliveryPage DeliveryPage = new DeliveryPage();
            _hubConnection?.StopAsync();
            this.Close();
            DeliveryPage.Show();
        }

        private void PalletButtonClick(object sender, RoutedEventArgs e)
        {
            PalletPage PalletPage = new PalletPage();
            _hubConnection?.StopAsync();
            this.Close();
            PalletPage.Show();
        }

        private void AccountButtonClick(object sender, RoutedEventArgs e)
        {
            AccountPage AccountPage = new AccountPage();
            _hubConnection?.StopAsync();
            this.Close();
            AccountPage.Show();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).ClearLoggedInUserData();
            Login Login = new Login();
            _hubConnection?.StopAsync();
            this.Close();
            Login.Show();
        }
    }
}

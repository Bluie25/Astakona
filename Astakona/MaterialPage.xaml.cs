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

            var LoggedInUser = ((App)Application.Current).LoggedInUser;
            if (!LoggedInUser.UpdateMaterial)
            {
                BigScrewBtn.IsEnabled = false;
                SmallScrewBtn.IsEnabled = false;
                Triplek18mmBtn.IsEnabled = false;
                Triplek15mmBtn.IsEnabled = false;
                Triplek12mmBtn.IsEnabled = false;
            }
        }

        private async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("INSERT LOCAL IP ADDRESS")
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
                            case 3:
                                Triplek18mmTB.Text = Convert.ToString(reader["Stock"]);
                                break;
                            case 4:
                                Triplek15mmTB.Text = Convert.ToString(reader["Stock"]);
                                break;
                            case 5:
                                Triplek12mmTB.Text = Convert.ToString(reader["Stock"]);
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

        private void Triplek18mmUpdateButton_Clicked(object sender, RoutedEventArgs e)
        {
            UpdateScrewStock UpdateScrewStock = new UpdateScrewStock(3);
            UpdateScrewStock.Show();
        }

        private void Triplek15mmUpdateButton_Clicked(object sender, RoutedEventArgs e)
        {
            UpdateScrewStock UpdateScrewStock = new UpdateScrewStock(4);
            UpdateScrewStock.Show();
        }

        private void Triplek12mmUpdateButton_Clicked(object sender, RoutedEventArgs e)
        {
            UpdateScrewStock UpdateScrewStock = new UpdateScrewStock(5);
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

        private void ReturnButtonClick(object sender, RoutedEventArgs e)
        {
            OrderReturnPage OrderReturnPage = new OrderReturnPage();
            _hubConnection?.StopAsync();
            this.Close();
            OrderReturnPage.Show();
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

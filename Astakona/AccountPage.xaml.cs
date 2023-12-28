using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Configuration;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;

namespace Astakona
{
    /// <summary>
    /// Interaction logic for AccountPage.xaml
    /// </summary>
    public partial class AccountPage : Window
    {
        private HubConnection _hubConnection;
        public string connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString + ";MultipleActiveResultSets=True";

        public AccountPage()
        {
            InitializeComponent();
            LoadPage();
            InitializeSignalR();
        }

        public void LoadPage()
        {
            NameTB.Text = ((App)Application.Current).LoggedInUser.Name;
            UsernameTB.Text = ((App)Application.Current).LoggedInUser.Username;
            PasswordBox.Password = ((App)Application.Current).LoggedInUser.Password;
        }

        private async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://192.168.1.3:5210/Hubs")
                .Build();

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            await _hubConnection.StartAsync();

            _hubConnection.On("ReceiveAccountsPageUpdate", () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadPage();
                });
            });
        }

        private void ShowButtonClick(object sender, MouseButtonEventArgs e)
        {
            PasswordBox.Visibility = Visibility.Visible;
            PasswordBox.Focus();
        }

        private void ShowButtonRelease(object sender, MouseButtonEventArgs e)
        {
            PasswordBox.Visibility = Visibility.Hidden;
        }

        private void EditUsernameButtonClick(object sender, RoutedEventArgs e)
        {
            UpdateAccount UpdateAccount = new UpdateAccount("Username:");
            UpdateAccount.Show();
        }

        private void EditPasswordButtonClick(object sender, RoutedEventArgs e)
        {
            UpdateAccount UpdateAccount = new UpdateAccount("Password:");
            UpdateAccount.Show();
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

        private void MaterialButtonClick(object sender, RoutedEventArgs e)
        {
            MaterialPage MaterialPage = new MaterialPage();
            _hubConnection?.StopAsync();
            this.Close();
            MaterialPage.Show();
        }

        private void PalletButtonClick(object sender, RoutedEventArgs e)
        {
            PalletPage PalletPage = new PalletPage();
            _hubConnection?.StopAsync();
            this.Close();
            PalletPage.Show();
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

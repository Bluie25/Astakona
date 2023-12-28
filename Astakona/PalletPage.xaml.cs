using Microsoft.AspNetCore.SignalR.Client;
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
    /// Interaction logic for PalletPage.xaml
    /// </summary>
    public partial class PalletPage : Window
    {
        private HubConnection _hubConnection;
        public string connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString + ";MultipleActiveResultSets=True";
        private Button UpdatePalletBtn;
        private Button DeletePalletBtn;
        public List<PalletDetails> Pallets { get; set; }

        public PalletPage()
        {
            InitializeComponent();
            InitializeSignalR();
            Pallets = new List<PalletDetails>();
            LoadPage();
            DataContext = this;

            var LoggedInUser = ((App)Application.Current).LoggedInUser;
            if (!LoggedInUser.AddInventory)
                AddPalletBtn.IsEnabled = false;
        }

        private void LoadPage()
        {
            Pallets.Clear();

            try
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    conn.Open();

                    using (SqlCommand Query = new SqlCommand("SELECT * FROM Inventories", conn))
                    {
                        SqlDataReader Reader = Query.ExecuteReader();
                        while (Reader.Read())
                        {
                            this.Pallets.Add(new PalletDetails()
                            {
                                InventoryID = Convert.ToInt32(Reader["InventoryID"]),
                                Name = Convert.ToString(Reader["Name"]),
                                Stock = Convert.ToDouble(Reader["Stock"]),
                                BigScrew = Convert.ToDouble(Reader["BigScrew"]),
                                SmallScrew = Convert.ToDouble(Reader["SmallScrew"])
                            });
                        }
                        CollectionViewSource.GetDefaultView(Pallets).Refresh();
                        Reader.Close();
                        conn.Close();
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
                .WithUrl("http://192.168.1.3:5210/Hubs")
                .Build();

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            await _hubConnection.StartAsync();

            _hubConnection.On("ReceivePalletsPageUpdate", () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadPage();
                });
            });
        }


        private void UpdatePalletBtn_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdatePalletBtn = (Button)sender;
        }

        private void DeletePalletBtn_Loaded(object sender, RoutedEventArgs e)
        {
            this.DeletePalletBtn = (Button)sender;
        }

        private void ListViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            ListViewItem listViewItem = (ListViewItem)sender;
            Button UpdateButton = FindChild<Button>(listViewItem, "UpdatePalletBtn");
            Button DeleteButton = FindChild<Button>(listViewItem, "DeletePalletBtn");

            if (UpdateButton != null && DeleteButton != null)
            {
                var loggedInUser = ((App)Application.Current).LoggedInUser;

                if (!loggedInUser.UpdateInventory)
                    UpdateButton.IsEnabled = false;

                if (!loggedInUser.DeleteInventory)
                    DeleteButton.IsEnabled = false;
            }
        }

        private T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T && ((FrameworkElement)child).Name == childName)
                {
                    return (T)child;
                }
                else
                {
                    T childOfChild = FindChild<T>(child, childName);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private void AddPalletButton_Click(object sender, RoutedEventArgs e)
        {
            AddPallet AddPallet = new AddPallet();
            AddPallet.Show();
        }

        private void UpdatePalletButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                PalletDetails SelectedPallet = btn.DataContext as PalletDetails;
                if (SelectedPallet != null)
                {
                    UpdatePallet UpdatePallet = new UpdatePallet(SelectedPallet);
                    UpdatePallet.Show();
                }
            }
        }
        private async void DeletePalletButton_Click(object sender, RoutedEventArgs e)
        {
           Button btn = sender as Button;
           if (btn != null)
           {
               PalletDetails SelectedPallet = btn.DataContext as PalletDetails;
               if (SelectedPallet != null)
               {
                   MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this pallet?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                   if (result == MessageBoxResult.Yes)
                   {
                       try
                       {
                           using (SqlConnection conn = new SqlConnection(this.connection))
                           {
                               conn.Open();
                               string query = $"DELETE FROM Inventories WHERE InventoryID = {SelectedPallet.InventoryID}";

                               using (SqlCommand DeleteCommand = new SqlCommand(query, conn))
                               {
                                   DeleteCommand.ExecuteNonQuery();
                               }

                               await _hubConnection.InvokeAsync("SendPalletsPageUpdate");
                               conn.Close();
                           }
                       }

                       catch (Exception ex)
                       {
                           MessageBox.Show($"Error deleting order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                       }
                   }
               }
           }
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

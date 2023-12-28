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
    /// Interaction logic for DeliveryPage.xaml
    /// </summary>
    public partial class DeliveryPage : Window
    {
        private HubConnection _hubConnection;
        public string connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString + ";MultipleActiveResultSets=True";
        private Button UpdateDeliveryBtn;
        public List<OrdersDetails> Delivery { get; set; }

        public DeliveryPage()
        {
            InitializeComponent();
            InitializeSignalR();
            Delivery = new List<OrdersDetails>();
            LoadPalletPage();
            DataContext = this;
        }

        private async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://192.168.1.3:5210/Hubs")
                .Build();

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            await _hubConnection.StartAsync();

            _hubConnection.On("ReceiveDeliveriesPageUpdate", () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadPalletPage();
                });
            });
        }

        public void LoadPalletPage()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    conn.Open();

                    using (SqlCommand Query = new SqlCommand("SELECT * FROM Orders WHERE IsFinished=0", conn))
                    {
                        SqlDataReader Reader = Query.ExecuteReader();
                        Delivery.Clear();
                        while (Reader.Read())
                        {
                            this.Delivery.Add(new OrdersDetails()
                            {
                                OrderID = Convert.ToInt32(Reader["OrderID"]),
                                InvoiceNo = Convert.ToString(Reader["InvoiceNo"]),
                                InventoryID = Convert.ToInt32(Reader["InventoryID"]),
                                InventoryName = Convert.ToString(Reader["InventoryName"]),
                                Amount = Convert.ToDouble(Reader["Amount"]),
                                ProductionCompleted = Convert.ToDouble(Reader["ProductionCompleted"]),
                                HTBTS = Convert.ToDouble(Reader["HTTBS"]),
                                HTEKS = Convert.ToDouble(Reader["HTEKS"]),
                                HTKKS = Convert.ToDouble(Reader["HTKKS"]),
                                TotalHeatCompleted = Convert.ToDouble(Reader["HTEKS"]) + Convert.ToDouble(Reader["HTBTS"]) + Convert.ToDouble(Reader["HTKKS"]),
                                OrderDate = Reader.GetDateTime(Reader.GetOrdinal("OrderDate")),
                                DueDate = Reader.GetDateTime(Reader.GetOrdinal("DueDate")),
                                Customer = Convert.ToString(Reader["Customer"]),
                                ManufactureTeam = Convert.ToString(Reader["ManufactureTeam"]),
                                Delivered = Convert.ToDouble(Reader["Delivered"])
                            });
                        }
                        Reader.Close();
                        CollectionViewSource.GetDefaultView(Delivery).Refresh();
                    }
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void UpdateDeliveryBtn_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateDeliveryBtn = (Button)sender;
        }

        private void ListViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            ListViewItem listViewItem = (ListViewItem)sender;
            Button updateButton = FindChild<Button>(listViewItem, "UpdateDeliveryBtn");

            if (updateButton != null)
            {
                var loggedInUser = ((App)Application.Current).LoggedInUser;

                if (!loggedInUser.UpdateDelivery)
                    updateButton.IsEnabled = false;
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

        private void UpdateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                OrdersDetails SelectedOrder = btn.DataContext as OrdersDetails;
                if (SelectedOrder != null)
                {
                    UpdateDelivery UpdateDelivery = new UpdateDelivery(SelectedOrder);
                    UpdateDelivery.Show();
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

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
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;
using System.Diagnostics;

namespace Astakona
{
    /// <summary>
    /// Interaction logic for OrderPage.xaml
    /// </summary>
    /// </summary>
    public partial class OrderPage : Window
    {
        private HubConnection _hubConnection;
        public string connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
        public List<OrdersDetails> Orders { get; set; }
       
        public OrderPage()
        {
            InitializeComponent();
            InitializeSignalR();
            Orders = new List<OrdersDetails>();
            LoadOrders();
            DataContext = this;
        }

        private async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://192.168.1.3:5210/Hubs")
                .Build();

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            await _hubConnection.StartAsync();

            _hubConnection.On("ReceiveOrderEntry", () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadOrders();
                });
            });

            _hubConnection.On("ReceiveOrderUpdate", () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadOrders();
                });
            });

            _hubConnection.On("ReceiveOrderDelete", () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadOrders();
                });
            });

        }

        public void LoadOrders()
        {
            try
            {
                using(SqlConnection conn = new SqlConnection(connection))
                {
                    conn.Open();
                    SqlCommand query = new SqlCommand("SELECT * FROM Orders", conn);
                    SqlDataReader reader = query.ExecuteReader();
                    Orders.Clear();

                    while (reader.Read())
                    {
                        this.Orders.Add(new OrdersDetails()
                        {
                            OrderID = Convert.ToInt32(reader["OrderID"]),
                            InventoryID = Convert.ToInt32(reader["InventoryID"]),
                            InventoryName = Convert.ToString(reader["InventoryName"]),
                            Amount = Convert.ToDouble(reader["Amount"]),
                            BigScrew = Convert.ToDouble(reader["BigScrew"]),
                            SmallScrew = Convert.ToDouble(reader["SmallScrew"]),
                            ProductionCompleted = Convert.ToDouble(reader["ProductionCompleted"]),
                            HeatCompleted = Convert.ToDouble(reader["HeatCompleted"]),
                            Date = reader.GetDateTime(8),
                            Customer = Convert.ToString(reader["Customer"]),
                        });  
                    }
                    CollectionViewSource.GetDefaultView(Orders).Refresh();
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

        private void ScrewStockButtonClick(object sender, RoutedEventArgs e)
        {
            ScrewStockPage ScrewStockPage = new ScrewStockPage();
            this.Close();
            ScrewStockPage.Show();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).ClearLoggedInUserData();
            Login Login = new Login();
            this.Close();
            Login.Show();
        }

        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            AddOrder AddOrder = new AddOrder();
            AddOrder.Show();
        }

        private void UpdateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                OrdersDetails SelectedOrder = btn.DataContext as OrdersDetails;
                if (SelectedOrder != null)
                {
                    UpdateOrder UpdateOrderPage = new UpdateOrder(SelectedOrder);
                    UpdateOrderPage.Show();
                }
            }
        }

        private async void DeleteOrderButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                OrdersDetails SelectedOrder = btn.DataContext as OrdersDetails;
                if (SelectedOrder != null)
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this order?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            using (SqlConnection conn = new SqlConnection(this.connection))
                            {
                                conn.Open();
                                string query = $"DELETE FROM Orders WHERE OrderID = {SelectedOrder.OrderID}";

                                using (SqlCommand DeleteCommand = new SqlCommand(query, conn))
                                {
                                    DeleteCommand.ExecuteNonQuery();
                                }

                                await _hubConnection.InvokeAsync("SendOrderDelete");
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
    }
}

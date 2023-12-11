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
        public string connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString + ";MultipleActiveResultSets=True";
        public List<OrdersDetails> Orders { get; set; }
       
        public OrderPage()
        {
            InitializeComponent();
            InitializeSignalR();
            Orders = new List<OrdersDetails>();
            LoadPage();
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
                    LoadPage();
                });
            });

            _hubConnection.On("ReceiveOrderUpdate", () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadPage();
                });
            });

            _hubConnection.On("ReceiveOrderDelete", () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadPage();
                });
            });

            _hubConnection.On("ReceiveScrewUpdate", () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadPage();
                });
            });
        }

        public void LoadPage()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    conn.Open();

                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            SqlCommand OrdersQuery = new SqlCommand("SELECT * FROM Orders", conn, transaction);
                            SqlDataReader OrdersReader = OrdersQuery.ExecuteReader();
                            Orders.Clear();

                            while (OrdersReader.Read())
                            {
                                this.Orders.Add(new OrdersDetails()
                                {
                                    OrderID = Convert.ToInt32(OrdersReader["OrderID"]),
                                    InvoiceNo = Convert.ToString(OrdersReader["InvoiceNo"]),
                                    InventoryID = Convert.ToInt32(OrdersReader["InventoryID"]),
                                    InventoryName = Convert.ToString(OrdersReader["InventoryName"]),
                                    Amount = Convert.ToDouble(OrdersReader["Amount"]),
                                    BigScrew = Convert.ToDouble(OrdersReader["BigScrew"]),
                                    SmallScrew = Convert.ToDouble(OrdersReader["SmallScrew"]),
                                    ProductionCompleted = Convert.ToDouble(OrdersReader["ProductionCompleted"]),
                                    HeatCompleted = Convert.ToDouble(OrdersReader["HeatCompleted"]),
                                    Customer = Convert.ToString(OrdersReader["Customer"]),
                                    OrderDate = OrdersReader.GetDateTime(OrdersReader.GetOrdinal("OrderDate")),
                                    DueDate = OrdersReader.GetDateTime(OrdersReader.GetOrdinal("DueDate")),
                                    ManufactureTeam = OrdersReader.GetString(OrdersReader.GetOrdinal("ManufactureTeam"))
                                });
                            }

                            OrdersReader.Close();
                            SqlCommand ScrewsQuery = new SqlCommand("SELECT * FROM Screws", conn, transaction);
                            SqlDataReader ScrewsReader = ScrewsQuery.ExecuteReader();

                            while (ScrewsReader.Read())
                            {
                                switch (Convert.ToDouble(ScrewsReader["ScrewID"]))
                                {
                                    case 1:
                                        BigScrewTB.Text = Convert.ToString(ScrewsReader["Stock"]);
                                        break;
                                    case 2:
                                        SmallScrewTB.Text = Convert.ToString(ScrewsReader["Stock"]);
                                        break;
                                }
                            }

                            transaction.Commit();
                            CollectionViewSource.GetDefaultView(Orders).Refresh();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error during transaction: {ex.Message}\n{ex.StackTrace}");
                            transaction.Rollback();
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

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
                .WithUrl("http://192.168.1.25:5210/OrderHub")
                .Build();

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            await _hubConnection.StartAsync();

            _hubConnection.On("ReceiveOrderUpdate", () =>
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

        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            AddOrder addOrder = new AddOrder();
            addOrder.Show();
        }
    }
}

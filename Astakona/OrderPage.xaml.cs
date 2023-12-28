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
        private Button UpdateOrderBtn;
        private Button DeleteOrderBtn;
        public List<OrdersDetails> Orders { get; set; }

        public OrderPage()
        {
            InitializeComponent();
            InitializeSignalR();
            Orders = new List<OrdersDetails>();
            LoadPage();
            DataContext = this;

            var LoggedInUser = ((App)Application.Current).LoggedInUser;
            if (!LoggedInUser.AddOrder)
                AddOrderBtn.IsEnabled = false;
        }

        private async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://192.168.1.26:5210/Hubs")
                .Build();

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            await _hubConnection.StartAsync();

            _hubConnection.On("ReceiveOrdersPageUpdate", () =>
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
                            SqlCommand OrdersQuery = new SqlCommand("SELECT * FROM Orders WHERE IsFinished=0", conn, transaction);
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
                                    ProductionCompleted = Convert.ToDouble(OrdersReader["ProductionCompleted"]),
                                    HTBTS = Convert.ToDouble(OrdersReader["HTTBS"]),
                                    HTEKS = Convert.ToDouble(OrdersReader["HTEKS"]),
                                    HTKKS = Convert.ToDouble(OrdersReader["HTKKS"]),
                                    TotalHeatCompleted = Convert.ToDouble(OrdersReader["HTEKS"]) + Convert.ToDouble(OrdersReader["HTBTS"]) + Convert.ToDouble(OrdersReader["HTKKS"]),
                                    OrderDate = OrdersReader.GetDateTime(OrdersReader.GetOrdinal("OrderDate")),
                                    DueDate = OrdersReader.GetDateTime(OrdersReader.GetOrdinal("DueDate")),
                                    Customer = Convert.ToString(OrdersReader["Customer"]),
                                    ManufactureTeam = Convert.ToString(OrdersReader["ManufactureTeam"]),
                                    Delivered = Convert.ToDouble(OrdersReader["Delivered"])
                                });
                            }

                            OrdersReader.Close();
                            SqlCommand MaterialsQuery = new SqlCommand("SELECT * FROM Materials", conn, transaction);
                            SqlDataReader MaterialsReader = MaterialsQuery.ExecuteReader();

                            while (MaterialsReader.Read())
                            {
                                switch (Convert.ToDouble(MaterialsReader["MaterialID"]))
                                {
                                    case 1:
                                        BigScrewTB.Text = Convert.ToString(MaterialsReader["Stock"]);
                                        break;
                                    case 2:
                                        SmallScrewTB.Text = Convert.ToString(MaterialsReader["Stock"]);
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

        private void UpdateOrderBtn_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateOrderBtn = (Button)sender;
        }

        private void DeleteOrderBtn_Loaded(object sender, RoutedEventArgs e)
        {
            this.DeleteOrderBtn = (Button)sender;
        }

        private void ListViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            ListViewItem listViewItem = (ListViewItem)sender;
            Button updateButton = FindChild<Button>(listViewItem, "UpdateOrderBtn");
            Button deleteButton = FindChild<Button>(listViewItem, "DeleteOrderBtn");

            if (updateButton != null && deleteButton != null)
            {
                var loggedInUser = ((App)Application.Current).LoggedInUser;

                if (!loggedInUser.UpdateOrder)
                    updateButton.IsEnabled = false;

                if (!loggedInUser.DeleteOrder)
                    deleteButton.IsEnabled = false;
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

                                await _hubConnection.InvokeAsync("SendDeliveriesPageUpdate");
                                await _hubConnection.InvokeAsync("SendOrdersPageUpdate");
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

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            Dashboard Dashboard = new Dashboard();
            _hubConnection?.StopAsync();
            Dashboard.Show();
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

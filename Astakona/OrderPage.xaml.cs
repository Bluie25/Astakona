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
using System.Data;
using System.ComponentModel;

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
        private bool CCB = false;
        private bool OGCB = true;
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
                .WithUrl("http://192.168.1.27:5210/Hubs")
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
                            SqlCommand OrdersQuery = new SqlCommand("SELECT * FROM Orders WHERE IsFinished<>@IsFinished", conn, transaction);
                            if (this.CCB && this.OGCB)
                                OrdersQuery.Parameters.Add("@IsFinished", SqlDbType.Int).Value = -1;
                            else if (!this.CCB && this.OGCB)
                                OrdersQuery.Parameters.Add("@IsFinished", SqlDbType.Int).Value = 1;
                            else if (this.CCB && !this.OGCB)
                                OrdersQuery.Parameters.Add("@IsFinished", SqlDbType.Int).Value = 0;
                            else
                                OrdersQuery = new SqlCommand("SELECT * FROM Orders WHERE IsFinished=-1", conn, transaction);

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
                                    HTBTS = Convert.ToDouble(OrdersReader["HTBTS"]),
                                    HTEKS = Convert.ToDouble(OrdersReader["HTEKS"]),
                                    HTKKS = Convert.ToDouble(OrdersReader["HTKKS"]),
                                    TotalHeatCompleted = Convert.ToDouble(OrdersReader["HTEKS"]) + Convert.ToDouble(OrdersReader["HTBTS"]) + Convert.ToDouble(OrdersReader["HTKKS"]),
                                    OrderDate = OrdersReader.GetDateTime(OrdersReader.GetOrdinal("OrderDate")),
                                    DueDate = OrdersReader.GetDateTime(OrdersReader.GetOrdinal("DueDate")),
                                    Customer = Convert.ToString(OrdersReader["Customer"]),
                                    ManufactureTeam = Convert.ToString(OrdersReader["ManufactureTeam"]),
                                    Delivered = Convert.ToDouble(OrdersReader["Delivered"]),
                                    Triplek18mm = Convert.ToDouble(OrdersReader["Triplek18mm"]),
                                    Triplek15mm = Convert.ToDouble(OrdersReader["Triplek15mm"]),
                                    Triplek12mm = Convert.ToDouble(OrdersReader["Triplek12mm"]),
                                    IsFinished = Convert.ToBoolean(OrdersReader["IsFinished"])
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
                                    case 3:
                                        Triplek18mmTB.Text = Convert.ToString(MaterialsReader["Stock"]);
                                        break;
                                    case 4:
                                        Triplek15mmTB.Text = Convert.ToString(MaterialsReader["Stock"]);
                                        break;
                                    case 5:
                                        Triplek12mmTB.Text = Convert.ToString(MaterialsReader["Stock"]);
                                        break;
                                }
                            }

                            transaction.Commit();
                            CollectionViewSource.GetDefaultView(Orders).Refresh();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error during transaction: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();

            ICollectionView view = CollectionViewSource.GetDefaultView(Orders);
            view.Filter = item =>
            {
                OrdersDetails dataItem = item as OrdersDetails; // Replace YourDataType with the actual type of your data items

                if (dataItem != null)
                {
                    return dataItem.InvoiceNo.ToLower().Contains(searchText)
                        || dataItem.InventoryName.ToLower().Contains(searchText)
                        || dataItem.Customer.ToLower().Contains(searchText)
                        ;
                }

                return false;
            };
        }

        private async void CheckBox_Status(object sender, RoutedEventArgs e)
        {
            if (CompletedCB != null && CompletedCB.IsChecked == true)
                this.CCB = true;
            else
                this.CCB = false;
            if (OnGoingCB != null && OnGoingCB.IsChecked == true)
                this.OGCB = true;
            else
                this.OGCB = false;

            if(_hubConnection != null)
                await _hubConnection.InvokeAsync("SendOrdersPageUpdate");
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
                        using (SqlConnection conn = new SqlConnection(this.connection))
                        {
                            conn.Open();
                            using (SqlTransaction transaction = conn.BeginTransaction())
                            {
                                try
                                {
                                    SqlCommand DeleteOrderCommand = new SqlCommand($"DELETE FROM Orders WHERE OrderID = {SelectedOrder.OrderID}", conn, transaction);
                                    DeleteOrderCommand.ExecuteNonQuery();

                                    ////DELETE RETURNED ORDER DATAS IF HAVE
                                    SqlCommand DeleteReturnedOrderCommand = new SqlCommand($"DELETE FROM ReturnedOrders WHERE OrderID = {SelectedOrder.OrderID}", conn, transaction);
                                    DeleteReturnedOrderCommand.ExecuteNonQuery();

                                    transaction.Commit();
                                    conn.Close();

                                    await _hubConnection.InvokeAsync("SendDeliveriesPageUpdate");
                                    await _hubConnection.InvokeAsync("SendOrdersPageUpdate");
                                    await _hubConnection.InvokeAsync("SendOrdersReturnPageUpdate");
                                    
                                    
                                }

                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Error deleting Order: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    transaction.Rollback();
                                }
                            }
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

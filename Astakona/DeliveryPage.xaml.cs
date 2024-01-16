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
using System.Data;
using System.ComponentModel;

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
        private bool CCB = false;
        private bool OGCB = true;

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
                .WithUrl("http://192.168.1.27:5210/Hubs")
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
            using (SqlConnection conn = new SqlConnection(connection))
            {
                conn.Open();
                try
                {
                    SqlCommand Query = new SqlCommand("SELECT * FROM Orders WHERE IsFinished<>@IsFinished", conn);
                    if (this.CCB && this.OGCB)
                        Query.Parameters.Add("@IsFinished", SqlDbType.Int).Value = -1;
                    else if (!this.CCB && this.OGCB)
                        Query.Parameters.Add("@IsFinished", SqlDbType.Int).Value = 1;
                    else if (this.CCB && !this.OGCB)
                        Query.Parameters.Add("@IsFinished", SqlDbType.Int).Value = 0;
                    else
                        Query = new SqlCommand("SELECT * FROM Orders WHERE IsFinished=-1", conn);

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
                            HTBTS = Convert.ToDouble(Reader["HTBTS"]),
                            HTEKS = Convert.ToDouble(Reader["HTEKS"]),
                            HTKKS = Convert.ToDouble(Reader["HTKKS"]),
                            TotalHeatCompleted = Convert.ToDouble(Reader["HTEKS"]) + Convert.ToDouble(Reader["HTBTS"]) + Convert.ToDouble(Reader["HTKKS"]),
                            OrderDate = Reader.GetDateTime(Reader.GetOrdinal("OrderDate")),
                            DueDate = Reader.GetDateTime(Reader.GetOrdinal("DueDate")),
                            Customer = Convert.ToString(Reader["Customer"]),
                            ManufactureTeam = Convert.ToString(Reader["ManufactureTeam"]),
                            Delivered = Convert.ToDouble(Reader["Delivered"]),
                            IsFinished = Convert.ToBoolean(Reader["IsFinished"]),
                        });
                    }
                    Reader.Close();
                    CollectionViewSource.GetDefaultView(Delivery).Refresh();
                }

                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading orders: {ex.Message}\n{ex.StackTrace}");
                }
            }
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

            if (_hubConnection != null)
                await _hubConnection.InvokeAsync("SendDeliveriesPageUpdate");
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();

            ICollectionView view = CollectionViewSource.GetDefaultView(Delivery);
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

        private void UpdateDeliveryButton_Click(object sender, RoutedEventArgs e)
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

        private void ReturnButtonClick(object sender, RoutedEventArgs e)
        {
            OrderReturnPage OrderReturnPage = new OrderReturnPage();
            _hubConnection?.StopAsync();
            this.Close();
            OrderReturnPage.Show();
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

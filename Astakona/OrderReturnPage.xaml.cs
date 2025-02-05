﻿using Microsoft.AspNetCore.SignalR.Client;
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
    /// Interaction logic for OrderReturnPage.xaml
    /// </summary>
    public partial class OrderReturnPage : Window
    {
        private HubConnection _hubConnection;
        public string connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString + ";MultipleActiveResultSets=True";
        public List<ReturnedOrdersDetails> ReturnedOrders { get; set; }
        private Button UpdateReturnedOrderBtn;
        private Button DeleteReturnedOrderBtn;
        private bool CCB = false;
        private bool OGCB = true;

        public OrderReturnPage()
        {
            InitializeComponent();
            InitializeSignalR();
            ReturnedOrders = new List<ReturnedOrdersDetails>();
            LoadOrdersReturnPage();
            DataContext = this;

            var loggedInUser = ((App)Application.Current).LoggedInUser;
            if (!loggedInUser.AddReturnedOrder)
                AddReturnedOrderBtn.IsEnabled = false;
        }

        private async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("LOCAL IP ADDRESS")
                .Build();

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            await _hubConnection.StartAsync();

            _hubConnection.On("ReceiveOrdersReturnPageUpdate", () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadOrdersReturnPage();
                });
            });
        }

        public void LoadOrdersReturnPage()
        {
            using (SqlConnection conn = new SqlConnection(connection))
            {
                conn.Open();
                try
                {
                    SqlCommand Query = new SqlCommand("SELECT * FROM ReturnedOrders WHERE IsFinished<>@IsFinished", conn);
                    if (this.CCB && this.OGCB)
                        Query.Parameters.Add("@IsFinished", SqlDbType.Int).Value = -1;
                    else if (!this.CCB && this.OGCB)
                        Query.Parameters.Add("@IsFinished", SqlDbType.Int).Value = 1;
                    else if (this.CCB && !this.OGCB)
                        Query.Parameters.Add("@IsFinished", SqlDbType.Int).Value = 0;
                    else
                        Query = new SqlCommand("SELECT * FROM ReturnedOrders WHERE IsFinished=-1", conn);

                    SqlDataReader Reader = Query.ExecuteReader();
                    ReturnedOrders.Clear();

                    while (Reader.Read())
                    {
                        this.ReturnedOrders.Add(new ReturnedOrdersDetails()
                        {
                            ReturnedOrderID = Convert.ToInt32(Reader["ReturnedOrderID"]),
                            OrderId = Convert.ToInt32(Reader["OrderID"]),
                            InvoiceNo = Convert.ToString(Reader["InvoiceNo"]),
                            InventoryName = Convert.ToString(Reader["InventoryName"]),
                            Amount = Convert.ToDouble(Reader["Amount"]),
                            ProductionCompleted = Convert.ToDouble(Reader["PRoductionCompleted"]),
                            HTCompleted = Convert.ToDouble(Reader["HTCompleted"]),
                            Delivered = Convert.ToDouble(Reader["Delivered"]),
                            Customer = Convert.ToString(Reader["Customer"]),
                            ReturnAmount = Convert.ToDouble(Reader["ReturnedAmount"]),
                            BigScrewUsed = Convert.ToDouble(Reader["BigScrewUsed"]),
                            SmallScrewUsed = Convert.ToDouble(Reader["SmallScrewUsed"]),
                            Triplek18mmUsed = Convert.ToDouble(Reader["Triplek18mmUsed"]),
                            Triplek15mmUsed = Convert.ToDouble(Reader["Triplek15mmUsed"]),
                            Triplek12mmUsed = Convert.ToDouble(Reader["Triplek12mmUsed"]),
                            PalletFixed = Convert.ToDouble(Reader["PalletFixed"]),
                            InventoryID = Convert.ToInt32(Reader["InventoryID"]),
                            IsFinished = Convert.ToBoolean(Reader["IsFinished"])
                        });
                    }

                    Reader.Close();
                    CollectionViewSource.GetDefaultView(ReturnedOrders).Refresh();
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
                await _hubConnection.InvokeAsync("SendOrdersReturnPageUpdate");
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();

            ICollectionView view = CollectionViewSource.GetDefaultView(ReturnedOrders);
            view.Filter = item =>
            {
                ReturnedOrdersDetails dataItem = item as ReturnedOrdersDetails; // Replace YourDataType with the actual type of your data items

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

        private void UpdateReturnedOrderBtn_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateReturnedOrderBtn = (Button)sender;
        }
        private void DeleteReturnedOrderBtn_Loaded(object sender, RoutedEventArgs e)
        {
            this.DeleteReturnedOrderBtn = (Button)sender;
        }

        private void ListViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            ListViewItem listViewItem = (ListViewItem)sender;
            Button updateButton = FindChild<Button>(listViewItem, "UpdateReturnedOrderBtn");
            Button deleteButton = FindChild<Button>(listViewItem, "DeleteReturnedOrderBtn");

            if (updateButton != null)
            {
                var loggedInUser = ((App)Application.Current).LoggedInUser;

                if (!loggedInUser.UpdateReturnedOrder)
                    updateButton.IsEnabled = false;

                if (!loggedInUser.DeleteReturnedOrder)
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

        private void AddReturnedOrderButton_Click(object sender, RoutedEventArgs e)
        {
            AddReturnedOrder addReturnedOrder = new AddReturnedOrder();
            addReturnedOrder.Show();
        }

        private void UpdateReturnedOrderButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                ReturnedOrdersDetails SelectedReturnedOrder = btn.DataContext as ReturnedOrdersDetails;
                if (SelectedReturnedOrder != null)
                {
                    UpdateReturnedOrder updateReturnedOrder = new UpdateReturnedOrder(SelectedReturnedOrder);
                    updateReturnedOrder.Show();
                }
            }
        }
        private async void DeleteOrderButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                ReturnedOrdersDetails SelectedReturnedOrder = btn.DataContext as ReturnedOrdersDetails;
                if (SelectedReturnedOrder != null)
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this return?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            using (SqlConnection conn = new SqlConnection(this.connection))
                            {
                                conn.Open();
                                string query = $"DELETE FROM ReturnedOrders WHERE ReturnedOrderID = {SelectedReturnedOrder.ReturnedOrderID}";

                                using (SqlCommand DeleteCommand = new SqlCommand(query, conn))
                                {
                                    DeleteCommand.ExecuteNonQuery();
                                }

                                await _hubConnection.InvokeAsync("SendOrdersReturnPageUpdate");
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

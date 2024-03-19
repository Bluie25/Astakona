using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
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
using Microsoft.AspNetCore.SignalR.Client;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Astakona
{
    /// <summary>
    /// Interaction logic for AddReturnedOrder.xaml
    /// </summary>
    public partial class AddReturnedOrder : Window
    {
        public string connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
        private HubConnection _hubConnection;
        public ReturnedOrdersDetails ReturnedOrderDetails;
        public int SelectedReturnedOrderID;
        public AddReturnedOrder()
        {
            InitializeComponent();
            InitializeSignalR();
            LoadProducts();
        }

        private async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                 .WithUrl("INSERT LOCAL IP ADDRESS")
                 .Build();

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            await _hubConnection.StartAsync();
        }

        private void LoadProducts()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(this.connection))
                {
                    conn.Open();
                    using (SqlCommand query = new SqlCommand("SELECT OrderID, InvoiceNo, InventoryID FROM Orders", conn))
                    {
                        using (SqlDataReader reader = query.ExecuteReader())
                        {
                            DataTable dataTable = new DataTable();
                            dataTable.Load(reader);
                            InvoiceNoComboBox.ItemsSource = dataTable.DefaultView;
                        }
                    }
                }
            }
            catch (Exception ex)
            { 
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnComboBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string userInput = InvoiceNoComboBox.Text;
                if (string.IsNullOrWhiteSpace(userInput))
                    (InvoiceNoComboBox.ItemsSource as DataView).RowFilter = string.Empty;
                else
                    (InvoiceNoComboBox.ItemsSource as DataView).RowFilter = $"InvoiceNo LIKE '%{userInput}%'";
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InvoiceNoComboBox.SelectedItem != null)
            {
                DataRowView SelectedOrder = (DataRowView)InvoiceNoComboBox.SelectedItem;
                int SelectedOrderID = (int)SelectedOrder["OrderID"];
                string SelectedOrderInvoiceNo = (string)SelectedOrder["InvoiceNo"];
                int SelectedOrderInventoryID = (int)SelectedOrder["InventoryID"];

                using (SqlConnection conn = new SqlConnection(this.connection))
                {
                    conn.Open();
                    using (SqlCommand query = new SqlCommand("SELECT * FROM Orders WHERE OrderID=@OrderID", conn))
                    {
                        query.Parameters.Add("@OrderID", SqlDbType.Int).Value = SelectedOrderID;
                        using (SqlDataReader reader = query.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                this.ReturnedOrderDetails = new ReturnedOrdersDetails()
                                {
                                    OrderId = SelectedOrderID,
                                    InvoiceNo = SelectedOrderInvoiceNo,
                                    InventoryName = Convert.ToString(reader["InventoryName"]),
                                    Amount = Convert.ToDouble(reader["Amount"]),
                                    ProductionCompleted = Convert.ToDouble(reader["ProductionCompleted"]),
                                    HTCompleted = Convert.ToDouble(reader["HTBTS"]) + Convert.ToDouble(reader["HTEKS"]) + Convert.ToDouble(reader["HTKKS"]),
                                    Delivered = Convert.ToDouble(reader["Delivered"]),
                                    Customer = Convert.ToString(reader["Customer"]),
                                    InventoryID = SelectedOrderInventoryID
                                };
                            }     
                        }
                    }
                    conn.Close();
                }
            }

            InventoryNameTB.Text = this.ReturnedOrderDetails.InventoryName;
            OrderProgressTB.Text = this.ReturnedOrderDetails.Delivered.ToString() + " / " + this.ReturnedOrderDetails.Amount.ToString();
            CustomerTB.Text = this.ReturnedOrderDetails.Customer;
            ReturnedAmountTB.Text = "0";
        }

        private void AmountTB_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private async void UpdateButton_Clicked(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(ReturnedAmountTB.Text))
            {
                using (SqlConnection conn = new SqlConnection(this.connection))
                {
                    conn.Open();
                    using (SqlCommand Query = new SqlCommand("INSERT INTO ReturnedOrders (OrderID, InvoiceNo, InventoryName, Amount, ProductionCompleted, HTCompleted, Delivered, Customer, ReturnedAmount, InventoryID) " +
                                                                                "VALUES (@OrderID, @InvoiceNo, @InventoryName, @Amount, @ProductionCompleted, @HTCompleted, @Delivered, @Customer, @ReturnedAmount, @InventoryID)", conn))
                    {
                        Query.Parameters.Add("@OrderID", SqlDbType.Int).Value = this.ReturnedOrderDetails.OrderId;
                        Query.Parameters.Add("@InvoiceNo", SqlDbType.NVarChar).Value = this.ReturnedOrderDetails.InvoiceNo;
                        Query.Parameters.Add("@InventoryName", SqlDbType.NVarChar).Value = this.ReturnedOrderDetails.InventoryName;
                        Query.Parameters.Add("@Amount", SqlDbType.Real).Value = this.ReturnedOrderDetails.Amount;
                        Query.Parameters.Add("@ProductionCompleted", SqlDbType.Real).Value = this.ReturnedOrderDetails.ProductionCompleted;
                        Query.Parameters.Add("@HTCompleted", SqlDbType.Real).Value = this.ReturnedOrderDetails.HTCompleted;
                        Query.Parameters.Add("@Delivered", SqlDbType.Real).Value = this.ReturnedOrderDetails.Delivered;
                        Query.Parameters.Add("@Customer", SqlDbType.NVarChar).Value = this.ReturnedOrderDetails.Customer;
                        Query.Parameters.Add("@ReturnedAmount", SqlDbType.Real).Value = Convert.ToDouble(ReturnedAmountTB.Text);
                        Query.Parameters.Add("@InventoryID", SqlDbType.Int).Value = this.ReturnedOrderDetails.InventoryID;
                        int rowsAffected = Query.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            if (_hubConnection != null)
                            {
                                await _hubConnection.InvokeAsync("SendOrdersReturnPageUpdate");
                                await _hubConnection.StopAsync();
                            }

                            Query.Dispose();
                            conn.Close();
                            this.Close();
                        }
                    }
                }
            }

            else
            {
                MessageBox.Show("Mohon isi Pallet yang di Return! (Boleh 0 dulu)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void CancelButton_Clicked(object sender, RoutedEventArgs e)
        {
            _hubConnection?.StopAsync();
            this.Close();
        }
    }
}

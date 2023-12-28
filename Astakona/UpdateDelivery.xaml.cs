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
using System.Text.RegularExpressions;

namespace Astakona
{
    /// <summary>
    /// Interaction logic for UpdateDelivery.xaml
    /// </summary>
    public partial class UpdateDelivery : Window
    {
        public string connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString + ";MultipleActiveResultSets=True";
        private HubConnection _hubConnection;
        public OrdersDetails SelectedOrder;
        public string error;
        public UpdateDelivery(OrdersDetails selectedOrder)
        {
            InitializeComponent();
            SelectedOrder = selectedOrder;
            InitializeSignalR();
            LoadDeliveryDetails();
        }

        private async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                 .WithUrl("http://192.168.1.3:5210/Hubs")
                 .Build();

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            await _hubConnection.StartAsync();
        }

        public void LoadDeliveryDetails()
        {
            InvoiceNoTB.Text = this.SelectedOrder.InvoiceNo.ToString();
            NameTB.Text = this.SelectedOrder.InventoryName.ToString();
            CustomerTB.Text = this.SelectedOrder.Customer.ToString();
            DueDate.SelectedDate = this.SelectedOrder.DueDate;
            DueDate.DisplayDate = this.SelectedOrder.DueDate;

            ReadyStockTB.Text = Convert.ToString(this.SelectedOrder.HTEKS + this.SelectedOrder.HTBTS + this.SelectedOrder.HTKKS);
            AmountLB.Content = "/ " + Convert.ToString(this.SelectedOrder.Amount);
            DeliveredTB.Text = Convert.ToString(this.SelectedOrder.Delivered);
        }

        private void NumberTB_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private bool VerifyInput()
        {
            StringBuilder errors = new StringBuilder();
            if (!double.TryParse(DeliveredTB.Text, out double DValue))
                errors.AppendLine("Harap isi jumlah kirim! (boleh 0)\n");

            if (DValue > Convert.ToDouble(ReadyStockTB.Text))
                errors.AppendLine("Pengiriman tidak bisa dilakukan karena pallet yang ready tidak cukup!\n");

            if (errors.Length > 0)
            {
                this.error = errors.ToString();
                errors.Clear();
                return false;
            }

            else
                return true;
        }

        private async void UpdateButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (VerifyInput())
            {
                using (SqlConnection conn = new SqlConnection(this.connection))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            SqlCommand UpdateOrderQuery = new SqlCommand("UPDATE Orders SET Delivered=@Delivered WHERE OrderID=@OrderID ", conn, transaction);
                            UpdateOrderQuery.Parameters.Add("@Delivered", SqlDbType.Real).Value = Convert.ToDouble(DeliveredTB.Text);
                            UpdateOrderQuery.Parameters.Add("@OrderID", SqlDbType.Int).Value = this.SelectedOrder.OrderID;
                            int rowsAffected = UpdateOrderQuery.ExecuteNonQuery();
                            UpdateOrderQuery.Dispose();

                            if (rowsAffected > 0)
                            {
                                SqlCommand UpdateInventoryQuery = new SqlCommand("UPDATE Inventories SET Stock=Stock-@Delivered WHERE InventoryID=@InventoryID", conn, transaction);
                                UpdateInventoryQuery.Parameters.Add("@Delivered", SqlDbType.Real).Value = Convert.ToDouble(DeliveredTB.Text) - this.SelectedOrder.Delivered;
                                UpdateInventoryQuery.Parameters.Add("@InventoryID", SqlDbType.Int).Value = this.SelectedOrder.InventoryID;
                                UpdateInventoryQuery.ExecuteNonQuery();
                                UpdateInventoryQuery.Dispose();

                                transaction.Commit();

                                if (_hubConnection != null)
                                {
                                    await _hubConnection.InvokeAsync("SendOrdersPageUpdate");
                                    await _hubConnection.InvokeAsync("SendDeliveriesPageUpdate");
                                    await _hubConnection.InvokeAsync("SendPalletsPageUpdate");
                                    await _hubConnection.StopAsync();
                                }
                                
                                conn.Close();
                                this.Close();
                            }

                            else
                            {
                                Console.WriteLine("No rows were updated. Check your condition.");
                            }
                        }

                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error during transaction: {ex.Message}\n{ex.StackTrace}");
                            transaction.Rollback();
                        }
                    }
                }
            }

            else
            {
                MessageBox.Show(this.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.error = "";
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

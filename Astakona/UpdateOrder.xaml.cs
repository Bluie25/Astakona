using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace Astakona
{
    /// <summary>
    /// Interaction logic for UpdateOrder.xaml
    /// </summary>
    public partial class UpdateOrder : Window
    {
        public string connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString + ";MultipleActiveResultSets=True";
        private HubConnection _hubConnection;
        public OrdersDetails SelectedOrder;
        public double SelectedUnitBigScrew;
        public double SelectedUnitSmallScrew;
        string error;

        public UpdateOrder(OrdersDetails SelectedOrder)
        {
            InitializeComponent();
            this.SelectedOrder = SelectedOrder;
            LoadSelectedOrderDetails();
            InitializeSignalR();
        }

        private async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                 .WithUrl("http://192.168.1.3:5210/Hubs")
                 .Build();

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            await _hubConnection.StartAsync();
        }

        public void LoadSelectedOrderDetails()
        {
            using (SqlConnection conn = new SqlConnection(this.connection))
            {
                conn.Open();
                using (SqlCommand query = new SqlCommand("SELECT InventoryID, Name FROM Inventories", conn))
                {
                    using (SqlDataReader reader = query.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        ComboBox.ItemsSource = dataTable.DefaultView;
                        ComboBox.DisplayMemberPath = "Name";
                        ComboBox.SelectedValuePath = "InventoryID";
                        ComboBox.SelectedValue = this.SelectedOrder.InventoryID;
                    }
                } 
                conn.Close();
            }

            AmountTB.Text = this.SelectedOrder.Amount.ToString();
            ProductionTB.Text = this.SelectedOrder.ProductionCompleted.ToString();
            HTTB.Text = this.SelectedOrder.HeatCompleted.ToString();
            CustomerTB.Text = this.SelectedOrder.Customer.ToString();
            OrderDate.SelectedDate = this.SelectedOrder.Date;
            OrderDate.DisplayDate = this.SelectedOrder.Date;
            BigScrewTB.Text = this.SelectedOrder.BigScrew.ToString();
            SmallScrewTB.Text = this.SelectedOrder.SmallScrew.ToString();
            this.SelectedUnitBigScrew = this.SelectedOrder.BigScrew / this.SelectedOrder.Amount;
            this.SelectedUnitSmallScrew = this.SelectedOrder.SmallScrew / this.SelectedOrder.Amount;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox.SelectedItem != null)
            {
                DataRowView SelectedInventory = (DataRowView)ComboBox.SelectedItem;
                int SelectedInventoryID = (int)SelectedInventory["InventoryID"];

                using (SqlConnection conn = new SqlConnection(this.connection))
                {
                    conn.Open();
                    using (SqlCommand query = new SqlCommand("SELECT BigScrew, SmallScrew FROM Formulas WHERE InventoryID=@InventoryID", conn))
                    {
                        query.Parameters.Add("@InventoryID", SqlDbType.Int).Value = SelectedInventoryID;
                        using (SqlDataReader reader = query.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                this.SelectedUnitBigScrew = Convert.ToDouble(reader["BigScrew"]);
                                this.SelectedUnitSmallScrew = Convert.ToDouble(reader["SmallScrew"]);
                            }
                        }
                    }
                    conn.Close();
                    UpdateScrewValues();
                }
            }
        }

        private void AmountTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateScrewValues();
        }

        private void NumberTB_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void UpdateScrewValues()
        {
            if (ComboBox.SelectedItem != null && !string.IsNullOrEmpty(AmountTB.Text))
            {
                if (BigScrewTB != null && SmallScrewTB != null)
                {
                    BigScrewTB.Text = Convert.ToString(this.SelectedUnitBigScrew * Convert.ToDouble(AmountTB.Text));
                    SmallScrewTB.Text = Convert.ToString(this.SelectedUnitSmallScrew * Convert.ToDouble(AmountTB.Text));
                }
            }

            else
            {
                if (BigScrewTB != null && SmallScrewTB != null)
                {
                    BigScrewTB.Text = "0";
                    SmallScrewTB.Text = "0";
                }
            }
        }

        private bool VerifyInput()
        {
            StringBuilder errors = new StringBuilder();

            if (!double.TryParse(AmountTB.Text, out double OValue))
                errors.AppendLine("Harap isi jumlah order! (boleh 0)\n");

            if (!double.TryParse(ProductionTB.Text, out double PValue))
                errors.AppendLine("Harap isi jumlah yang sudah di produksi! (boleh 0)\n");

            if (!double.TryParse(HTTB.Text, out double HTValue))
                errors.AppendLine("Harap isi jumlah yang sudah di HT! (boleh 0)\n");

            if (string.IsNullOrEmpty(CustomerTB.Text))
                errors.AppendLine("Nama customer tidak boleh kosong!\n");

            if (OValue <= 0)
                errors.AppendLine("Jumlah order tidak boleh 0!\n");

            if (PValue > OValue)
                errors.AppendLine("Jumlah produksi melebihi jumlah order!\n");

            if (HTValue > PValue)
                errors.AppendLine("Jumlah yang sudah di HT melebihi jumlah yang sudah terproduksi!\n");

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
                DataRowView SelectedInventory = (DataRowView)ComboBox.SelectedItem;
                int SelectedInventoryID = (int)SelectedInventory["InventoryID"];
                string SelectedInventoryName = (string)SelectedInventory["Name"];

                using (SqlConnection conn = new SqlConnection(this.connection))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            double BigScrewStock = 0;
                            double SmallScrewStock = 0;
                            SqlCommand CheckScrewStockQuery = new SqlCommand("SELECT * FROM Screws", conn, transaction);
                            SqlDataReader CheckScrewStockReader = CheckScrewStockQuery.ExecuteReader();

                            while (CheckScrewStockReader.Read())
                            {
                                switch (Convert.ToInt32(CheckScrewStockReader["ScrewID"]))
                                {
                                    case 1:
                                        BigScrewStock = Convert.ToDouble(CheckScrewStockReader["Stock"]);
                                        break;
                                    case 2:
                                        SmallScrewStock = Convert.ToDouble(CheckScrewStockReader["Stock"]);
                                        break;
                                }
                            }

                            if ((Convert.ToDouble(ProductionTB.Text) * this.SelectedUnitBigScrew) > BigScrewStock)
                            {
                                error += "Stock paku besar (2 1/2 Inci) tidak cukup!\n";
                            }

                            if ((Convert.ToDouble(ProductionTB.Text) * this.SelectedUnitSmallScrew) > SmallScrewStock)
                            {
                                error += "Stock paku kecil (2 Inci) tidak cukup!\n";
                            }

                            if (!string.IsNullOrWhiteSpace(this.error))
                            {
                                MessageBox.Show(this.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                this.error = "";
                                return;
                            }

                            else
                            {
                                using (SqlCommand UpdateOrderQuery = new SqlCommand("UPDATE Orders SET " +
                                                                            "InventoryID=@InventoryID, " +
                                                                            "InventoryName=@InventoryName, " +
                                                                            "Amount=@Amount, " +
                                                                            "BigScrew=@BigScrew, " +
                                                                            "SmallScrew=@SmallScrew, " +
                                                                            "ProductionCompleted=@ProductionCompleted, " +
                                                                            "HeatCompleted=@HeatCompleted, " +
                                                                            "Date=@Date, " +
                                                                            "Customer=@Customer WHERE OrderID=@OrderID", conn, transaction))
                                {
                                    UpdateOrderQuery.Parameters.Add("@InventoryID", SqlDbType.Int).Value = SelectedInventoryID;
                                    UpdateOrderQuery.Parameters.Add("@InventoryName", SqlDbType.NVarChar).Value = SelectedInventoryName;
                                    UpdateOrderQuery.Parameters.Add("@Amount", SqlDbType.Real).Value = Convert.ToDouble(AmountTB.Text);
                                    UpdateOrderQuery.Parameters.Add("@BigScrew", SqlDbType.Real).Value = Convert.ToDouble(BigScrewTB.Text);
                                    UpdateOrderQuery.Parameters.Add("@SmallScrew", SqlDbType.Real).Value = Convert.ToDouble(SmallScrewTB.Text);
                                    UpdateOrderQuery.Parameters.Add("@ProductionCompleted", SqlDbType.Real).Value = Convert.ToDouble(ProductionTB.Text);
                                    UpdateOrderQuery.Parameters.Add("@HeatCompleted", SqlDbType.Real).Value = Convert.ToDouble(HTTB.Text);
                                    UpdateOrderQuery.Parameters.Add("@Date", SqlDbType.Date).Value = OrderDate.SelectedDate;
                                    UpdateOrderQuery.Parameters.Add("@Customer", SqlDbType.NVarChar).Value = Convert.ToString(CustomerTB.Text);
                                    UpdateOrderQuery.Parameters.Add("@OrderID", SqlDbType.Int).Value = this.SelectedOrder.OrderID;

                                    int rowsAffected = UpdateOrderQuery.ExecuteNonQuery();

                                    if (rowsAffected > 0)
                                    {
                                        SqlCommand SubtractScrewStockQuery = new SqlCommand("UPDATE Screws SET Stock = Stock - @BigScrew WHERE ScrewID = 1", conn, transaction);
                                        SubtractScrewStockQuery.Parameters.Add("@BigScrew", SqlDbType.Real).Value = this.SelectedUnitBigScrew * (Convert.ToDouble(ProductionTB.Text) - this.SelectedOrder.ProductionCompleted);
                                        SubtractScrewStockQuery.ExecuteNonQuery();

                                        SubtractScrewStockQuery = new SqlCommand("UPDATE Screws SET Stock = Stock - @SmallScrew WHERE ScrewID = 2", conn, transaction);
                                        SubtractScrewStockQuery.Parameters.Add("@SmallScrew", SqlDbType.Real).Value = this.SelectedUnitSmallScrew * (Convert.ToDouble(ProductionTB.Text) - this.SelectedOrder.ProductionCompleted);
                                        SubtractScrewStockQuery.ExecuteNonQuery();

                                        transaction.Commit();
                                        await _hubConnection.InvokeAsync("SendOrderUpdate");
                                        conn.Close();
                                        this.Close();
                                    }
                                    else
                                    {
                                        MessageBox.Show("Failed to update order.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
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
            }
        }

        private void CancelButton_Clicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

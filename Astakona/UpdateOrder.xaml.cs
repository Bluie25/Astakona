﻿using Microsoft.AspNetCore.SignalR.Client;
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
using static System.Net.WebRequestMethods;

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
        public string error;

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
                 .WithUrl("INSERT LOCAL IP ADDRESS")
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

            InvoiceNoTB.Text = this.SelectedOrder.InvoiceNo.ToString();
            ProductionTB.Text = this.SelectedOrder.ProductionCompleted.ToString();
            AmountTB.Text = this.SelectedOrder.Amount.ToString();
            CustomerTB.Text = this.SelectedOrder.Customer.ToString();
            ManufactureTeamTB.Text = this.SelectedOrder.ManufactureTeam.ToString();
            
            HTEKSTB.Text = this.SelectedOrder.HTEKS.ToString();
            HTBTSTB.Text = this.SelectedOrder.HTBTS.ToString();
            HTKKSTB.Text = this.SelectedOrder.HTKKS.ToString();

            OrderDate.SelectedDate = this.SelectedOrder.OrderDate;
            OrderDate.DisplayDate = this.SelectedOrder.OrderDate;
            DueDate.SelectedDate = this.SelectedOrder.DueDate; 
            DueDate.DisplayDate = this.SelectedOrder.DueDate;

            Triplek18mmTB.Text = this.SelectedOrder.Triplek18mm.ToString("F2");
            Triplek15mmTB.Text = this.SelectedOrder.Triplek15mm.ToString("F2");
            Triplek12mmTB.Text = this.SelectedOrder.Triplek12mm.ToString("F2");
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
                    using (SqlCommand query = new SqlCommand("SELECT * FROM Inventories WHERE InventoryID=@InventoryID", conn))
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
                    UpdateMaterialsValues();
                }
            }
        }

        private void AmountTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateMaterialsValues();
        }

        private void NumberTB_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (!char.IsDigit(e.Text[0]) && e.Text[0] != '.')
            {
                e.Handled = true;
                return;
            }

            if (e.Text[0] == '.' && textBox.Text.Contains('.'))
            {
                e.Handled = true;
                return;
            }
        }

        private void UpdateMaterialsValues()
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
            if (string.IsNullOrEmpty(InvoiceNoTB.Text))
                errors.AppendLine("Harap isi nomor Invoice!\n");

            if (string.IsNullOrEmpty(ManufactureTeamTB.Text))
                errors.AppendLine("Harap isi team yang memproduksi!\n");

            if (string.IsNullOrEmpty(CustomerTB.Text))
                errors.AppendLine("Nama customer tidak boleh kosong!\n");

            if (!double.TryParse(AmountTB.Text, out double OValue))
                errors.AppendLine("Harap isi jumlah order! (boleh 0)\n");

            if (!double.TryParse(ProductionTB.Text, out double PValue))
                errors.AppendLine("Harap isi jumlah yang sudah di produksi! (boleh 0)\n");

            if (!double.TryParse(HTEKSTB.Text, out double HTEKSValue))
                errors.AppendLine("Harap isi jumlah yang sudah di HT di EKS! (boleh 0)\n");

            if (!double.TryParse(HTBTSTB.Text, out double HTBTSValue))
                errors.AppendLine("Harap isi jumlah yang sudah di HT di BTS! (boleh 0)\n");

            if (!double.TryParse(HTKKSTB.Text, out double HTKKSValue))
                errors.AppendLine("Harap isi jumlah yang sudah di HT di KKS! (boleh 0)\n");

            if (!double.TryParse(Triplek18mmTB.Text, out double T18mm))
                errors.AppendLine("Harap isi jumlah triplek 18 mm yang terpakai! (boleh 0)\n");

            if (!double.TryParse(Triplek15mmTB.Text, out double T15mm))
                errors.AppendLine("Harap isi jumlah triplek 18 mm yang terpakai! (boleh 0)\n");

            if (!double.TryParse(Triplek12mmTB.Text, out double T12mm))
                errors.AppendLine("Harap isi jumlah triplek 18 mm yang terpakai! (boleh 0)\n");

            if (PValue > OValue)
                errors.AppendLine("Jumlah produksi melebihi jumlah order!\n");

            if ((HTEKSValue + HTBTSValue + HTKKSValue) > PValue)
                errors.AppendLine("Jumlah total yang sudah di HT melebihi jumlah yang sudah terproduksi!\n");

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
                            SqlCommand CheckMaterialStockQuery = new SqlCommand("SELECT * FROM Materials", conn, transaction);
                            SqlDataReader CheckMaterialStockReader = CheckMaterialStockQuery.ExecuteReader();

                            while (CheckMaterialStockReader.Read())
                            {
                                switch (Convert.ToInt32(CheckMaterialStockReader["MaterialID"]))
                                {
                                    case 1:
                                        if (((Convert.ToDouble(ProductionTB.Text) * this.SelectedUnitBigScrew) - (this.SelectedOrder.ProductionCompleted * this.SelectedUnitBigScrew)) > Convert.ToDouble(CheckMaterialStockReader["Stock"]))
                                            error += "Stock paku besar (2 1/2 Inci) tidak cukup!\n";
                                        break;
                                    case 2:
                                        if (((Convert.ToDouble(ProductionTB.Text) * this.SelectedUnitSmallScrew) - (this.SelectedOrder.ProductionCompleted * this.SelectedUnitSmallScrew)) > Convert.ToDouble(CheckMaterialStockReader["Stock"]))
                                            error += "Stock paku kecil (2 Inci) tidak cukup!\n";
                                        break;
                                    case 3:
                                        if ((Convert.ToDouble(Triplek18mmTB.Text) - this.SelectedOrder.Triplek18mm) > Convert.ToDouble(CheckMaterialStockReader["Stock"]))
                                            error += "Stock triplek 18 mm tidak cukup!\n";
                                        break;
                                    case 4:
                                        if ((Convert.ToDouble(Triplek15mmTB.Text) - this.SelectedOrder.Triplek15mm) > Convert.ToDouble(CheckMaterialStockReader["Stock"]))
                                            error += "Stock triplek 15 mm tidak cukup!\n";
                                        break;
                                    case 5:
                                        if ((Convert.ToDouble(Triplek12mmTB.Text) - this.SelectedOrder.Triplek12mm) > Convert.ToDouble(CheckMaterialStockReader["Stock"]))
                                            error += "Stock triplek 12 mm tidak cukup!\n";
                                        break;
                                }
                            }

                            if (string.IsNullOrWhiteSpace(this.error))
                            {
                                ////UPDATE ORDER DETAILS IN ORDERS TABLE
                                using (SqlCommand UpdateOrderQuery = new SqlCommand("UPDATE Orders SET " +
                                                                            "InventoryID=@InventoryID, " +
                                                                            "InventoryName=@InventoryName, " +
                                                                            "Amount=@Amount, " +
                                                                            "ProductionCompleted=@ProductionCompleted, " +
                                                                            "OrderDate=@OrderDate, " +
                                                                            "Customer=@Customer, " +
                                                                            "ManufactureTeam=@ManufactureTeam, " +
                                                                            "InvoiceNo=@InvoiceNo, " +
                                                                            "DueDate=@DueDate, " +
                                                                            "HTEKS=@HTEKS, " +
                                                                            "HTBTS=@HTBTS, " +
                                                                            "HTKKS=@HTKKS, " +
                                                                            "Triplek18mm=@Triplek18mm, " +
                                                                            "Triplek15mm=@Triplek12mm, " +
                                                                            "Triplek12mm=@Triplek12mm WHERE OrderID=@OrderID", conn, transaction))

                                {
                                    UpdateOrderQuery.Parameters.Add("@InventoryID", SqlDbType.Int).Value = SelectedInventoryID;
                                    UpdateOrderQuery.Parameters.Add("@InventoryName", SqlDbType.NVarChar).Value = SelectedInventoryName;
                                    UpdateOrderQuery.Parameters.Add("@Amount", SqlDbType.Real).Value = Convert.ToDouble(AmountTB.Text);
                                    UpdateOrderQuery.Parameters.Add("@ProductionCompleted", SqlDbType.Real).Value = Convert.ToDouble(ProductionTB.Text);
                                    UpdateOrderQuery.Parameters.Add("@OrderDate", SqlDbType.Date).Value = OrderDate.SelectedDate;
                                    UpdateOrderQuery.Parameters.Add("@Customer", SqlDbType.NVarChar).Value = Convert.ToString(CustomerTB.Text);
                                    UpdateOrderQuery.Parameters.Add("@ManufactureTeam", SqlDbType.NVarChar).Value = ManufactureTeamTB.Text.ToString();
                                    UpdateOrderQuery.Parameters.Add("@InvoiceNo", SqlDbType.NVarChar).Value = InvoiceNoTB.Text.ToString();
                                    UpdateOrderQuery.Parameters.Add("@DueDate", SqlDbType.Date).Value = DueDate.SelectedDate;
                                    UpdateOrderQuery.Parameters.Add("@HTEKS", SqlDbType.Real).Value = Convert.ToDouble(HTEKSTB.Text);
                                    UpdateOrderQuery.Parameters.Add("@HTBTS", SqlDbType.Real).Value = Convert.ToDouble(HTBTSTB.Text);
                                    UpdateOrderQuery.Parameters.Add("@HTKKS", SqlDbType.Real).Value = Convert.ToDouble(HTKKSTB.Text);
                                    UpdateOrderQuery.Parameters.Add("@Triplek18mm", SqlDbType.Real).Value = Convert.ToDouble(Triplek18mmTB.Text);
                                    UpdateOrderQuery.Parameters.Add("@Triplek15mm", SqlDbType.Real).Value = Convert.ToDouble(Triplek15mmTB.Text);
                                    UpdateOrderQuery.Parameters.Add("@Triplek12mm", SqlDbType.Real).Value = Convert.ToDouble(Triplek12mmTB.Text);
                                    UpdateOrderQuery.Parameters.Add("@OrderID", SqlDbType.Int).Value = this.SelectedOrder.OrderID;


                                    int rowsAffected = UpdateOrderQuery.ExecuteNonQuery();

                                    if (rowsAffected > 0)
                                    {
                                        UpdateOrderQuery.Dispose();

                                        ////UPDATE USED MATERIALS FOR PRODUCTION IN MATERIALS TABLE
                                        SqlCommand UpdateMaterialStockQuery = new SqlCommand(@"UPDATE Materials SET Stock = 
                                                                                                CASE 
                                                                                                    WHEN MaterialID = 1 THEN Stock - @UsedMaterials1 
                                                                                                    WHEN MaterialID = 2 THEN Stock - @UsedMaterials2 
                                                                                                    WHEN MaterialID = 3 THEN Stock - @UsedMaterials3 
                                                                                                    WHEN MaterialID = 4 THEN Stock - @UsedMaterials4 
                                                                                                    WHEN MaterialID = 5 THEN Stock - @UsedMaterials5 
                                                                                                ELSE Stock 
                                                                                            END 
                                                                                        WHERE MaterialID IN (1, 2, 3, 4, 5)", conn, transaction);

                                        UpdateMaterialStockQuery.Parameters.Add("@UsedMaterials1", SqlDbType.Real).Value = this.SelectedUnitBigScrew * (Convert.ToDouble(ProductionTB.Text) - this.SelectedOrder.ProductionCompleted);
                                        UpdateMaterialStockQuery.Parameters.Add("@UsedMaterials2", SqlDbType.Real).Value = this.SelectedUnitSmallScrew * (Convert.ToDouble(ProductionTB.Text) - this.SelectedOrder.ProductionCompleted);
                                        UpdateMaterialStockQuery.Parameters.Add("@UsedMaterials3", SqlDbType.Real).Value = (Convert.ToDouble(Triplek18mmTB.Text) - this.SelectedOrder.Triplek18mm);
                                        UpdateMaterialStockQuery.Parameters.Add("@UsedMaterials4", SqlDbType.Real).Value = (Convert.ToDouble(Triplek15mmTB.Text) - this.SelectedOrder.Triplek15mm);
                                        UpdateMaterialStockQuery.Parameters.Add("@UsedMaterials5", SqlDbType.Real).Value = (Convert.ToDouble(Triplek12mmTB.Text) - this.SelectedOrder.Triplek12mm);
                                        UpdateMaterialStockQuery.ExecuteNonQuery();
                                        UpdateMaterialStockQuery.Dispose();

                                        ////UPDATE PALLETS STOCK IN INVENTORIES TABLE
                                        SqlCommand UpdateInventoriesStockQuery = new SqlCommand("UPDATE Inventories SET Stock = Stock + @ProducedPallet WHERE InventoryID = @SelectedPalletID", conn, transaction);
                                        UpdateInventoriesStockQuery.Parameters.Add("@ProducedPallet", SqlDbType.Real).Value = Convert.ToDouble(ProductionTB.Text) - this.SelectedOrder.ProductionCompleted;
                                        UpdateInventoriesStockQuery.Parameters.Add("@SelectedPalletID", SqlDbType.Int).Value = SelectedInventoryID;
                                        UpdateInventoriesStockQuery.ExecuteNonQuery();
                                        UpdateInventoriesStockQuery.Dispose();

                                        ////UPDATE RETURNED DETAILS IF HAVE
                                        SqlCommand UpdateReturnedOrdersQuery = new SqlCommand("UPDATE ReturnedOrders SET " +
                                                                                                "InvoiceNo=@InvoiceNo, " +
                                                                                                "InventoryName=@InventoryName, " +
                                                                                                "Amount=@Amount, " +
                                                                                                "ProductionCompleted=@ProductionCompleted, " +
                                                                                                "HTCompleted=@HTCompleted, " +
                                                                                                "Customer=@Customer WHERE OrderID=@OrderID", conn, transaction);
                                        UpdateReturnedOrdersQuery.Parameters.Add("@InvoiceNo", SqlDbType.NVarChar).Value = InvoiceNoTB.Text;
                                        UpdateReturnedOrdersQuery.Parameters.Add("@InventoryName", SqlDbType.NVarChar).Value = SelectedInventoryName;
                                        UpdateReturnedOrdersQuery.Parameters.Add("@Amount", SqlDbType.Real).Value = Convert.ToDouble(AmountTB.Text);
                                        UpdateReturnedOrdersQuery.Parameters.Add("@ProductionCompleted", SqlDbType.Real).Value = Convert.ToDouble(ProductionTB.Text);
                                        UpdateReturnedOrdersQuery.Parameters.Add("@HTCompleted", SqlDbType.Real).Value = Convert.ToDouble(HTBTSTB.Text) + Convert.ToDouble(HTEKSTB.Text) + Convert.ToDouble(HTKKSTB.Text);
                                        UpdateReturnedOrdersQuery.Parameters.Add("@Customer", SqlDbType.NVarChar).Value = CustomerTB.Text;
                                        UpdateReturnedOrdersQuery.Parameters.Add("@OrderID", SqlDbType.Int).Value = this.SelectedOrder.OrderID;
                                        UpdateReturnedOrdersQuery.ExecuteNonQuery();
                                        UpdateReturnedOrdersQuery.Dispose();

                                        transaction.Commit();

                                        if (_hubConnection != null)
                                        {
                                            await _hubConnection.InvokeAsync("SendOrdersPageUpdate");
                                            await _hubConnection.InvokeAsync("SendMaterialsPageUpdate");
                                            await _hubConnection.InvokeAsync("SendDeliveriesPageUpdate");
                                            await _hubConnection.InvokeAsync("SendPalletsPageUpdate");
                                            await _hubConnection.InvokeAsync("SendOrdersReturnPageUpdate");
                                            await _hubConnection.StopAsync();
                                        }

                                        conn.Close();
                                        this.Close();
                                    }

                                    else
                                    {
                                        MessageBox.Show("Failed to update order.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error during transaction: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            _hubConnection?.StopAsync();
            this.Close();
        }

        private void Triplek18mmTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }
    }
}

using Microsoft.AspNetCore.SignalR.Client;
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
using System.Configuration;
using System.Collections;

namespace Astakona
{
    /// <summary>
    /// Interaction logic for UpdateReturnedOrder.xaml
    /// </summary>
    public partial class UpdateReturnedOrder : Window
    {
        public string connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
        private HubConnection _hubConnection;
        private ReturnedOrdersDetails SelectedReturnedOrder;
        private string error;
        public UpdateReturnedOrder(ReturnedOrdersDetails selectedReturnedOrder)
        {
            InitializeComponent();
            this.SelectedReturnedOrder = selectedReturnedOrder;
            InitializeSignalR();
            LoadReturnedOrderDetails();
        }

        private async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                 .WithUrl("INSERT LOCAL IP ADDRESS")
                 .Build();

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            await _hubConnection.StartAsync();
        }

        private void LoadReturnedOrderDetails()
        {
            InvoiceNoTB.Text = this.SelectedReturnedOrder.InvoiceNo;
            InventoryNameTB.Text = this.SelectedReturnedOrder.InventoryName;
            OrderProgressTB.Text = this.SelectedReturnedOrder.Delivered.ToString() + " / " + this.SelectedReturnedOrder.Amount.ToString();
            CustomerTB.Text = this.SelectedReturnedOrder.Customer;
            ReturnedAmountTB.Text = this.SelectedReturnedOrder.ReturnAmount.ToString();
            FixedPalletTB.Text = this.SelectedReturnedOrder.PalletFixed.ToString();
            BigScrewTB.Text = this.SelectedReturnedOrder.BigScrewUsed.ToString();
            SmallScrewTB.Text = this.SelectedReturnedOrder.SmallScrewUsed.ToString();
            Triplek12mmTB.Text = this.SelectedReturnedOrder.Triplek12mmUsed.ToString();
            Triplek15mmTB.Text = this.SelectedReturnedOrder.Triplek15mmUsed.ToString();
            Triplek18mmTB.Text = this.SelectedReturnedOrder.Triplek18mmUsed.ToString();
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

        private bool VerifyInput()
        {
            StringBuilder errors = new StringBuilder();
            if (!double.TryParse(ReturnedAmountTB.Text, out double RA))
                errors.AppendLine("Harap isi jumlah yang di return!\n");

            if (!double.TryParse(FixedPalletTB.Text, out double FP))
                errors.AppendLine("Harap isi jumlah pallet yagn sudah diperbaiki! (boleh 0)\n");

            if (string.IsNullOrEmpty(BigScrewTB.Text))
                errors.AppendLine("Harap isi jumlah paku besar yang terpakai untuk perbaikan! (boleh 0)\n");

            if (string.IsNullOrEmpty(SmallScrewTB.Text))
                errors.AppendLine("Harap isi jumlah paku kecil yang terpakai untuk perbaikan! (boleh 0)\n");

            if (string.IsNullOrEmpty(Triplek12mmTB.Text))
                errors.AppendLine("Harap isi jumlah triplek 12 mm yang terpakai untuk perbaikan! (boleh 0)\n");
            
            if (string.IsNullOrEmpty(Triplek15mmTB.Text))
                errors.AppendLine("Harap isi jumlah triplek 15 mm yang terpakai untuk perbaikan! (boleh 0)\n");

            if (string.IsNullOrEmpty(Triplek18mmTB.Text))
                errors.AppendLine("Harap isi jumlah triplek 18 mm yang terpakai untuk perbaikan! (boleh 0)\n");

            if(FP > RA)
                errors.AppendLine("Pallet yang diperbaiki melebihi jumlah return!\n");

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
                            SqlCommand CheckMaterialStockQuery = new SqlCommand("SELECT * FROM Materials", conn, transaction);
                            SqlDataReader CheckMaterialStockReader = CheckMaterialStockQuery.ExecuteReader();

                            while (CheckMaterialStockReader.Read())
                            {
                                switch (Convert.ToInt32(CheckMaterialStockReader["MaterialID"]))
                                {
                                    case 1:
                                        if (((Convert.ToDouble(BigScrewTB.Text) - this.SelectedReturnedOrder.BigScrewUsed) > Convert.ToDouble(CheckMaterialStockReader["Stock"])))
                                            error += "Stock paku besar (2 1/2 Inci) tidak cukup!\n";
                                        break;
                                    case 2:
                                        if (((Convert.ToDouble(SmallScrewTB.Text) - this.SelectedReturnedOrder.SmallScrewUsed) > Convert.ToDouble(CheckMaterialStockReader["Stock"])))
                                            error += "Stock paku kecil (2 Inci) tidak cukup!\n";
                                        break;
                                    case 3:
                                        if ((Convert.ToDouble(Triplek18mmTB.Text) - this.SelectedReturnedOrder.Triplek18mmUsed) > Convert.ToDouble(CheckMaterialStockReader["Stock"]))
                                            error += "Stock triplek 18 mm tidak cukup!\n";
                                        break;
                                    case 4:
                                        if ((Convert.ToDouble(Triplek15mmTB.Text) - this.SelectedReturnedOrder.Triplek15mmUsed) > Convert.ToDouble(CheckMaterialStockReader["Stock"]))
                                            error += "Stock triplek 15 mm tidak cukup!\n";
                                        break;
                                    case 5:
                                        if ((Convert.ToDouble(Triplek12mmTB.Text) - this.SelectedReturnedOrder.Triplek12mmUsed) > Convert.ToDouble(CheckMaterialStockReader["Stock"]))
                                            error += "Stock triplek 12 mm tidak cukup!\n";
                                        break;
                                }
                            }
                            CheckMaterialStockReader.Close();

                            if (string.IsNullOrWhiteSpace(this.error))
                            {
                                ////UPDATE RETURNED ORDERS DETAILS
                                using (SqlCommand UpdateReturnedOrdersQuery = new SqlCommand("UPDATE ReturnedOrders SET " +
                                                                                                        "ReturnedAmount=@ReturnedAmount ," +
                                                                                                        "PalletFixed=@PalletFixed, " +
                                                                                                        "BigScrewUsed=@BigScrewUsed, " +
                                                                                                        "SmallScrewUsed=@SmallScrewUsed, " +
                                                                                                        "Triplek18mmUsed=@Triplek18mmUsed, " +
                                                                                                        "Triplek15mmUsed=@Triplek15mmUsed, " +
                                                                                                        "Triplek12mmUsed=@Triplek12mmUsed, " +
                                                                                                        "IsFinished=@IsFinished WHERE ReturnedOrderID=@ReturnedOrderID", conn, transaction))
                                {
                                    UpdateReturnedOrdersQuery.Parameters.Add("@ReturnedAmount", SqlDbType.Real).Value = Convert.ToDouble(ReturnedAmountTB.Text);
                                    UpdateReturnedOrdersQuery.Parameters.Add("@PalletFixed", SqlDbType.Real).Value = Convert.ToDouble(FixedPalletTB.Text);
                                    UpdateReturnedOrdersQuery.Parameters.Add("@BigScrewUsed", SqlDbType.Real).Value = Convert.ToDouble(BigScrewTB.Text);
                                    UpdateReturnedOrdersQuery.Parameters.Add("@SmallScrewUsed", SqlDbType.Real).Value = Convert.ToDouble(SmallScrewTB.Text);
                                    UpdateReturnedOrdersQuery.Parameters.Add("@Triplek18mmUsed", SqlDbType.Real).Value = Convert.ToDouble(Triplek18mmTB.Text);
                                    UpdateReturnedOrdersQuery.Parameters.Add("@Triplek15mmUsed", SqlDbType.Real).Value = Convert.ToDouble(Triplek15mmTB.Text);
                                    UpdateReturnedOrdersQuery.Parameters.Add("@Triplek12mmUsed", SqlDbType.Real).Value = Convert.ToDouble(Triplek12mmTB.Text);
                                    UpdateReturnedOrdersQuery.Parameters.Add("@ReturnedOrderID", SqlDbType.Int).Value = this.SelectedReturnedOrder.ReturnedOrderID;
                                    if (Convert.ToDouble(FixedPalletTB.Text) == Convert.ToDouble(ReturnedAmountTB.Text))
                                    {
                                        UpdateReturnedOrdersQuery.Parameters.Add("@IsFinished", SqlDbType.Bit).Value = 1;
                                        MessageBox.Show("Perbaikan return untuk Invoice " + this.SelectedReturnedOrder.InvoiceNo + " telah terselesaikan.", "Completed", MessageBoxButton.OK);
                                    }

                                    else
                                        UpdateReturnedOrdersQuery.Parameters.Add("@IsFinished", SqlDbType.Bit).Value = 0;
                                    int rowsAffected = UpdateReturnedOrdersQuery.ExecuteNonQuery();
                                    UpdateReturnedOrdersQuery.Dispose();

                                    if (rowsAffected > 0)
                                    {
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

                                        UpdateMaterialStockQuery.Parameters.Add("@UsedMaterials1", SqlDbType.Real).Value = Convert.ToDouble(BigScrewTB.Text) - this.SelectedReturnedOrder.BigScrewUsed;
                                        UpdateMaterialStockQuery.Parameters.Add("@UsedMaterials2", SqlDbType.Real).Value = Convert.ToDouble(SmallScrewTB.Text) - this.SelectedReturnedOrder.SmallScrewUsed;
                                        UpdateMaterialStockQuery.Parameters.Add("@UsedMaterials3", SqlDbType.Real).Value = Convert.ToDouble(Triplek18mmTB.Text) - this.SelectedReturnedOrder.Triplek18mmUsed;
                                        UpdateMaterialStockQuery.Parameters.Add("@UsedMaterials4", SqlDbType.Real).Value = Convert.ToDouble(Triplek15mmTB.Text) - this.SelectedReturnedOrder.Triplek15mmUsed;
                                        UpdateMaterialStockQuery.Parameters.Add("@UsedMaterials5", SqlDbType.Real).Value = Convert.ToDouble(Triplek12mmTB.Text) - this.SelectedReturnedOrder.Triplek12mmUsed;
                                        UpdateMaterialStockQuery.ExecuteNonQuery();
                                        UpdateMaterialStockQuery.Dispose();

                                        ////UPDATE PALLETS STOCK IN INVENTORIES TABLE (ON HOLD)
                                        /*SqlCommand UpdateInventoriesStockQuery = new SqlCommand("UPDATE Inventories SET Stock=Stock+@FixedPallet WHERE InventoryID=@SelectedPalletID", conn, transaction);
                                        if(Convert.ToDouble(FixedPalletTB.Text) != Convert.ToDouble(ReturnedAmountTB.Text))
                                            UpdateInventoriesStockQuery.Parameters.Add("@FixedPallet", SqlDbType.Real).Value = Convert.ToDouble(FixedPalletTB.Text) - this.SelectedReturnedOrder.PalletFixed;
                                        else
                                            UpdateInventoriesStockQuery.Parameters.Add("@FixedPallet", SqlDbType.Real).Value = - this.SelectedReturnedOrder.PalletFixed;
                                        UpdateInventoriesStockQuery.Parameters.Add("@SelectedPalletID", SqlDbType.Int).Value = this.SelectedReturnedOrder.InventoryID;
                                        UpdateInventoriesStockQuery.ExecuteNonQuery();
                                        UpdateInventoriesStockQuery.Dispose();*/

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
    }
}

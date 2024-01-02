using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Configuration;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;

namespace Astakona
{
    /// <summary>
    /// Interaction logic for PalletUpdate.xaml
    /// </summary>
    public partial class UpdatePallet : Window
    {
        public string connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString + ";MultipleActiveResultSets=True";
        private HubConnection _hubConnection;
        public PalletDetails SeletedPallet;
        public string error;

        public UpdatePallet(PalletDetails seletedPallet)
        {
            InitializeComponent();
            this.SeletedPallet = seletedPallet;
            LoadSelectedPalletDetails();
            InitializeSignalR();
        }

        public void LoadSelectedPalletDetails()
        {
            NameTB.Text = this.SeletedPallet.Name;
            StockTB.Text = this.SeletedPallet.Stock.ToString();
            BigScrewTB.Text = this.SeletedPallet.BigScrew.ToString();
            SmallScrewTB.Text = this.SeletedPallet.SmallScrew.ToString();
        }


        private async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                 .WithUrl("http://192.168.1.26:5210/Hubs")
                 .Build();

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            await _hubConnection.StartAsync();
        }

        private void NumberTB_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
       
        private bool VerifyInput()
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrEmpty(NameTB.Text))
                errors.AppendLine("Harap isi nama pallet!\n");

            if (string.IsNullOrEmpty(StockTB.Text))
                errors.AppendLine("Harap isi stock! (boleh 0)\n");

            if (string.IsNullOrEmpty(BigScrewTB.Text))
                errors.AppendLine("Harap isi paku besarnya! (boleh 0)\n");

            if (string.IsNullOrEmpty(SmallScrewTB.Text))
                errors.AppendLine("Harap isi paku kecilnya! (boleh 0)\n");

            if (errors.Length > 0)
            {
                this.error = errors.ToString();
                errors.Clear();
                return false;
            }

            else
                return true;
        }

        private async void UpdateButtonClick(object sender, RoutedEventArgs e)
        {
            if (VerifyInput())
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(this.connection))
                    {
                        conn.Open();

                        using (SqlCommand Query = new SqlCommand("UPDATE Inventories SET " +
                                                                "Name=@Name, " +
                                                                "Stock=@Stock, " +
                                                                "BigScrew=@BigScrew, " +
                                                                "SmallScrew=@SmallScrew WHERE InventoryID=@InventoryID", conn))
                        {
                            Query.Parameters.Add("@Name", SqlDbType.NVarChar).Value = NameTB.Text;
                            Query.Parameters.Add("@Stock", SqlDbType.Int).Value = Convert.ToDouble(StockTB.Text);
                            Query.Parameters.Add("@BigScrew", SqlDbType.Int).Value = Convert.ToDouble(BigScrewTB.Text);
                            Query.Parameters.Add("@SmallScrew", SqlDbType.Int).Value = Convert.ToDouble(SmallScrewTB.Text);
                            Query.Parameters.Add("@InventoryID", SqlDbType.Int).Value = this.SeletedPallet.InventoryID;

                            Query.ExecuteNonQuery();

                            if (_hubConnection != null)
                            {
                                await _hubConnection.InvokeAsync("SendPalletsPageUpdate");
                                await _hubConnection.StopAsync();
                            }

                            conn.Close();
                            this.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            else
            {
                MessageBox.Show(this.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.error = "";
            }
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            _hubConnection?.StopAsync();
            this.Close();
        }
    }
}

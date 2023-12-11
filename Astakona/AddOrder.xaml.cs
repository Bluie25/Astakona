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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Collections;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;
using static System.Net.WebRequestMethods;


namespace Astakona
{
    /// <summary>
    /// Interaction logic for AddOrder.xaml
    /// </summary>
    public partial class AddOrder : Window
    {
        public double SelectedBigScrew;
        public double SelectedSmallScrew;
        public string connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
        private HubConnection _hubConnection;
        public string error;

        public AddOrder()
        {
            InitializeComponent();
            LoadProducts();
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
        private void LoadProducts()
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
                    }
                }
                conn.Close();
            }
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
                            while(reader.Read())
                            {
                                this.SelectedBigScrew = Convert.ToDouble(reader["BigScrew"]);
                                this.SelectedSmallScrew = Convert.ToDouble(reader["SmallScrew"]);
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

        private void UpdateScrewValues()
        {
            if (ComboBox.SelectedItem != null && !string.IsNullOrEmpty(AmountTB.Text))
            {
                if (BigScrewTB != null && SmallScrewTB != null)
                {
                    BigScrewTB.Text = Convert.ToString(this.SelectedBigScrew * Convert.ToDouble(AmountTB.Text));
                    SmallScrewTB.Text = Convert.ToString(this.SelectedSmallScrew * Convert.ToDouble(AmountTB.Text));
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

        private void AmountTB_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private bool ValidateInputs()
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrEmpty(InvoiceNoTB.Text))
                errors.AppendLine("Harap isi nomor Invoice!\n");

            if(ComboBox.SelectedItem == null)
                errors.AppendLine("Harap pilih item order!\n");

            if (string.IsNullOrEmpty(ManufactureTeamTB.Text))
                errors.AppendLine("Harap isi team yang memproduksi!\n");

            if(string.IsNullOrEmpty(AmountTB.Text))
                errors.AppendLine("Harap isi jumlah order! (boleh 0)\n");

            if (string.IsNullOrEmpty(CustomerTB.Text))
                errors.AppendLine("Nama customer tidak boleh kosong!\n");

            if (errors.Length > 0)
            {
                this.error = errors.ToString();
                errors.Clear();
                return false;
            }

            else
                return true;
        }
   

        private async void AddButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (ValidateInputs())
            {
                DataRowView SelectedInventory = (DataRowView)ComboBox.SelectedItem;
                int SelectedInventoryID = (int)SelectedInventory["InventoryID"];
                string SelectedInventoryName = (string)SelectedInventory["Name"];

                using (SqlConnection conn = new SqlConnection(this.connection))
                {
                    conn.Open();
                    using (SqlCommand query = new SqlCommand("INSERT INTO Orders (InventoryID, InvoiceNo, InventoryName, Amount, BigScrew, SmallScrew, ProductionCompleted, HeatCompleted, Customer, OrderDate, DueDate, ManufactureTeam) " +
                                                             "VALUES (@InventoryID, @InvoiceNo, @InventoryName, @Amount, @BigScrew, @SmallScrew, @ProductionCompleted, @HeatCompleted, @Customer, @OrderDate, @DueDate, @ManufactureTeam)", conn))
                    {
                        query.Parameters.Add("@InventoryID", SqlDbType.Int).Value = SelectedInventoryID;
                        query.Parameters.Add("@InvoiceNo", SqlDbType.NVarChar).Value = InvoiceNoTB.Text;
                        query.Parameters.Add("@InventoryName", SqlDbType.NVarChar).Value = SelectedInventoryName;
                        query.Parameters.Add("@Amount", SqlDbType.Real).Value = Convert.ToDouble(AmountTB.Text);
                        query.Parameters.Add("@BigScrew", SqlDbType.Real).Value = Convert.ToDouble(BigScrewTB.Text);
                        query.Parameters.Add("@SmallScrew", SqlDbType.Real).Value = Convert.ToDouble(SmallScrewTB.Text);
                        query.Parameters.Add("@ProductionCompleted", SqlDbType.Real).Value = 0;
                        query.Parameters.Add("@HeatCompleted", SqlDbType.Real).Value = 0;
                        query.Parameters.Add("@OrderDate", SqlDbType.Date).Value = OrderDate.SelectedDate;
                        query.Parameters.Add("@DueDate", SqlDbType.Date).Value = DueDate.SelectedDate;
                        query.Parameters.Add("@ManufactureTeam", SqlDbType.NVarChar).Value = ManufactureTeamTB.Text;
                        query.Parameters.Add("@Customer", SqlDbType.NVarChar).Value = Convert.ToString(CustomerTB.Text);

                        int rowsAffected = query.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        { 
                            await _hubConnection.InvokeAsync("SendOrderEntry");
                            conn.Close();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Failed to add order.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

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
using System.Text.RegularExpressions;

namespace Astakona
{
    /// <summary>
    /// Interaction logic for UpdateScrewStock.xaml
    /// </summary>
    public partial class UpdateScrewStock : Window
    {
        public string connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
        private HubConnection _hubConnection;
        public int MaterialID;

        public UpdateScrewStock(int MaterialID)
        {
            InitializeComponent();
            this.MaterialID = MaterialID;
            LoadMaterialsStock();
            InitializeSignalR();
        }
        private async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                 .WithUrl("http://192.168.1.26:5210/Hubs")
                 .Build();

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            await _hubConnection.StartAsync();
        }

        public void LoadMaterialsStock()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    conn.Open();
                    SqlCommand query = new SqlCommand("SELECT * FROM Materials WHERE MaterialID=@MaterialID", conn);
                    query.Parameters.Add("@MaterialID", SqlDbType.Int).Value = this.MaterialID;
                    SqlDataReader reader = query.ExecuteReader();

                    while (reader.Read())
                    {
                        ScrewLB.Content = Convert.ToString(reader["Name"]);
                        ScrewTB.Text = Convert.ToString(reader["Stock"]);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void MaterialTB_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
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

        private async void UpdateButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!double.TryParse(ScrewTB.Text, out double stockValue))
                {
                    MessageBox.Show("Mohon memasukan angka! (boleh 0)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                else
                {
                    using (SqlConnection conn = new SqlConnection(this.connection))
                    {
                        conn.Open();
                        using (SqlCommand query = new SqlCommand("UPDATE Materials SET Stock=@Stock WHERE MaterialID=@MaterialID", conn))
                        {
                            query.Parameters.Add("@Stock", SqlDbType.Real).Value = Convert.ToDouble(ScrewTB.Text);
                            query.Parameters.Add("@MaterialID", SqlDbType.Int).Value = this.MaterialID;
                            int rowsAffected = query.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                if(_hubConnection != null)
                                {
                                    await _hubConnection.InvokeAsync("SendOrdersPageUpdate");
                                    await _hubConnection.InvokeAsync("SendMaterialsPageUpdate");
                                    await _hubConnection.StopAsync();
                                }
                                
                                conn.Close();
                                this.Close();
                            }

                            else
                                MessageBox.Show("Failed to add order.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void CancelButton_Clicked(object sender, RoutedEventArgs e)
        {
            _hubConnection?.StopAsync();
            this.Close();
        }
    }
}

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
        public int ScrewID;

        public UpdateScrewStock(int screwID)
        {
            InitializeComponent();
            this.ScrewID = screwID;
            LoadScrewsStock();
        }


        public void LoadScrewsStock()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    conn.Open();
                    SqlCommand query = new SqlCommand("SELECT * FROM Screws WHERE ScrewID=@ScrewID", conn);
                    query.Parameters.Add("@ScrewID", SqlDbType.Int).Value = this.ScrewID;
                    SqlDataReader reader = query.ExecuteReader();

                    while (reader.Read())
                    {
                        ScrewTB.Text = Convert.ToString(reader["Stock"]);
                        ScrewLB.Content = Convert.ToString(reader["Name"]);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ScrewTB_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
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

                using (SqlConnection conn = new SqlConnection(this.connection))
                {
                    conn.Open();
                    using (SqlCommand query = new SqlCommand("UPDATE Screws SET Stock=@Stock WHERE ScrewID=@ScrewID", conn))
                    {
                        query.Parameters.Add("@Stock", SqlDbType.Int).Value = Convert.ToDouble(ScrewTB.Text);
                        query.Parameters.Add("@ScrewID", SqlDbType.Int).Value = this.ScrewID;
                        int rowsAffected = query.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            await _hubConnection.InvokeAsync("SendScrewUpdate");
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void CancelButton_Clicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

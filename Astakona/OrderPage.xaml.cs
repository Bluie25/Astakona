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

namespace Astakona
{
    /// <summary>
    /// Interaction logic for OrderPage.xaml
    /// </summary>
    public partial class OrderPage : Window
    {
        public string connection = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
        public List<OrdersDetails> Orders { get; set; }
        public OrderPage()
        {
            InitializeComponent();
            Orders = new List<OrdersDetails>();
            LoadOrders();
            DataContext = this;
        }

        public void LoadOrders()
        {
            try
            {
                using(SqlConnection conn = new SqlConnection(connection))
                {
                    conn.Open();
                    SqlCommand query = new SqlCommand("SELECT * FROM Orders", conn);
                    SqlDataReader reader = query.ExecuteReader();

                    while (reader.Read())
                    {
                        this.Orders.Add(new OrdersDetails()
                        {
                            OrderID = Convert.ToInt32(reader["OrderID"]),
                            InventoryID = Convert.ToInt32(reader["InventoryID"]),
                            InventoryName = Convert.ToString(reader["InventoryName"]),
                            Amount = Convert.ToDouble(reader["Amount"]),
                            BigScrew = Convert.ToDouble(reader["BigScrew"]),
                            SmallScrew = Convert.ToDouble(reader["SmallScrew"]),
                            ProductionCompleted = Convert.ToDouble(reader["ProductionCompleted"]),
                            HeatCompleted = Convert.ToDouble(reader["HeatCompleted"]),
                            Date = reader.GetDateTime(8),
                            Customer = Convert.ToString(reader["Customer"]),
                        });  
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}\n{ex.StackTrace}");
            }
        }
        private void HomePageButton_Click(object sender, RoutedEventArgs e)
        {
            Dashboard Dashboard = new Dashboard();
            this.Close();
            Dashboard.Show();
        }

        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            AddOrder addOrder = new AddOrder();
            addOrder.Show();
        }
    }
}

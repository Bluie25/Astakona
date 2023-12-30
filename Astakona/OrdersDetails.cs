using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using System.Windows.Controls;
using System.Windows;

namespace Astakona
{
    [Serializable]
    public class OrdersDetails
    {
        public int OrderID { get; set; }
        public string InvoiceNo { get; set; }
        public int InventoryID { get; set; }
        public string InventoryName { get; set; }
        public double Amount { get; set; }
        public double ProductionCompleted { get; set; }
        public double HTBTS { get; set; }
        public double HTEKS { get; set; }
        public double HTKKS { get; set; }
        public double TotalHeatCompleted { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Customer { get; set; }
        public string ManufactureTeam { get; set; }
        public bool IsFinished { get; set; }
        public double Delivered { get; set; }
        public double Triplek18mm { get; set; }
        public double Triplek15mm { get; set; }
        public double Triplek12mm { get; set; }
    }
}

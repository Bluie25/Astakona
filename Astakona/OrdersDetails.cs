using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astakona
{
    [Serializable]
    public class OrdersDetails
    {
        public int OrderID { get; set; }
        public int InventoryID { get; set; }
        public string InvoiceNo { get; set; }
        public string InventoryName { get; set; }
        public double Amount { get; set; }
        public double BigScrew { get; set; }
        public double SmallScrew { get; set; }
        public double ProductionCompleted { get; set; }
        public double HeatCompleted { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Customer { get; set; }
        public string ManufactureTeam { get; set; }
}
}

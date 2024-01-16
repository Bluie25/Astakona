using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astakona
{
    public class ReturnedOrdersDetails
    {
        public int ReturnedOrderID { get; set; }
        public int OrderId { get; set; }
        public string InvoiceNo { get; set; }
        public string InventoryName { get; set; }
        public double Amount { get; set; }
        public double ProductionCompleted { get; set; }
        public double HTCompleted { get; set; }
        public double Delivered { get; set; }
        public string Customer { get; set; }
        public double ReturnAmount { get; set; }
        public double BigScrewUsed { get; set; }
        public double SmallScrewUsed { get; set; }
        public double Triplek18mmUsed { get; set; }
        public double Triplek15mmUsed { get; set; }
        public double Triplek12mmUsed { get; set; }
        public bool IsFinished { get; set; }
        public double PalletFixed { get; set; }
        public int InventoryID { get; set; }
    }
}

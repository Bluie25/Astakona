using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astakona
{
    public class LoggedInDetails
    {
        public int EmployeeID { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public double Salary { get; set; }
        public bool ManageAccounts { get; set; }
        public bool AccessSalaries { get; set; }
        public bool AddOrder { get; set; }
        public bool UpdateOrder { get; set; }
        public bool DeleteOrder { get; set; }
        public bool AddInventory { get; set; }
        public bool UpdateInventory { get; set; }
        public bool DeleteInventory { get; set; }
        public bool UpdateDelivery { get; set; }
    }
}

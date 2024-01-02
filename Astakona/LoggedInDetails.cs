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
        public bool ManageSalaries { get; set; }
        public bool AddOrder { get; set; }
        public bool UpdateOrder { get; set; }
        public bool DeleteOrder { get; set; }
        public bool AddInventory { get; set; }
        public bool UpdateInventory { get; set; }
        public bool DeleteInventory { get; set; }
        public bool UpdateDelivery { get; set; }
        public bool UpdateMaterial { get; set; }
        public bool AddReturnedOrder { get; set; }
        public bool UpdateReturnedOrder { get; set; }
        public bool DeleteReturnedOrder { get; set; }   
    }
}

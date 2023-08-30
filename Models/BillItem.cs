using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUI_App_Tutorial.Models
{
    public class BillItem
    {
        public long? Id { get; set; }
        public Product? Product { get; set; }
        public long? idTable { get; set; }
        public bool? orderSent { get; set; }
        public double? Quantity { get; set; }
        public double? PredefinedQuantity { get; set; }
        public string? Mention { get; set; }
        public string BillItemText => orderSent == true ? 
            $"{Product.Denumire} | {Quantity} buc. | comanda trimisa" : 
            $"{Product.Denumire} | {Quantity} buc.";
    }
}

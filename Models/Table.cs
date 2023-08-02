using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUI_App_Tutorial.Models
{
    public enum TableStatus
    {
        LIBERA,
        IN_LUCRU,
        NOTA_EMISA
    }
    public class Table
    {
        public int? Id { get; set; }
        public int? IdSala { get; set; }
        public string? Name { get; set; }
        public bool? Occupied { get; set; }
        public int? AcumPeScaun { get; set; }
        public int? IdUser { get; set; }
        public int? IdCopil { get; set; }
        public TableStatus? Status { get; set; }
    }
}

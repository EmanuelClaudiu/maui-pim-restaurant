using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUI_App_Tutorial.Models
{
    public class Product
    {
        public long? Id { get; set; }

        public string? Categorie { get; set; }

        public string? Denumire { get; set; }

        public long? Grupa { get; set; }

        public long? Locatie { get; set; }

        public double? Pret { get; set; }

        public List<PredefinedQuantity>? CantitatiPredefinite { get; set; }
    }
}

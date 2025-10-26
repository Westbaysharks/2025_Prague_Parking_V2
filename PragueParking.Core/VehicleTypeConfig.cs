using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core
{
    // En enkel klass som exakt matchar strukturen för ett fordon
    // i vår settings.json-fil. Detta gör inläsningen smidig.
    public class VehicleTypeConfig
    {
        public string Type { get; set; } = string.Empty;
        public int Size { get; set; }
        public double PricePerHour { get; set; }
    }
}
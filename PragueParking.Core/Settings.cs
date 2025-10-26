using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core
{
    // Matchar huvudstrukturen i settings.json.
    public class Settings
    {
        public int TotalSpots { get; set; } = 100; // Standardvärde
        public int SpotSize { get; set; } = 4;     // Standardstorlek per P-ruta
        public int FreeMinutes { get; set; } = 10;
        public List<VehicleTypeConfig> VehicleTypes { get; set; } = new List<VehicleTypeConfig>();
    }
}

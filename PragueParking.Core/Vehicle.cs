using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PragueParking.Core
{
    // En "abstrakt" basklass kan inte skapas direkt, men den innehåller
    // gemensam kod som alla "barnklasser" (Car, MC, etc.) ärver.
    public abstract class Vehicle : IVehicle
    {
        // Vi använder "properties" med en "set" för att låta
        // Json-serialiseraren sätta värdena när vi läser från fil.
        public string RegNumber { get; set; }
        public int Size { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string VehicleType { get; set; }
        public double PricePerHour { get; set; }

        // Konstruktor som används när vi skapar ett NYTT fordon.
        public Vehicle(string regNumber, VehicleTypeConfig config)
        {
            RegNumber = regNumber.ToUpper(); // Sparar alltid som versaler
            Size = config.Size;
            VehicleType = config.Type;
            PricePerHour = config.PricePerHour;
            ArrivalTime = DateTime.Now; // Sätter ankomsttid till precis nu
        }

        // En parameterlös konstruktor krävs av JSON-serialiseraren
        // när den återskapar objekt från en fil.

        [JsonConstructor]
        public Vehicle()
        {
            RegNumber = string.Empty;
            VehicleType = string.Empty;
        }
    }
}

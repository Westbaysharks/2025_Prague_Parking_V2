using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core
{
    // Bike ärver allt från Vehicle.
    public class Bike : Vehicle
    {
        // Anrop "base(regNumber, config)" skickar värdena vidare
        // till basklassens (Vehicle) konstruktor.
        public Bike(string regNumber, VehicleTypeConfig config) : base(regNumber, config) { }

        // Tom konstruktor för JSON-inläsning.
        public Bike() : base() { }
    }
}
